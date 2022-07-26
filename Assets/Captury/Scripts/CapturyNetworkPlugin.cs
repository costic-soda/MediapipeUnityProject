// kate: indent-width 4; tab-width 4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Captury
{
    //==========================================
    // internal structures that are more easy to use
    //==========================================
    [Serializable]
    public class CapturySkeletonJoint
    {
        public string name;
        public int parent;
        public Vector3 offset;
        public Vector3 orientation;
        public Transform transform;
        public Quaternion originalRotation;
    }

    [Serializable]
    public class CapturySkeleton
    {
        public string name;
        public int id;
        public CapturySkeletonJoint[] joints; // joints that are streamed

        public int scalingProgress = 100; // [0 .. 100]
        public int trackingQuality = 100; // [0 .. 100]

        public static CapturyNetworkPlugin networkPlugin;

        public float streamedBackLength = -1.0f;
        public float targetBackLength = -1.0f;

        // reference to game object that gets animated
        // set it with setTargetSkeleton()
        public GameObject target {
            get { return targetSkeleton; }
        }

        // reference to game object that the motion is applied to directly
        // set it with setReferenceSkeleton()
        public GameObject reference {
            get { return referenceSkeleton; }
        }

        // note: CapturySkeleton takes ownership of the passed skeleton and destroys it when it is destroyed
        public void setReferenceSkeleton(GameObject refSkel, Avatar avatar, float backLen)
        {
            if (referenceSkeleton != null) {
                networkPlugin.LogWarning("CapturySkeleton.setReferenceSkeleton() can only be called once");
                return;
            }

            referenceSkeleton = refSkel;
            referenceName = referenceSkeleton.name;

            poseGetter = new HumanPoseHandler(avatar, refSkel.transform);
            foreach (CapturySkeletonJoint j in joints) {
                // check if the joint name matches a reference transform and assign it
                ArrayList children = referenceSkeleton.transform.GetAllChildren();
                foreach (Transform tra in children) {
                    if (tra.name.EndsWith(j.name)) {
                        j.transform = tra;
                        j.originalRotation = tra.rotation;
                        continue;
                    }
                }
            }

            lock (this) {
                streamedBackLength = backLen;

                if (targetSkeleton && targetBackLength > 0.0f && streamedBackLength > 0.0f) {
                    float scale = streamedBackLength / targetBackLength;
                    networkPlugin.Log("Captury: SCALING2 avatar " + id + " by " + scale, refSkel);
                    targetSkeleton.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
        }

        public void setTargetSkeleton(GameObject targetSkel, Avatar avatar, float avatarBackLength)
        {
            targetSkeleton = targetSkel;
            if (targetSkeleton == null) {
                targetName = "";
                return;
            }
            targetName = targetSkeleton.name;

            if (joints.Length == 1) { // rigid object
                joints[0].transform = targetSkeleton.transform;
            }

            // scale skeleton to size of actor
            lock (this) {
                targetBackLength = avatarBackLength;
                if (targetSkeleton) {
                    if (targetBackLength > 0.0f && streamedBackLength > 0.0f) {
                        float scale = streamedBackLength / targetBackLength;
                        networkPlugin.Log("Captury: SCALING1 avatar " + id + " by " + scale, targetSkel);
                        targetSkeleton.transform.localScale = new Vector3(scale, scale, scale);
                    } else {
                        networkPlugin.Log("Captury: NOT SCALING avatar " + id + " streamed " + streamedBackLength + " avatar " + avatarBackLength, targetSkel);
                    }

                    try {
                        poseSetter = new HumanPoseHandler(avatar, targetSkeleton.transform);
                    } catch {
                        networkPlugin.LogError("Captury: the assigned target avatar is not valid. Please make sure the avatar passed to CapturyNetworkPlugin.setTargetSkeleton() is a valid humanoid avatar.", targetSkel);
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        #endif
                    }
                }
            }
        }

        // the pose has been set on the referenceSkeleton already
        // now apply it on the target skeleton
        public void updatePose()
        {
            if (targetSkeleton == null || referenceSkeleton == null || poseGetter == null || poseSetter == null)
                return;

            HumanPose pose = new HumanPose();
            poseGetter.GetHumanPose(ref pose);
            poseSetter.SetHumanPose(ref pose);
        }

        public void deactivate()
        {
            lock (referenceSkeletonsToDestroy) {
                referenceSkeletonsToDestroy.Add(referenceSkeleton);
            }
            referenceSkeleton = null;
        }

        public static void cleanup()
        {
            lock (referenceSkeletonsToDestroy) {
                foreach (GameObject skel in referenceSkeletonsToDestroy)
                    UnityEngine.Object.Destroy(skel);
                referenceSkeletonsToDestroy.Clear();
            }
        }

        ~CapturySkeleton()
        {
            if (referenceSkeleton) {
                lock (referenceSkeletonsToDestroy) {
                    referenceSkeleton.SetActive(false);
                    referenceSkeletonsToDestroy.Add(referenceSkeleton);
                }
            }
        }

        private static List<GameObject> referenceSkeletonsToDestroy = new List<GameObject>();

        private GameObject referenceSkeleton = null; // the reference skeleton
        public String referenceName;
        private HumanPoseHandler poseGetter;
        private GameObject targetSkeleton = null; // the target skeleton
        public String targetName;
        private HumanPoseHandler poseSetter;
    }

    [Serializable]
    public class CapturyMarkerTransform
    {
        public Quaternion rotation;
        public Vector3 translation;
        public Int64 timestamp;
        public float bestAccuracy;
        public bool consumed;
    }

    [Serializable]
    public class ARTag
    {
        public int id;
        public Vector3 translation;
        public Quaternion rotation;
    }

    //=================================
    // define captury class structures
    //=================================
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyJoint
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] name;
        public int parent;
        public float ox, oy, oz;
        public float rx, ry, rz;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyActor
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] name;
        public int id;
        public int numJoints;
        public IntPtr joints;
        public int numBlobs;
        public IntPtr blobs;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyPose
    {
        public Int32 actor;
        public Int64 timestamp;
        public Int32 numValues;
        public IntPtr values;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyARTag
    {
        public int id;
        public float ox, oy, oz; // position
        public float nx, ny, nz; // normal
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CapturyImage
    {
        public int width;
        public int height;
        public int camera;
        public ulong timestamp;
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyTransform
    {
        public float rx; // rotation euler angles
        public float ry;
        public float rz;
        public float tx; // translation
        public float ty;
        public float tz;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CapturyCamera
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] name;
        public int id;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float orientationX;
        public float orientationY;
        public float orientationZ;
        public float sensorWidth;   // in mm
        public float sensorHeight;  // in mm
        public float focalLength;   // in mm
        public float lensCenterX;   // in mm
        public float lensCenterY;   // in mm
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] distortionModel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public float distortion;

        // the following can be computed from the above values and are provided for convenience only
        // the matrices are stored column wise:
        // 0  3  6  9
        // 1  4  7 10
        // 2  5  8 11
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        float extrinsic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        float intrinsic;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CapturyLatencyInfo
    {
        public Int64 firstImagePacketTime;
        public Int64 optimizationStartTime;
        public Int64 optimizationEndTime;
        public Int64 packetSentTime;
        public Int64 packetReceivedTime;
        public Int64 timestampOfCorrespondingPose;
    }

    //====================
    // the network plugin
    //====================
    public class CapturyNetworkPlugin : MonoBehaviour
    {
        public static CapturyNetworkPlugin Instance;
        //=============================================
        // import the functions from RemoteCaptury dll
        //=============================================
        [DllImport("RemoteCaptury")]
        private static extern int Captury_connect(string ip, ushort port);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_disconnect();
        [DllImport("RemoteCaptury")]
        private static extern int Captury_getActors(out IntPtr actorData);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_startStreaming(int what);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_stopStreaming();
        [DllImport("RemoteCaptury")]
        private static extern IntPtr Captury_getCurrentPose(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_freePose(IntPtr pose);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_requestTexture(IntPtr actor);
        [DllImport("RemoteCaptury")]
        private static extern IntPtr Captury_getTexture(IntPtr actor);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_freeImage(IntPtr image);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_setRotationConstraint(int actorId, int jointIndex, IntPtr rotation, Int64 timestamp, float weight);
        [DllImport("RemoteCaptury")]
        private static extern Int64 Captury_getMarkerTransform(IntPtr actor, int jointIndex, IntPtr transform);
        [DllImport("RemoteCaptury")]
        private static extern Int64 Captury_synchronizeTime();
        [DllImport("RemoteCaptury")]
        private static extern Int64 Captury_getTime();
        [DllImport("RemoteCaptury")]
        private static extern Int64 Captury_getTimeOffset();
        [DllImport("RemoteCaptury")]
        private static extern IntPtr Captury_getLastErrorMessage();
        [DllImport("RemoteCaptury")]
        private static extern void Captury_freeErrorMessage(IntPtr msg);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_getCameras(out IntPtr cameras);
        [DllImport("RemoteCaptury")]
        private static extern IntPtr Captury_getCurrentARTags();
        [DllImport("RemoteCaptury")]
        private static extern void Captury_freeARTags(IntPtr arTags);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_snapActor(float x, float z, float heading);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_snapActorEx(float x, float z, float radius, float heading, [MarshalAs(UnmanagedType.LPStr)]string skeletonName, int snapMethod, int quickScaling);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_startTracking(int actorId, float x, float z, float heading);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_getTrackingQuality(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_getScalingProgress(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern int Captury_getBackgroundQuality();
        [DllImport("RemoteCaptury")]
        private static extern int Captury_captureBackground(IntPtr callback, IntPtr userData);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_rescaleActor(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_recolorActor(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_updateActorColors(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_stopTracking(int actorId);
        [DllImport("RemoteCaptury")]
        private static extern void Captury_deleteActor(int actorId);
        [DllImport("RemoteCaptury")]
        //[return: MarshalAs(UnmanagedType.LPStr)]
        private static extern IntPtr Captury_getStatus();
        [DllImport("RemoteCaptury")]
        private static extern void Captury_getCurrentLatency(IntPtr latencyInfo);

        public enum SnapMode { SNAP_BACKGROUND_LOCAL, SNAP_BACKGROUND_GLOBAL, SNAP_BODYPARTS_LOCAL, SNAP_BODYPARTS_GLOBAL, SNAP_BODYPARTS_JOINTS, SNAP_DEFAULT };

        public string host = "127.0.0.1";
        public ushort port = 2101;
        public float scaleFactor = 0.001f; // mm to m
        public int actorCheckTimeout = 500; // in ms
        public bool streamARTags = false;

        // Events
        public delegate void SkeletonDelegate(CapturySkeleton skeleton);
        public event SkeletonDelegate SkeletonFound;
        public event SkeletonDelegate SkeletonLost;
        public event SkeletonDelegate ScalingProgressChanged;
        public delegate void CamerasChangedDelegate(GameObject[] cameras);
        public event CamerasChangedDelegate CamerasChanged;
        public delegate void ARTagsDetectedDelegate(ARTag[] artags);
        public event ARTagsDetectedDelegate ARTagsDetected;

        private Vector3[]    cameraPositions;
        private Quaternion[] cameraOrientations;
        private float[]      cameraFieldOfViews;
        public GameObject[]  cameras;

        public ARTag[] arTags = new ARTag[0];

        public Text log;
        private String asyncLog = "";

        public GameObject streamedSkeleton; // this is used to set CapturySkeleton.setReferenceSkeleton()
        public Avatar streamedAvatar;
        public String streamedSkeletonLeftHip = "LeftUpLeg";
        public String streamedSkeletonHead = "Head";

        /// <summary>
        /// set the offset in world coordinates by moving the object the
        /// </summary>
        private Vector3 worldPosition = Vector3.zero;
        private Quaternion worldRotation = Quaternion.identity;

        // threading data for communication with server
        private Thread communicationThread;
        private Mutex communicationMutex = new Mutex();
        private bool communicationFinished = false;

        // internal variables
        private bool isConnected = false;
        private bool isSetup = false;

        // skeleton data from Captury
        private Dictionary<int, int> actorFound = new Dictionary<int, int>();
        private Dictionary<int, CapturySkeleton> skeletons = new Dictionary<int, CapturySkeleton>();
        private Dictionary<string, int> jointsWithConstraints = new Dictionary<string, int>();

        // for debugging latency
        public bool measureLatency = false;
        public struct Timestamps {
            public Int64                pose;
            public Int64                update;
            public CapturyLatencyInfo   latencyInfo;
            public Timestamps(Int64 p, Int64 up, CapturyLatencyInfo li)
            {
                pose = p;
                update = up;
                latencyInfo = li;
            }
        };

        public Dictionary<int, Timestamps> timestampsForPoses = new Dictionary<int, Timestamps>();
        private IntPtr latencyBuffer = IntPtr.Zero;

        private static System.Threading.Thread mainThread;

        void Awake()
        {
            mainThread = System.Threading.Thread.CurrentThread;
            // try to set retargeter if available
            CapturySkeleton.networkPlugin = this;
            if (CapturyNetworkPlugin.Instance == null) {
                CapturyNetworkPlugin.Instance = this;
            }

            if (streamedAvatar == null || !streamedAvatar.isHuman || !streamedAvatar.isValid)
                LogError("CapturyNetworkPlugin.streamedAvatar must be set and humanoid.");
        }

        //=============================
        // this is run once at startup
        //=============================
        void Start()
        {
            worldPosition = transform.position;
            worldRotation = transform.rotation;

            // start the connection thread
            communicationThread = new Thread(lookForActors);
            communicationThread.Start();
        }

        //==========================
        // this is run once at exit
        //==========================
        void OnDisable()
        {
            Log("Captury: disabling network plugin");
            communicationFinished = true;
            communicationThread.Join();
        }

        //============================
        // this is run once per frame
        //============================
        void Update()
        {
            // only perform if we are actually connected
            if (!isConnected)
                return;

            worldPosition = transform.position;
            worldRotation = transform.rotation;

            Int64 before = Captury_getTime();

            // make sure we lock access before doing anything
            //            Log("Captury: Starting pose update...");
            communicationMutex.WaitOne();

            // fetch current pose for all skeletons
            foreach (KeyValuePair<int, CapturySkeleton> kvp in skeletons) {
                // get the actor id
                int actorId = kvp.Key;

                // skeleton does not have a reference yet. set it.
                if (!skeletons[actorId].reference) {
                    float lHipY = -1.0f;
                    float headY = -1.0f;
                    float backLen = -1.0f;
                    foreach (CapturySkeletonJoint j in kvp.Value.joints) {
                        if (j.name == streamedSkeletonLeftHip) {
                            lHipY = j.offset.y;
                            for (int i = j.parent; i != -1; i = kvp.Value.joints[i].parent)
                                lHipY += kvp.Value.joints[i].offset.y;
                        } else if (j.name == streamedSkeletonHead) {
                            headY = j.offset.y;
                            for (int i = j.parent; i != -1; i = kvp.Value.joints[i].parent)
                                headY += kvp.Value.joints[i].offset.y;
                        }
                    }
                    backLen = (lHipY != -1.0f && headY != -1.0f) ? (headY - lHipY) * 0.001f : -1.0f;
                    Log("Captury: streamed back length " + backLen);
                    // GameObject refSkel = new GameObject();
                    // Avatar av = CreateAvatar(skeletons[actorId], ref refSkel);
                    // skeletons[actorId].setReferenceSkeleton(refSkel, av, backLen);
                    GameObject refSkel = Instantiate(streamedSkeleton, null);
                    skeletons[actorId].setReferenceSkeleton(refSkel, streamedAvatar, backLen);
                    refSkel.transform.SetParent(transform);
                    dumpSkeletons();
#if DEBUG_VISUALS
                    refSkel.SetActive(true);
#endif
                }

                // get pointer to pose
                IntPtr poseData = Captury_getCurrentPose(actorId);

                // check if we actually got data, if not, continue
                if (poseData == IntPtr.Zero) {
                    // something went wrong, get error message
                    IntPtr msg = Captury_getLastErrorMessage();
                    string errmsg = Marshal.PtrToStringAnsi(msg);
                    Log("Captury: Stream error " + actorId + ": " + errmsg);
                    //Captury_freeErrorMessage(msg);
                } else {
                    // convert the pose
                    CapturyPose pose;
                    pose = (CapturyPose)Marshal.PtrToStructure(poseData, typeof(CapturyPose));

                    // store timestamp stats for measuring latency
                    if (measureLatency) {
                        Int64 now = Captury_getTime();
                        if (latencyBuffer == IntPtr.Zero)
                            latencyBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CapturyLatencyInfo)));
                        Captury_getCurrentLatency(latencyBuffer);
                        CapturyLatencyInfo latencyInfo = new CapturyLatencyInfo();
                        latencyInfo = (CapturyLatencyInfo)Marshal.PtrToStructure(latencyBuffer, typeof(CapturyLatencyInfo));
                        if (latencyInfo.timestampOfCorrespondingPose == pose.timestamp) {
                            timestampsForPoses[pose.actor] = new Timestamps(pose.timestamp, now, latencyInfo);
                            // Log("Captury: got latency: img: " + (latencyInfo.optimizationStartTime - latencyInfo.firstImagePacketTime) + " opt: " + (latencyInfo.optimizationEndTime - latencyInfo.optimizationStartTime) + " net: " + (latencyInfo.packetReceivedTime - latencyInfo.packetSentTime) + " ->unity: " + (now - latencyInfo.packetReceivedTime) + " total: " + (now - latencyInfo.firstImagePacketTime));
                            Captury_synchronizeTime();
                        }
                    }

                    // copy the data into a float array
                    float[] values = new float[pose.numValues * 6];
                    Marshal.Copy(pose.values, values, 0, pose.numValues * 6);

                    // int id = pose.actor;
                    // Log("Captury: id " + id + " " + actorId + ": " + values[0] + " " + values[2] + " " + skeletons.Count + " " + actorId);

                    Vector3 pos = new Vector3();
                    Vector3 rot = new Vector3();

                    // directly update pose of reference skeleton
                    for (int jointID = 0; jointID < Math.Min(skeletons[actorId].joints.Length, pose.numValues); jointID++) {
                        // ignore any joints that do not map to a transform
                        if (skeletons[actorId].joints[jointID].transform == null) {
                            // Log("Captury: skipping joint " + jointID + " " + skeletons[actorId].joints[jointID].name);
                            continue;
                        }

                        Quaternion orig = skeletons[actorId].joints[jointID].originalRotation;

                        // Log("Captury: at joint " + jointID + " " + skeletons[actorId].joints[jointID].name + " " + ConvertRotation(rot));

                        // set offset and rotation
                        int baseIndex = jointID * 6;
                        pos.Set(values[baseIndex + 0], values[baseIndex + 1], values[baseIndex + 2]);
                        skeletons[actorId].joints[jointID].transform.position = ConvertPosition(pos);
                        rot.Set(values[baseIndex + 3], values[baseIndex + 4], values[baseIndex + 5]);
                        skeletons[actorId].joints[jointID].transform.rotation = ConvertRotation(rot) * orig;
                    }

                    skeletons[actorId].updatePose();

                    // finally, free the pose data again
                    Captury_freePose(poseData);

                    skeletons[actorId].trackingQuality = Captury_getTrackingQuality(actorId);

                    int scaling = Captury_getScalingProgress(actorId);
                    skeletons[actorId].scalingProgress = scaling;

                    if (scaling != skeletons[actorId].scalingProgress && ScalingProgressChanged != null)
                        ScalingProgressChanged(skeletons[actorId]);
                }
            }

            if (cameras != null && cameraPositions != null && cameras.Length != cameraPositions.Length) {
                cameras = new GameObject[cameraPositions.Length];
                for (int i = 0; i < cameraPositions.Length; ++i) {
                    cameras[i] = new GameObject("Camera " + (i+1));
                    cameras[i].transform.SetParent(transform);
                    cameras[i].AddComponent(typeof(Camera));
                    cameras[i].SetActive(false);
                    cameras[i].transform.position = cameraPositions[i];
                    cameras[i].transform.rotation = cameraOrientations[i];
                    Camera cam = cameras[i].GetComponent(typeof(Camera)) as Camera;
                    cam.fieldOfView = cameraFieldOfViews[i];
                }
                // Fire cameras changed event
                if (CamerasChanged != null)
                    CamerasChanged(cameras);
            }

            // get artags
            IntPtr arTagData = Captury_getCurrentARTags();

            // check if we actually got data, if not, continue
            if (arTagData == IntPtr.Zero) {
                // something went wrong, get error message
                //IntPtr msg = Captury_getLastErrorMessage();
                //string errmsg = Marshal.PtrToStringAnsi(msg);
                //Captury_freeErrorMessage(msg);
            } else {
                IntPtr at = arTagData;
                int num;
                for (num = 0; num < 100; ++num) {
                    CapturyARTag arTag = (CapturyARTag)Marshal.PtrToStructure(at, typeof(CapturyARTag));
                    if (arTag.id == -1)
                        break;
                    Array.Resize(ref arTags, num + 1);
                    arTags[num] = new ARTag();
                    arTags[num].id = arTag.id;
                    arTags[num].translation = ConvertPosition(new Vector3(arTag.ox, arTag.oy, arTag.oz));
                    arTags[num].rotation = ConvertRotation(Quaternion.LookRotation(new Vector3(arTag.nx, arTag.ny, arTag.nz)).eulerAngles);
                    at = new IntPtr(at.ToInt64() + Marshal.SizeOf(typeof(CapturyARTag)));
                }
                if (num != 0 && ARTagsDetected != null)
                    ARTagsDetected(arTags);
                else
                    Array.Resize(ref arTags, 0);

                Captury_freeARTags(arTagData);
            }

            communicationMutex.ReleaseMutex();

            CapturySkeleton.cleanup();

            lock(asyncLog) {
                if (log)
                    log.text += asyncLog;
                asyncLog = "";
            }
        }

        //================================================
        // This function continously looks for new actors
        // It runs in a separate thread
        //================================================
        void lookForActors()
        {
            try {

                while (!communicationFinished) {
                    // wait for actorCheckTimeout ms before continuing
                    Thread.Sleep(actorCheckTimeout);

                    // try to connect to captury live
                    if (!isSetup) {
                        if (Captury_connect(host, port) == 1 && Captury_synchronizeTime() != 0) {
                            isSetup = true;
                            Log("Captury: Successfully opened port to Captury Live");
                            Log("Captury: The time difference is " + Captury_getTimeOffset());
                        } else
                            Log(String.Format("Captury: Unable to connect to Captury Live at {0}:{1} ", host, port));

                        IntPtr cameraData = IntPtr.Zero;
                        int numCameras = Captury_getCameras(out cameraData);
                        if (numCameras > 0 && cameraData != IntPtr.Zero) {
                            cameraPositions = new Vector3[numCameras];
                            cameraOrientations = new Quaternion[numCameras];
                            cameraFieldOfViews = new float[numCameras];
                            int szStruct = Marshal.SizeOf(typeof(CapturyCamera)) + 192; // this offset is here to take care of implicit padding
                            for (uint i = 0; i < numCameras; i++) {
                                CapturyCamera camera = new CapturyCamera();
                                camera = (CapturyCamera)Marshal.PtrToStructure(new IntPtr(cameraData.ToInt64() + (szStruct * i)), typeof(CapturyCamera));
                                cameraPositions[i] = ConvertPosition(new Vector3(camera.positionX, camera.positionY, camera.positionZ));
                                cameraOrientations[i] = ConvertRotation(new Vector3(camera.orientationX, camera.orientationY, camera.orientationZ)) * new Quaternion(0, 0.7071f, 0, 0.7071f);
                                cameraFieldOfViews[i] = (float) (Math.Atan2(camera.focalLength, 0.5f * camera.sensorWidth) * (camera.sensorHeight / camera.sensorWidth) * 2 * 180 / Math.PI);
                            }
                        }
                    }
                    if (isSetup) {
                        // get actors
                        IntPtr actorData = IntPtr.Zero;
                        int numActors = Captury_getActors(out actorData);
                        if (numActors > 0 && actorData != IntPtr.Zero) {
                            if (numActors != skeletons.Count)
                                Log(String.Format("Captury: Received {0} actors", numActors));

                            // create actor struct
                            int szStruct = Marshal.SizeOf(typeof(CapturyActor))+4; // implicit padding
                            for (uint i = 0; i < numActors; i++) {
                                // get an actor
                                CapturyActor actor = new CapturyActor();
                                actor = (CapturyActor)Marshal.PtrToStructure(new IntPtr(actorData.ToInt64() + (szStruct * i)), typeof(CapturyActor));

                                // check if we already have it in our dictionary
                                if (skeletons.ContainsKey(actor.id)) { // access to actors does not need to be locked here because the other thread is read-only
                                    actorFound[actor.id] = 2;
                                    continue;
                                }
                                Log("Captury: Found new actor " + actor.id);

                                // no? we need to convert it
                                CapturySkeleton skeleton = new CapturySkeleton();
                                ConvertActor(actor, actorData, ref skeleton);

                                if (SkeletonFound != null)
                                    SkeletonFound(skeleton);

                                //  and add it to the list of actors we are processing, making sure this is secured by the mutex
                                communicationMutex.WaitOne();
                                skeletons.Add(actor.id, skeleton);
                                actorFound.Add(actor.id, 2);
                                communicationMutex.ReleaseMutex();

                                // dumpSkeletons();
                            }
                        }

                        if (!isConnected) {
                            if (Captury_startStreaming(0x11 | (streamARTags ? 0x04 : 0) | (measureLatency ? 0x40 : 0)) == 1) {
                                Log("Captury: Successfully started streaming data");
                                isConnected = true;
                            } else
                                LogWarning("Captury: failed to start streaming");
                        }

                        // reduce the actor countdown by one for each actor
                        int[] keys = new int[actorFound.Keys.Count];
                        actorFound.Keys.CopyTo(keys, 0);
                        foreach (int key in keys)
                            actorFound[key]--;
                    }

                    // remove all actors that were not found in the past few actor checks
                    communicationMutex.WaitOne();
                    List<int> unusedKeys = new List<int>();
                    foreach (KeyValuePair<int, int> kvp in actorFound) {
                        if (kvp.Value > 0)
                            continue;

                        if (SkeletonLost != null) {
                            // dumpSkeletons();
                            Log("Captury: lost skeleton " + kvp.Key + ". telling all my friends.");
                            SkeletonLost(skeletons[kvp.Key]);
                        }

                        skeletons[kvp.Key].deactivate();

                        // remove actor
                        skeletons.Remove(kvp.Key);
                        unusedKeys.Add(kvp.Key);
                    }
                    communicationMutex.ReleaseMutex();

                    // clear out actorfound structure
                    foreach (int key in unusedKeys)
                        actorFound.Remove(key);
                }

                Log("Captury: Disconnecting");
                // make sure we disconnect
                Captury_disconnect();
                isSetup = false;
                isConnected = false;
            } catch (DllNotFoundException) {
                LogError("Captury: RemoteCaptury.dll/libRemoteCaptury.so could not be loaded");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            } catch (EntryPointNotFoundException ex) {
                LogError("Captury: RemoteCaptury.dll/libRemoteCaptury.so does not provide symbol: " + ex.Message);
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }
        }

        // if heading > 360 heading is considered unknown
        public void snapActor(float x, float z, float radius, float heading = 720, string name = "", SnapMode snapMethod = SnapMode.SNAP_DEFAULT, bool quickScaling = false)
        {
            Captury_snapActorEx(x, z, radius, heading, name, (int)snapMethod,  quickScaling? 1: 0);
        }

        public void rescaleActor(CapturySkeleton skel)
        {
            Captury_rescaleActor(skel.id);
        }

        public void recolorActor(CapturySkeleton skel)
        {
            Captury_recolorActor(skel.id);
        }

        public void updateActorColors(CapturySkeleton skel)
        {
            Captury_updateActorColors(skel.id);
        }

        public void stopTracking(CapturySkeleton skel)
        {
            Captury_stopTracking(skel.id);
            // if (skeletons.ContainsKey(skel.id)) {
            //     if (SkeletonLost != null) {
            //         Log("Captury: Stopping to track skeleton. Telling all my friends.");
            //         SkeletonLost(skel);
            //     }

            //     // remove actor
            //     actorPointers.Remove(skel.id);
            //     skeletons.Remove(skel.id);
            // }
        }

        public void deleteActor(CapturySkeleton skel)
        {
            // dumpSkeletons();

            Log("Captury: deleting skeleton " + skel.id);
            Captury_deleteActor(skel.id);

            // if (skeletons.ContainsKey(skel.id)) {
            //     if (SkeletonLost != null) {
            //         Log("Captury: Deleting skeleton. Telling all my friends.");
            //         SkeletonLost(skel);
            //     }

            //     // remove actor
            //     actorPointers.Remove(skel.id);
            //     skeletons.Remove(skel.id);
            // }
        }

        public int getBackgroundQuality()
        {
            return Captury_getBackgroundQuality();
        }

        public void captureBackground()
        {
            Captury_captureBackground(IntPtr.Zero, IntPtr.Zero);
        }

        public string getCapturyLiveStatus()
        {
            return  Marshal.PtrToStringAnsi(Captury_getStatus());
        }

        public void setRotationConstraint(int id, string jointName, Transform t)
        {
            if (skeletons.ContainsKey(id))
            {
                Log("Cannot set rotation for " + jointName + ": no skeleton with id " + id);
                return;
            }
            else
                Log("Captury: Set " + jointName + "-rotation to " + t);
            communicationMutex.WaitOne();
            CapturySkeleton skel = skeletons[id];
            communicationMutex.ReleaseMutex();

            int index;
            if (jointsWithConstraints.ContainsKey(jointName))
                index = jointsWithConstraints[jointName];
            else
            {
                index = 0;
                foreach (CapturySkeletonJoint j in skel.joints)
                {
                    if (j.name == jointName)
                        break;
                    ++index;
                }
                if (index == skel.joints.Length)
                {
                    Log("Cannot set constraint for joint " + jointName + ": no such joint");
                    return;
                }
            }

            //        CapturySkeletonJoint jnt = skel.joints[index];
            Vector3 euler = ConvertToEulerAngles(ConvertRotationToLive(t.rotation));
            IntPtr rotation = Marshal.AllocHGlobal(12);
            Marshal.StructureToPtr(euler, rotation, false);
            Captury_setRotationConstraint(id, index, rotation, Captury_getTime(), 1.0f);
            Marshal.FreeHGlobal(rotation);
        }

        //===============================================
        // helper function to map an actor to a skeleton
        //===============================================
        private void ConvertActor(CapturyActor actor, IntPtr actorData, ref CapturySkeleton skel)
        {
            if (skel == null)
            {
                Log("Captury: Null skeleton reference");
                return;
            }

            // copy data over
            skel.name = System.Text.Encoding.UTF8.GetString(actor.name);
            skel.name = skel.name.Remove(skel.name.IndexOf('\0'));
            skel.id = actor.id;
            //skel.rawData = actorData;

            int[] parents = new int[actor.numJoints];
            parents[0] = -1; // hips
            parents[1] = 0; // spine
            parents[2] = 1; // spine1
            parents[3] = 2; // spine2
            parents[4] = 3; // spine3
            parents[5] = 4; // spine4
            parents[6] = 5; // neck
            parents[7] = 6; // head
            parents[8] = 7; // headee
            parents[9] = 4; // leftshoulder
            parents[10] = 9; // leftarm
            parents[11] = 10; // leftforearm
            parents[12] = 11; // lefthand
            parents[13] = 12; // lefthandee
            parents[14] = 4; // rightshoulder
            parents[15] = 14; // rightarm
            parents[16] = 15; // rightforearm
            parents[17] = 16; // righthand
            parents[18] = 17; // righthandee
            parents[19] = 0; // leftupleg
            parents[20] = 19; // leftleg
            parents[21] = 20; // leftfoot
            parents[22] = 21; // lefttoebase
            parents[23] = 22; // leftfootee
            parents[24] = 0; // rightupleg
            parents[25] = 24; // rightleg
            parents[26] = 25; // rightfoot
            parents[27] = 26; // righttoebase
            parents[28] = 27; // rightfootee

            // create joints
            int szStruct = Marshal.SizeOf(typeof(CapturyJoint));
            skel.joints = new CapturySkeletonJoint[actor.numJoints];
            for (uint i = 0; i < actor.numJoints; i++)
            {
                // marshall the joints into a new joint struct
                CapturyJoint joint = new CapturyJoint();
                joint = (CapturyJoint)Marshal.PtrToStructure(new IntPtr(actor.joints.ToInt64() + (szStruct * i)), typeof(CapturyJoint));

                skel.joints[i] = new CapturySkeletonJoint();
                skel.joints[i].name = System.Text.Encoding.ASCII.GetString(joint.name);
                int jpos = skel.joints[i].name.IndexOf("\0");
                skel.joints[i].name = skel.joints[i].name.Substring(0, jpos);
                skel.joints[i].offset.Set(joint.ox, joint.oy, joint.oz);
                skel.joints[i].orientation.Set(joint.rx, joint.ry, joint.rz);
                skel.joints[i].parent = parents[i];

                //Log("Captury: Got joint " + skel.joints[i].name + " at " + joint.ox + joint.oy + joint.oz);
            }
        }

        // helper function for debugging
        // print the currently known skeletons
        public void dumpSkeletons()
        {
            // debug deleting
            foreach (KeyValuePair<int, CapturySkeleton> kvp in skeletons) {
                int actorId = kvp.Key;
                CapturySkeleton skel = kvp.Value;
                Log("Captury: skeleton " + actorId + " " + skel.name + " " + (skel.reference ? (" ref: " + skel.referenceName) : "no reference") + " " + (skel.target ? (" tgt: " + skel.targetName) : " no target"), skel.reference);
            }
        }

        //===============================================
        // helper function to map an actor to a skeleton
        //===============================================
        private Avatar CreateAvatar(CapturySkeleton skel, ref GameObject root)
        {
            if (skel == null)
            {
                LogWarning("Captury: Null skeleton reference");
                return null;
            }

            int numJoints = skel.joints.Length;

            HumanDescription desc = new HumanDescription();
            desc.skeleton = new SkeletonBone[numJoints+1];
            desc.human = new HumanBone[numJoints+1];
            string[] mecanimNames = HumanTrait.BoneName;
            // GameObject master = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject[] hierarchy = new GameObject[numJoints];

            string[] mecNames = new string[numJoints];
            mecNames[0] = "Hips"; // hips
            mecNames[1] = "Spine"; // spine
            mecNames[2] = "Chest"; // spine1
            mecNames[3] = ""; // spine2
            mecNames[4] = "UpperChest"; // spine3
            mecNames[5] = ""; // spine4
            mecNames[6] = "Neck"; // neck
            mecNames[7] = "Head"; // head
            mecNames[8] = ""; // headee
            mecNames[9] = "LeftShoulder"; // leftshoulder
            mecNames[10] = "LeftUpperArm"; // leftarm
            mecNames[11] = "LeftLowerArm"; // leftforearm
            mecNames[12] = "LeftHand"; // lefthand
            mecNames[13] = ""; // lefthandee
            mecNames[14] = "RightShoulder"; // rightshoulder
            mecNames[15] = "RightUpperArm"; // rightarm
            mecNames[16] = "RightLowerArm"; // rightforearm
            mecNames[17] = "RightHand"; // righthand
            mecNames[18] = ""; // righthandee
            mecNames[19] = "LeftUpperLeg"; // leftupleg
            mecNames[20] = "LeftLowerLeg"; // leftleg
            mecNames[21] = "LeftFoot"; // leftfoot
            mecNames[22] = "LeftToes"; // lefttoebase
            mecNames[23] = ""; // leftfootee
            mecNames[24] = "RightUpperLeg"; // rightupleg
            mecNames[25] = "RightLowerLeg"; // rightleg
            mecNames[26] = "RightFoot"; // rightfoot
            mecNames[27] = "RightToes"; // righttoebase
            mecNames[28] = ""; // rightfootee

            root.name = "Root";

            // create joints
            for (uint i = 0, n = 1; i < numJoints; i++)
            {
                //
                desc.skeleton[i+1].name = skel.joints[i].name;
                desc.skeleton[i+1].position = skel.joints[i].offset * 0.001f;
                desc.skeleton[i+1].rotation = Quaternion.identity;
                desc.skeleton[i+1].scale = Vector3.one;

                if (mecNames[i].Length != 0) {
                    desc.human[n].boneName = skel.joints[i].name;
                    desc.human[n].humanName = mecNames[i];
                    desc.human[n].limit.useDefaultValues = true;
                    ++n;
                }

                hierarchy[i] = new GameObject();
                // hierarchy[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // hierarchy[i].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                if (i > 0 && skel.joints[i].parent >= 0)
                    hierarchy[i].transform.SetParent(hierarchy[skel.joints[i].parent].transform);
                else
                    hierarchy[i].transform.SetParent(root.transform);
                hierarchy[i].transform.position = hierarchy[i].transform.parent.transform.position + skel.joints[i].offset * 0.001f;
                hierarchy[i].name = skel.joints[i].name;

                //Log("Captury: Got joint " + skel.joints[i].name + " at " + joint.ox + joint.oy + joint.oz);
            }

            desc.human[0].boneName = "Root";
            desc.human[0].humanName = "Root";
            desc.human[0].limit.useDefaultValues = true;

            desc.skeleton[0].name = "Root";
            desc.skeleton[0].position = Vector3.zero;
            desc.skeleton[0].rotation = Quaternion.identity;
            desc.skeleton[0].scale = Vector3.one;
            // hierarchy[0].transform.SetParent(hierarchy[0].transform);

            Avatar av = AvatarBuilder.BuildHumanAvatar(root, desc);
            if (av.isValid) {
                Log("created valid avatar from thin air");
                return av;
            } else {
                Log("Captury: failed to create avatar");
                return null;
            }
        }

        public Int64 getTime()
        {
            return (Int64)Captury_getTime();
        }

        //========================================================================================================
        // Helper function to convert a position from a right-handed to left-handed coordinate system (both Y-up)
        //========================================================================================================
        public Vector3 ConvertPosition(Vector3 position)
        {
            position.x *= scaleFactor;
            position.y *= scaleFactor;
            position.z *= scaleFactor;
            return worldRotation * new Vector3 (position.z, position.y, position.x) + worldPosition;
        }

        //========================================================================================================
        // Helper function to convert a position from a left-handed to right-handed coordinate system (both Y-up)
        //========================================================================================================
        public Vector3 ConvertPositionToLive(Vector3 position)
        {
            position.x /= scaleFactor;
            position.y /= scaleFactor;
            position.z /= scaleFactor;
            return Quaternion.Inverse(worldRotation) * (new Vector3(position.z, position.y, position.x) - worldPosition);
        }

        //===========================================================================================================================
        // Helper function to convert a rotation from a right-handed Captury Live to left-handed Unity coordinate system (both Y-up)
        //===========================================================================================================================
        public Quaternion ConvertRotation(Vector3 rotation)
        {
            Quaternion qx = Quaternion.AngleAxis(rotation.x, Vector3.back);
            Quaternion qy = Quaternion.AngleAxis(rotation.y, Vector3.down);
            Quaternion qz = Quaternion.AngleAxis(rotation.z, Vector3.left);
            Quaternion qq = qz * qy * qx;
            return worldRotation * qq;
        }

        //===========================================================================================================
        // Helper function to convert a rotation from Unity back to Captury Live (left-handed to right-handed, Y-up)
        //===========================================================================================================
        public static Quaternion ConvertRotationToLive(Quaternion rotation)
        {
            Vector3 angles = rotation.eulerAngles;

            Quaternion qx = Quaternion.AngleAxis(angles.x, Vector3.back);
            Quaternion qy = Quaternion.AngleAxis(angles.y, Vector3.down);
            Quaternion qz = Quaternion.AngleAxis(angles.z, Vector3.left);
            Quaternion qq = qz * qy * qx;
            return qq;
        }

        //=============================================================================
        // Helper function to convert a rotation to the Euler angles Captury Live uses
        //=============================================================================
        public static Vector3 ConvertToEulerAngles(Quaternion quat)
        {
            const float RAD2DEGf = 57.29577951308232088f;
            Vector3 euler = new Vector3();
            float sqw = quat.w * quat.w;
            float sqx = quat.x * quat.x;
            float sqy = quat.y * quat.y;
            float sqz = quat.z * quat.z;
            float tmp1 = quat.x * quat.y;
            float tmp2 = quat.w * quat.z;
            euler[1] = (float)-Math.Asin(Math.Min(Math.Max(2.0 * (quat.x*quat.z - quat.y*quat.w), -1.0f), 1.0f));
            float C = (float)Math.Cos(euler[1]);
            if (Math.Abs(C) > 0.005)
            {
                euler[2] = (float)Math.Atan2(2.0 * (quat.x*quat.y + quat.z*quat.w) / C, ( sqx - sqy - sqz + sqw) / C) * RAD2DEGf;
                euler[0] = (float)Math.Atan2(2.0 * (quat.y*quat.z + quat.x*quat.w) / C, (-sqx - sqy + sqz + sqw) / C) * RAD2DEGf;
            }
            else
            {
                euler[2] = 0;
                if ((tmp1 - tmp2) < 0)
                    euler[0] = (float)Math.Atan2((quat.x*quat.y - quat.z*quat.w) - (quat.y*quat.z - quat.x*quat.w), ((-sqx + sqy - sqz + sqw) + 2.0 * (quat.x*quat.z + quat.y*quat.w))*0.5f) * RAD2DEGf;
                else
                    euler[0] = (float)Math.Atan2((quat.x*quat.y - quat.z*quat.w) + (quat.y*quat.z - quat.x*quat.w), ((-sqx + sqy - sqz + sqw) - 2.0 * (quat.x*quat.z + quat.y*quat.w))*0.5f) * RAD2DEGf;
            }
            euler[1] *= RAD2DEGf;

            if (Double.IsNaN(euler[0]) || Double.IsNaN(euler[1]) || Double.IsNaN(euler[2]))
                return euler;

            return euler;
        }

        public bool IsConnected {
            get { return isConnected; }
        }

        public void Log(String msg)
        {
            Debug.Log(msg);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text += "\n" + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\n" + msg;
            }
        }

        public void Log(String msg, UnityEngine.Object context)
        {
            Debug.Log(msg, context);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text += "\n" + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\n" + msg;
            }
        }

        public void LogWarning(String msg)
        {
            Debug.LogWarning(msg);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text += "\nWarning: " + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\nWarning: " + msg;
            }
        }

        public void LogWarning(String msg, UnityEngine.Object context)
        {
            Debug.LogWarning(msg, context);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text += "\nWarning: " + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\nWarning: " + msg;
            }
        }

        public void LogError(String msg)
        {
            Debug.LogError(msg);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text = "\nError: " + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\nError: " + msg;
            }
        }

        public void LogError(String msg, UnityEngine.Object context)
        {
            Debug.LogError(msg, context);
            if (log && mainThread.Equals(System.Threading.Thread.CurrentThread))
                log.text = "\nError: " + msg;
            else {
                lock (asyncLog)
                    asyncLog += "\nError: " + msg;
            }
        }
    }

    //==========================================================================
    // Helper extension function to get all children from a specified transform
    //==========================================================================
    public static class TransformExtension
    {
        public static ArrayList GetAllChildren(this Transform transform)
        {
            ArrayList children = new ArrayList();
            foreach (Transform child in transform)
            {
                children.Add(child);
                children.AddRange(GetAllChildren(child));
            }
            return children;
        }
    }
}
