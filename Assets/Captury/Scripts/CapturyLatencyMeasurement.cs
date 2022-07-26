using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Captury
{
    /// <summary>
    /// Instantiates Captury Avatars and handles the user assignment
    /// </summary>
    public class CapturyLatencyMeasurement : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="CapturyNetworkPlugin"/> handles the connection to the captury server
        /// </summary>
        [SerializeField]
        public CapturyNetworkPlugin networkPlugin = null;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private Int64 lastPoseTimestamp = 0;
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (networkPlugin) {
                Int64 now = (Int64)networkPlugin.getTime();
                Int64 maxLatency = 0;
                Int64 maxUpdate = 0;
                Int64 maxPoseTimestamp = 0;
                foreach (KeyValuePair<int, CapturyNetworkPlugin.Timestamps> kvp in networkPlugin.timestampsForPoses) {
                    Int64 first = kvp.Value.latencyInfo.firstImagePacketTime;
                    Int64 latency = now - first;
                    if (latency > maxLatency)
                        maxLatency = latency;
                    latency = now - kvp.Value.update;
                    if (latency > maxUpdate)
                        maxUpdate = latency;
                    if (kvp.Value.latencyInfo.timestampOfCorrespondingPose > maxPoseTimestamp)
                        maxPoseTimestamp = kvp.Value.latencyInfo.timestampOfCorrespondingPose;
                }

                if (maxPoseTimestamp != lastPoseTimestamp) {
                    Debug.Log("latency: " + maxLatency / 1000 + "ms incl. unity update: " + maxUpdate / 1000 + "ms");
                    lastPoseTimestamp = maxPoseTimestamp;
                }
            }

            Graphics.Blit(source, destination);
        }

    }
}
