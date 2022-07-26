using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

// This is considered to be an example of an avatar manager.
// You are encouraged to modify this script to match your application.
//
// This script instantiates avatars when they are tracked and removes them when tracking has finished.
// Optionally, the script can ask CapturyLive to search for Actors whenever no one is being tracked.


namespace Captury
{
	/// <summary>
	/// Instantiates Captury Avatars and handles the user assignment
	/// </summary>
	[RequireComponent(typeof(CapturyNetworkPlugin))]
	public class CapturySimpleAvatarManager : MonoBehaviour
	{
		[System.Serializable]
		public class AvatarMesh
		{
			public GameObject	mesh;
			public Avatar		avatar;
			public Transform	leftHip;
			public Transform	head;
		}

		[SerializeField]
		[Tooltip("The avatar prefabs that will be instantiated once they are tracked.")]
		private AvatarMesh[] avatars;
		private int nextAvatar = -1; // next avatar to spawn

		[SerializeField]
		[Tooltip("Prop prefabs (6 dof) that will be instantiated once they are tracked.")]
		private GameObject[] props;
		private int nextProp = -1; // next prop to spawn

		[SerializeField]
		[Tooltip("Search for a new actor when no one is tracked. The search is performed at the origin of the object this script is attached to.")]
		private bool searchWhenNotTracking = false;
		[SerializeField]
		private float searchEveryXSeconds = 5.0f; // search every 10 seconds
		private float lastSearched;

		[SerializeField]
		private GameObject trackingArea; // if set instantiate avatars as children of this. otherwise they are instantiated as children of the object the manager is attached to.

 		[SerializeField]
 		[Tooltip("When an actor leaves the volume of the parent object tracking is stopped. You need to set a box collider component on the current object.")]
 		private bool stopTrackingWhenOutsideBounds = false;

		//Function to get random number
		private static readonly System.Random randomNumberGenerator = new System.Random();

		/// <summary>
		/// The <see cref="CapturyNetworkPlugin"/> handles the connection to the captury server
		/// </summary>
		private CapturyNetworkPlugin networkPlugin;

		/// <summary>
		/// List of <see cref="CapturySkeleton"/> which will be instantiated in the next Update
		/// </summary>
		private List<CapturySkeleton> newSkeletons = new List<CapturySkeleton>();
		/// <summary>
		/// List of <see cref="CapturySkeleton"/> which will be destroyed in the next Update
		/// </summary>
		private List<CapturySkeleton> lostSkeletons = new List<CapturySkeleton>();

		/// <summary>
		/// List of <see cref="CapturySkeleton"/> which are currently tracked
		/// </summary>
		[SerializeField]
		private List<CapturySkeleton> trackedSkeletons = new List<CapturySkeleton>();

		private BoxCollider boxCollider;

		private void Start()
		{
			networkPlugin = GetComponent<CapturyNetworkPlugin>();

			if (trackingArea)
				boxCollider = trackingArea.GetComponent<BoxCollider>();
			else
				boxCollider = GetComponent<BoxCollider>();
			if (boxCollider == null && stopTrackingWhenOutsideBounds) {
				Debug.LogError("Could not find BoxCollider component. If you want to stop tracking avatars when they get outside bounds attach a BoxCollider component.");
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#endif
			}

			// check the avatar prefabs
			if (avatars == null || avatars.Length == 0) {
				Debug.LogError("No avatar was set. Make sure you assign at least one Avatar prefab to CapturySimpleAvatarManager.avatars");
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
				#endif
			}

			// make sure it searches immediately
			lastSearched = -searchEveryXSeconds - 1.0f;

			// keep the CapturyAvatarManager GameObject between scenes
			DontDestroyOnLoad(gameObject);

			// register for skeleton events
			networkPlugin.SkeletonFound += OnSkeletonFound;
			networkPlugin.SkeletonLost  += OnSkeletonLost;
		}

		private void OnDestroy()
		{
			// unregister from events
			if (networkPlugin != null) {
				networkPlugin.SkeletonFound -= OnSkeletonFound;
				networkPlugin.SkeletonLost  -= OnSkeletonLost;
			}
		}

		private void Update()
		{
			if (boxCollider != null && stopTrackingWhenOutsideBounds) {
				checkBoundsOnAvatars();
			}

			lock (newSkeletons) {
				InstantiateAvatars(newSkeletons);
			}

			lock (lostSkeletons) {
				DestroyAvatars(lostSkeletons);
			}

			if (searchWhenNotTracking && !trackedSkeletons.Any()) {
				float now = Time.time;
				if (now - lastSearched > searchEveryXSeconds) {
					networkPlugin.snapActor(transform.position.x, transform.position.z, 1000.0f);
					lastSearched = now;
					Debug.Log("snapping Actor");
				}
			}
		}

