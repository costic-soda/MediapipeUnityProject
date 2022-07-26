// kate: indent-width 4; tab-width 4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class MonoCaptury : MonoBehaviour
{
	[DllImport("CameraCaptury")]
	private static extern int ccInit();
	[DllImport("CameraCaptury")]
	private static extern IntPtr ccGetCameraName(int cameraIndex);
	[DllImport("CameraCaptury")]
	private static extern int ccStartCamera(int cameraIndex);
	[DllImport("CameraCaptury")]
	private static extern int ccStopCamera();
	[DllImport("CameraCaptury")]
	private static extern IntPtr ccGetCurrentImage();
	[DllImport("CameraCaptury")]
	private static extern IntPtr ccGetCameraCalibration();

	// on Awake or Start:
	// returns the number of detected cameras
	[DllImport("MonoCaptury")]
	private static extern int mcInit(IntPtr licenseKey);

	// sets the directory name that debug images are written to
	[DllImport("MonoCaptury")]
	private static extern void mcSetDebugDirectory(IntPtr dir);

	// sets the directory name that debug images are written to
	[DllImport("MonoCaptury")]
	private static extern void mcSetDetectionThresholds(float noseThreshold, float handThreshold, float shoulderThreshold);

	[DllImport("MonoCaptury")]
	private static extern int mcRunInference(IntPtr img, IntPtr calib);

	// on Update:
	// < 0 for left and > 0 for right, magnitude encodes strength (in pixels)
	[DllImport("MonoCaptury")]
	private static extern int mcGetLean();

	// negative: wrist below shoulder, positive: above shoulder, magnitude encodes strength (in pixels)
	[DllImport("MonoCaptury")]
	private static extern int mcIsLeftHandUp();

	// negative: wrist below shoulder, positive: above shoulder, magnitude encodes strength (in pixels)
	[DllImport("MonoCaptury")]
	private static extern int mcIsRightHandUp();

	public GameObject head;
	public GameObject leftHand;
	public GameObject rightHand;

	private IntPtr calib;

	private Thread thread;
	private bool stopNow;

	public float inferenceFramerate = 10.0f;

	//=============================
	// this is run once at startup
	//=============================
	void Start()
	{
		IntPtr licenseKey = Marshal.StringToHGlobalAnsi("K6q8Jiu8T9gyeJb5TbU1R3etfxYJ17kcyBFT8pfVnozTRPyTXLFyQ73ak6Ei4CbCeAasgSMqLqGfcomxLtSD7pZkVCat5ifKS3DynoKd3rBLZLBYjdJ3DqzoBUcrdB4gjXLZzkNeNhAS2ZLnkKYFQwtbr");
		int numCameras = ccInit();
		int ok = mcInit(licenseKey);
		Marshal.FreeHGlobal(licenseKey);

		if (ok < 0) {
			Debug.LogError("license is invalid: " + ok);
			Debug.Break();
		}

		if (numCameras <= 0) {
			Debug.LogError("not enough cameras: " + numCameras);
			Debug.Break();
		}

	        for (int i = 0; i < numCameras; ++i) {
			IntPtr msg = ccGetCameraName(i);
			string name = Marshal.PtrToStringAnsi(msg);
			Debug.Log("Captury: found camera " + name);
		}

		// IntPtr dirName = Marshal.StringToHGlobalAnsi(".");
		// mcSetDebugDirectory(dirName);
		// Marshal.FreeHGlobal(dirName);

		// mcSetDetectionThresholds(0.1f, 0.02f, 0.02f);


		int ret = ccStartCamera(8);
		if (ret <= 0) { // simply use first camera
			Debug.Log("failed to start camera: " + ret);
		}
		Debug.Log("started camera");

		stopNow = false;
		thread = new Thread(track);
		thread.Start();
	}
	void track()
	{
		calib = ccGetCameraCalibration();

		Int64 step = (Int64) (1e7f / inferenceFramerate);
		if (inferenceFramerate == 0)
			step = 1000000; // 10fps

		while (!stopNow) {
			Int64 now = DateTime.Now.Ticks;
			Int64 endTime = now + step;
			IntPtr img = ccGetCurrentImage();
			if (img == IntPtr.Zero) {
				Debug.Log("no image received");
				return;
			}

			int ret = mcRunInference(img, calib);
			if (ret <= 0) {
				Debug.Log("inference failed: " + ret);
			}

			Int64 inferenceTime = (DateTime.Now.Ticks - now);
			now = DateTime.Now.Ticks;
			if (endTime > now)
				Thread.Sleep((int) ((endTime - now) / 10000));

			Debug.Log("inference ran for " + (inferenceTime / 10000) + "ms, slept for " + ((endTime - now) / 10000) + "ms, " + " span " + (step / 10000) + "ms");
		}
	}
	//============================
	// this is run once per frame
	//============================
	void Update()
	{
		int lean = mcGetLean();
		int leftHandUp = mcIsLeftHandUp();
		int rightHandUp = mcIsRightHandUp();

		if (lean != 1000000)
			head.transform.position = new Vector3(-lean*0.01f, head.transform.position.y, head.transform.position.z);

		if (leftHandUp != 1000000)
			leftHand.transform.position = new Vector3(leftHand.transform.position.x, -leftHandUp*0.01f, leftHand.transform.position.z);

		if (rightHandUp != 1000000)
			rightHand.transform.position = new Vector3(rightHand.transform.position.x, -rightHandUp*0.01f, rightHand.transform.position.z);

		Debug.Log("lean: " + lean + ", left: " + leftHandUp + ", right: " + rightHandUp);
	}

	//==========================
	// this is run once at exit
	//==========================
	void OnDestroy()
	{
		ccStopCamera();

		stopNow = true;
		thread.Join();
	}
}
