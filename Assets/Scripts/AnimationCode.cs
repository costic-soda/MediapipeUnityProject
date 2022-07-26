using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AnimationCode : MonoBehaviour
{
    public GameObject hipCube;
    public GameObject shoulderCube;
    public GameObject[] Body;
    List<string> lines;

    public Transform leftHandSphere;
    public Transform rightHandSphere;
    public Transform leftElbowSphere;
    public Transform rightElbowSphere;
    public Transform leftShoulderSphere;
    public Transform rightShoulderSphere;
    public Transform leftFootSphere;
    public Transform rightFootSphere;
    public Transform leftKneeSphere;
    public Transform rightKneeSphere;
    public Transform leftHipSphere;
    public Transform rightHipSphere;
    //public Transform bodyTarget;
    public Transform noseSphere;

    public Camera ZCam;
    //public Vector3 bodyCubePos;
    //public float bodyCubeYOffset;
    //public Transform spine1;
    //public Transform pelvis;

    int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        //lines = System.IO.File.ReadLines("Assets/AnimationFile1.txt").ToList();
    }

    #region List of Body Parts
    /*
    NOSE = 0
    LEFT_EYE_INNER = 1
    LEFT_EYE = 2
    LEFT_EYE_OUTER = 3
    RIGHT_EYE_INNER = 4
    RIGHT_EYE = 5
    RIGHT_EYE_OUTER = 6
    LEFT_EAR = 7
    RIGHT_EAR = 8
    MOUTH_LEFT = 9
    MOUTH_RIGHT = 10
    LEFT_SHOULDER = 11
    RIGHT_SHOULDER = 12
    LEFT_ELBOW = 13
    RIGHT_ELBOW = 14
    LEFT_WRIST = 15
    RIGHT_WRIST = 16
    LEFT_PINKY = 17
    RIGHT_PINKY = 18
    LEFT_INDEX = 19
    RIGHT_INDEX = 20
    LEFT_THUMB = 21
    RIGHT_THUMB = 22
    LEFT_HIP = 23
    RIGHT_HIP = 24
    LEFT_KNEE = 25
    RIGHT_KNEE = 26
    LEFT_ANKLE = 27
    RIGHT_ANKLE = 28
    LEFT_HEEL = 29
    RIGHT_HEEL = 30
    LEFT_FOOT_INDEX = 31
    RIGHT_FOOT_INDEX = 32
    */
    #endregion

    // Update is called once per frame
    void Update()
    {
        Vector3 leftShoulderZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_SHOULDER[0]), float.Parse(Globals.Variables.LEFT_SHOULDER[1]) - 0.5f, float.Parse(Globals.Variables.LEFT_SHOULDER[2])));
        Vector3 leftElbowZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_ELBOW[0]), float.Parse(Globals.Variables.LEFT_ELBOW[1]), float.Parse(Globals.Variables.LEFT_ELBOW[2])));
        Vector3 leftHandZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_WRIST[0]), float.Parse(Globals.Variables.LEFT_WRIST[1]), float.Parse(Globals.Variables.LEFT_WRIST[2])));

        leftShoulderSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_SHOULDER[0]), float.Parse(Globals.Variables.LEFT_SHOULDER[1]) - 0.5f, leftShoulderZ[2]);
        leftElbowSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_ELBOW[0]), float.Parse(Globals.Variables.LEFT_ELBOW[1]), leftElbowZ[2]);
        leftHandSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_WRIST[0]), float.Parse(Globals.Variables.LEFT_WRIST[1]), leftHandZ[2]);

        Vector3 righttShoulderZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_SHOULDER[0]), float.Parse(Globals.Variables.RIGHT_SHOULDER[1]) - 0.5f, float.Parse(Globals.Variables.RIGHT_SHOULDER[2])));
        Vector3 rightElbowZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_ELBOW[0]), float.Parse(Globals.Variables.RIGHT_ELBOW[1]), float.Parse(Globals.Variables.RIGHT_ELBOW[2])));
        Vector3 rightHandZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_WRIST[0]), float.Parse(Globals.Variables.RIGHT_WRIST[1]), float.Parse(Globals.Variables.RIGHT_WRIST[2])));

        rightShoulderSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_SHOULDER[0]), float.Parse(Globals.Variables.RIGHT_SHOULDER[1]) - 0.5f, righttShoulderZ[2]);
        rightElbowSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_ELBOW[0]), float.Parse(Globals.Variables.RIGHT_ELBOW[1]), rightElbowZ[2]);
        rightHandSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_WRIST[0]), float.Parse(Globals.Variables.RIGHT_WRIST[1]), rightHandZ[2]);

        Vector3 leftHipZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_HIP[0]), float.Parse(Globals.Variables.LEFT_HIP[1]), float.Parse(Globals.Variables.LEFT_HIP[2])));
        Vector3 leftKneeZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_KNEE[0]), float.Parse(Globals.Variables.LEFT_KNEE[1]), float.Parse(Globals.Variables.LEFT_KNEE[2])));
        Vector3 leftFootZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.LEFT_ANKLE[0]), float.Parse(Globals.Variables.LEFT_ANKLE[1]), float.Parse(Globals.Variables.LEFT_ANKLE[2])));

        leftHipSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_HIP[0]), float.Parse(Globals.Variables.LEFT_HIP[1]), leftHipZ[2]);
        leftKneeSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_KNEE[0]), float.Parse(Globals.Variables.LEFT_KNEE[1]), leftKneeZ[2]);
        leftFootSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_ANKLE[0]), float.Parse(Globals.Variables.LEFT_ANKLE[1]), leftFootZ[2]);

        Vector3 rightHipZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_HIP[0]), float.Parse(Globals.Variables.RIGHT_HIP[1]), float.Parse(Globals.Variables.RIGHT_HIP[2])));
        Vector3 rightKneeZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_KNEE[0]), float.Parse(Globals.Variables.RIGHT_KNEE[1]), float.Parse(Globals.Variables.RIGHT_KNEE[2])));
        Vector3 rightFootZ = ZCam.ViewportToWorldPoint(new Vector3(float.Parse(Globals.Variables.RIGHT_ANKLE[0]), float.Parse(Globals.Variables.RIGHT_ANKLE[1]), float.Parse(Globals.Variables.RIGHT_ANKLE[2])));

        rightHipSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_HIP[0]), float.Parse(Globals.Variables.RIGHT_HIP[1]), rightHipZ[2]);
        rightKneeSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_KNEE[0]), float.Parse(Globals.Variables.RIGHT_KNEE[1]), rightKneeZ[2]);
        rightFootSphere.transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_ANKLE[0]), float.Parse(Globals.Variables.RIGHT_ANKLE[1]), rightFootZ[2]);

        Body[17].transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_WRIST[0]), float.Parse(Globals.Variables.LEFT_WRIST[1]), float.Parse(Globals.Variables.LEFT_WRIST[2]));
        Body[18].transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_WRIST[0]), float.Parse(Globals.Variables.RIGHT_WRIST[1]), float.Parse(Globals.Variables.RIGHT_WRIST[2]));
        Body[19].transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_WRIST[0]), float.Parse(Globals.Variables.LEFT_WRIST[1]), float.Parse(Globals.Variables.LEFT_WRIST[2]));
        Body[20].transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_WRIST[0]), float.Parse(Globals.Variables.RIGHT_WRIST[1]), float.Parse(Globals.Variables.RIGHT_WRIST[2]));
        Body[21].transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_WRIST[0]), float.Parse(Globals.Variables.LEFT_WRIST[1]), float.Parse(Globals.Variables.LEFT_WRIST[2]));
        Body[22].transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_WRIST[0]), float.Parse(Globals.Variables.RIGHT_WRIST[1]), float.Parse(Globals.Variables.RIGHT_WRIST[2]));


        Body[29].transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_ANKLE[0]), float.Parse(Globals.Variables.LEFT_ANKLE[1]), float.Parse(Globals.Variables.LEFT_ANKLE[2]));
        Body[30].transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_ANKLE[0]), float.Parse(Globals.Variables.RIGHT_ANKLE[1]), float.Parse(Globals.Variables.RIGHT_ANKLE[2]));
        Body[31].transform.localPosition = new Vector3(float.Parse(Globals.Variables.LEFT_ANKLE[0]), float.Parse(Globals.Variables.LEFT_ANKLE[1]), float.Parse(Globals.Variables.LEFT_ANKLE[2]));
        Body[32].transform.localPosition = new Vector3(float.Parse(Globals.Variables.RIGHT_ANKLE[0]), float.Parse(Globals.Variables.RIGHT_ANKLE[1]), float.Parse(Globals.Variables.RIGHT_ANKLE[2]));

        for (int i = 0; i <= 1; i++)
        {
            Body[i].transform.localPosition = new Vector3(float.Parse(Globals.Variables.NOSE[0]), float.Parse(Globals.Variables.NOSE[1]), float.Parse(Globals.Variables.NOSE[2]));
        }
        //string[] points = lines[counter].Split(',');
        //for (int i = 0; i <= 32; i++)
        //{
        //    float x = float.Parse(points[0 + (i * 3)]) / 100;
        //    float y = float.Parse(points[1 + (i * 3)]) / 100;
        //    float z = float.Parse(points[2 + (i * 3)]) / 300;
        //    Body[i].transform.localPosition = new Vector3(x, y, z);
        //}
        //counter += 1;
        //if (counter == lines.Count)
        //{
        //    counter = 0;
        //}
        //Thread.Sleep(30);
    }
}