		/// <summary>
		/// Called when a new captury skeleton is found
		/// </summary>
		/// <param name="skeleton"></param>
		private void OnSkeletonFound(CapturySkeleton skeleton)
		{
			Debug.Log("CapturyAvatarManager found skeleton with id " + skeleton.id + " and name " + skeleton.name);

			lock (newSkeletons) {
				newSkeletons.Add(skeleton);
			}
		}

		/// <summary>
		/// Called when a captury skeleton is lost
		/// </summary>
		/// <param name="skeleton"></param>
		private void OnSkeletonLost(CapturySkeleton skeleton)
		{
			Debug.Log("CapturyAvatarManager lost skeleton with id " + skeleton.id + " and name " + skeleton.name);
			lock (lostSkeletons) {
				lostSkeletons.Add(skeleton);
			}
		}

		/// <summary>
		/// Instantiates default avatars for the given list of skeletons
		/// </summary>
		/// <param name="skeletons"></param>
		private void InstantiateAvatars(List<CapturySkeleton> skeletons)
		{
			foreach (CapturySkeleton skel in skeletons) {
				Avatar av;
				GameObject avatar;
				float backLength = -1.0f;
				if (skel.joints.Length < 10) {
					if (props == null || props.Length == 0) {
						Debug.LogError("Cannot spawn prop. Make sure you assign at least one Prop prefab to CapturySimpleAvatarManager.props.");
						#if UNITY_EDITOR
						UnityEditor.EditorApplication.isPlaying = false;
						#endif
						continue;
					} else {
						++nextProp;
						if (nextProp >= props.Length)
							nextProp = 0;
						avatar = Instantiate(props[nextProp], trackingArea ? trackingArea.transform : transform);
						Debug.Log(Time.time + ": Instantiating prop " + nextProp + " for skeleton with id " + skel.id + " and name " + skel.name);
					}
					av = null;
				} else {
					if (avatars == null || avatars.Length == 0) {
						Debug.LogError("Cannot spawn avatar. Make sure you assign at least one Avatar prefab to CapturySimpleAvatarManager.avatars.");
						#if UNITY_EDITOR
						UnityEditor.EditorApplication.isPlaying = false;
						#endif
						continue;
					}
					++nextAvatar;
					if (nextAvatar >= avatars.Length)
						nextAvatar = 0;
					avatar = Instantiate(avatars[nextAvatar].mesh, trackingArea ? trackingArea.transform : transform);
					avatar.name = skel.name + "(" + skel.id + ")";
					av = avatars[nextAvatar].avatar;
					if (avatars[nextAvatar].head && avatars[nextAvatar].leftHip)
						backLength = avatars[nextAvatar].head.position.y - avatars[nextAvatar].leftHip.position.y;
					Debug.Log(Time.time + ": Instantiating avatar " + nextAvatar + " for skeleton with id " + skel.id + " and name " + skel.name);
				}

				avatar.SetActive(true);

				if(skel.target != null) {
					// destroy old avatar
					Destroy(skel.target);
				}

				skel.setTargetSkeleton(avatar, av, backLength);

				trackedSkeletons.Add(skel);
			}
			skeletons.Clear();
		}

		/// <summary>
		/// Destorys avatars for the given list of skeletons
		/// </summary>
		private void DestroyAvatars(List<CapturySkeleton> skeletons)
		{
			foreach (CapturySkeleton skel in skeletons) {
				Debug.Log("Destroying avatar for skeleton with id " + skel.id + " and name " + skel.name);
				Destroy(skel.target);
				skel.setTargetSkeleton(null, null, -1.0f);
				trackedSkeletons.Remove(skel);
			}
			skeletons.Clear();
		}

		// stop tracking a skeleton if it's outside the bounds of the collider
		private void checkBoundsOnAvatars()
		{
			foreach (CapturySkeleton skel in trackedSkeletons) {
				Vector3 pos = transform.TransformPoint(skel.joints[0].transform.position);
				if (!IsPointInsideBox(pos, boxCollider) && !IsPointInsideBox(skel.joints[0].transform.position, boxCollider)) {
					Debug.Log(Time.time + ": Avatar " + skel.id + " is outside bounds. " + pos + "  " + skel.joints[0].transform.position + " Stopping tracking." + boxCollider.bounds.min + " " + boxCollider.bounds.max);
					networkPlugin.stopTracking(skel);
					lostSkeletons.Add(skel);
					// the skeleton will be destroyed by the event callback
				}
			}
		}

		private bool IsPointInsideBox(Vector3 point, BoxCollider box)
		{
			return (point.x > box.bounds.min.x && point.x < box.bounds.max.x &&
					point.y > box.bounds.min.y && point.y < box.bounds.max.y &&
					point.z > box.bounds.min.z && point.z < box.bounds.max.z);
		}
	}
}
