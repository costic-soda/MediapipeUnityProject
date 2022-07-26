

* Getting Started *

1. Attach the CapturyNetworkPlugin and the CapturySimpleAvatarManager scripts to an object. Also make sure that RemoteCaptury.dll is in the project. Tracking data will be relative to this object. So you can easily move it in your scene or realize elevators, conveyor belts or use it to calibrate the tracking volume in the scene by moving the parent object around.

2. Assign the IP address of CapturyLive to the member "host" of CapturyNetworkPlugin. Port should be 2101.

3. Assign Actor with Head (Hands) to the Streamed Skeleton Member and the corresponding humanoid avatar to Streamed Avatar. Also type in the names of the bone that corresponds to the base of the skull of the streamed skeleton (Head) and the left hip (LeftUpLeg).

4. Make sure Actor with Head (Hands) is in the T-pose and is looking in -Z direction. At the moment a 90 degree rotation around the Y axis is necessary.

5. Assign the avatars that are supposed to be animated to the Avatars member of the CapturySimpleAvatarManager component.

When the project is started the avatar will be instantiated as soon as a person is tracked by CapturyLive and disappear when tracking is stopped.


In addition, if there is a BoxCollider attached to the object as well then this is interpreted as the valid tracking volume. As soon as a skeleton leaves the volume, tracking of that skeleton is stopped. This behavior is only enabled when stopTrackingWhenOutsideBounds is true.





* Hacking *

The CapturyNetworkPlugin.cs should not be modified. It handles the communication with CapturyLive via RemoteCaptury.dll. It provides two events SkeletonFound and SkeletonLost. These will be called when a new skeleton is found or when a skeleton is lost. It is marked as DontDestroyOnLoad so that it survives scene loading and only a single instance of the script should be running. It also provides some functions for controlling tracking:

public void snapActor(float x, float z, float heading = 720)
tries to find a skeleton at the given location. If heading is > 360 it is assumed that the orientation is not known. Otherwise this is the direction the person should be looking in (in degrees). Zero means along the x-axis, 90 degrees means along the z-axis.

public void stopTracking(CapturySkeleton skel)
stops tracking the given skeleton

public void deleteActor(CapturySkeleton skel)
stops tracking the given skeleton and deletes the corresponding data in CapturyLive (on the server)

A lot more functions are available for remote control of CapturyLive. Check the public functions in CapturyNetworkPlugin.cs.


CapturySimpleAvatarManager is a sample implementation that instantiates the attached avatar when the SkeletonFound event comes in and removes it when SkeletonLost is called. This procedure has to be done in two stages because SkeletonFound and SkeletonLost are called from a different thread and GameObjects must be instantiated during Update().

The current implementation assumes that CapturySimpleAvatarManager is attached to the same object as CapturyNetworkPlugin. You could easily drop that restriction by using GameObject.Find(...) rather than GetComponent(CapturyNetworkPlugin) directly. This would introduce flexibility that may be required in some situations. For example this allows you to use different AvatarManagers for different scenes.


Changing spawning behavior is a very common case. You could use the skeleton name or some other state information (e.g. where it spawns) to decide, which avatar to spawn.
