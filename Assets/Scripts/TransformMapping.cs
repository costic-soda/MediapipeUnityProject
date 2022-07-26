using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformMapping : MonoBehaviour
{
    //public Transform NOSE;
    //public Transform LEFT_EYE_INNER;
    //public Transform LEFT_EYE;
    //public Transform LEFT_EYE_OUTER;
    //public Transform RIGHT_EYE_INNER;
    //public Transform RIGHT_EYE;
    //public Transform RIGHT_EYE_OUTER;
    //public Transform LEFT_EAR;
    //public Transform RIGHT_EAR;
    //public Transform MOUTH_LEFT;
    //public Transform MOUTH_RIGHT;
    //public Transform LEFT_SHOULDER;
    //public Transform RIGHT_SHOULDER;
    //public Transform LEFT_ELBOW;
    //public Transform RIGHT_ELBOW;
    //public Transform LEFT_WRIST;
    //public Transform RIGHT_WRIST;
    //public Transform LEFT_PINKY;
    //public Transform RIGHT_PINKY;
    //public Transform LEFT_INDEX;
    //public Transform RIGHT_INDEX;
    //public Transform LEFT_THUMB;
    //public Transform RIGHT_THUMB;
    //public Transform LEFT_HIP;
    //public Transform RIGHT_HIP;
    //public Transform LEFT_KNEE;
    //public Transform RIGHT_KNEE;
    //public Transform LEFT_ANKLE;
    //public Transform RIGHT_ANKLE;
    //public Transform LEFT_HEEL;
    //public Transform RIGHT_HEEL;
    //public Transform LEFT_FOOT_INDEX;
    //public Transform RIGHT_FOOT_INDEX;

    //public Transform NOSE_SPHERE;
    //public Transform LEFT_EYE_INNER_SPHERE;
    //public Transform LEFT_EYE_SPHERE;
    //public Transform LEFT_EYE_OUTER_SPHERE;
    //public Transform RIGHT_EYE_INNER_SPHERE;
    //public Transform RIGHT_EYE_SPHERE;
    //public Transform RIGHT_EYE_OUTER_SPHERE;
    //public Transform LEFT_EAR_SPHERE;
    //public Transform RIGHT_EAR_SPHERE;
    //public Transform MOUTH_LEFT_SPHERE;
    //public Transform MOUTH_RIGHT_SPHERE;
    //public Transform LEFT_SHOULDER_SPHERE;
    //public Transform RIGHT_SHOULDER_SPHERE;
    //public Transform LEFT_ELBOW_SPHERE;
    //public Transform RIGHT_ELBOW_SPHERE;
    //public Transform LEFT_WRIST_SPHERE;
    //public Transform RIGHT_WRIST_SPHERE;
    //public Transform LEFT_PINKY_SPHERE;
    //public Transform RIGHT_PINKY_SPHERE;
    //public Transform LEFT_INDEX_SPHERE;
    //public Transform RIGHT_INDEX_SPHERE;
    //public Transform LEFT_THUMB_SPHERE;
    //public Transform RIGHT_THUMB_SPHERE;
    //public Transform LEFT_HIP_SPHERE;
    //public Transform RIGHT_HIP_SPHERE;
    //public Transform LEFT_KNEE_SPHERE;
    //public Transform RIGHT_KNEE_SPHERE;
    //public Transform LEFT_ANKLE_SPHERE;
    //public Transform RIGHT_ANKLE_SPHERE;
    //public Transform LEFT_HEEL_SPHERE;
    //public Transform RIGHT_HEEL_SPHERE;
    //public Transform LEFT_FOOT_INDEX_SPHERE;
    //public Transform RIGHT_FOOT_INDEX_SPHERE;
    //public Transform model;
    //public Transform body;

    public Transform leftHandSphere;
    public Transform rightHandSphere;
    public Transform leftHand;
    public Transform rightHand;

    public Transform leftElbowSphere;
    public Transform rightElbowSphere;
    public Transform leftElbow;
    public Transform rightElbow;

    public Transform leftShoulderSphere;
    public Transform rightShoulderSphere;
    public Transform leftShoulder;
    public Transform rightShoulder;

    public Transform leftFootSphere;
    public Transform rightFootSphere;
    public Transform leftFoot;
    public Transform rightFoot;

    public Transform leftKneeSphere;
    public Transform rightKneeSphere;
    public Transform leftKnee;
    public Transform rightKnee;

    public Transform leftHipSphere;
    public Transform rightHipSphere;
    public Transform leftHip;
    public Transform rightHip;

    public Transform bodyTarget;
    public Transform noseSphere;
    public Transform modelBody;
    public Transform stickBody;

    public Vector3 bodyCubePos;
    public float bodyCubeYOffset;
    public Transform spine1;
    public Transform pelvis;
    //public Transform head;
    // Start is called before the first frame update
    void Start()
    {
        modelBody.transform.position = stickBody.transform.position;
        bodyCubeYOffset = spine1.transform.position.y - pelvis.transform.position.y;
        bodyCubePos = new Vector3(
            (leftHipSphere.transform.position.x + rightHipSphere.transform.position.x) / 2,
            (leftHipSphere.transform.position.y + rightHipSphere.transform.position.y) / 2 + bodyCubeYOffset,
            (leftHipSphere.transform.position.z + rightHipSphere.transform.position.z) / 2
        );
        bodyTarget.transform.position = bodyCubePos;
        //model.transform.position = body.transform.position;
        leftHand.transform.position = leftHandSphere.transform.position;
        rightHand.transform.position = rightHandSphere.transform.position;

        leftElbow.transform.position = leftElbowSphere.transform.position;
        rightElbow.transform.position = rightElbowSphere.transform.position;
        
        leftShoulder.transform.position = leftShoulderSphere.transform.position;
        rightShoulder.transform.position = rightShoulderSphere.transform.position;

        leftFoot.transform.position = leftFootSphere.transform.position;
        rightFoot.transform.position = rightFootSphere.transform.position;

        leftKnee.transform.position = leftKneeSphere.transform.position;
        rightKnee.transform.position = rightKneeSphere.transform.position;

        leftHip.transform.position = leftHipSphere.transform.position;
        rightHip.transform.position = rightHipSphere.transform.position;

        //hip.transform.position = hipCube.transform.position;
        //head.transform.position = noseSphere.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        //modelBody.transform.position = stickBody.transform.position;

        //Bodyguard.transform.position = Body.transform.position;

        bodyCubePos = new Vector3(
            (leftHipSphere.transform.position.x + rightHipSphere.transform.position.x) / 2,
            (leftHipSphere.transform.position.y + rightHipSphere.transform.position.y) / 2 + bodyCubeYOffset,
            (leftHipSphere.transform.position.z + rightHipSphere.transform.position.z) / 2
        );
        bodyTarget.transform.position = bodyCubePos;
        leftHand.transform.position = leftHandSphere.transform.position;
        rightHand.transform.position = rightHandSphere.transform.position;

        leftElbow.transform.position = leftElbowSphere.transform.position;
        rightElbow.transform.position = rightElbowSphere.transform.position;

        leftShoulder.transform.position = leftShoulderSphere.transform.position;
        rightShoulder.transform.position = rightShoulderSphere.transform.position;

        leftFoot.transform.position = leftFootSphere.transform.position;
        rightFoot.transform.position = rightFootSphere.transform.position;

        leftKnee.transform.position = leftKneeSphere.transform.position;
        rightKnee.transform.position = rightKneeSphere.transform.position;

        leftHip.transform.position = leftHipSphere.transform.position;
        rightHip.transform.position = rightHipSphere.transform.position;

        //NOSE.transform.position = NOSE_SPHERE.transform.position;
        //LEFT_EYE_INNER.transform.position = LEFT_EYE_INNER_SPHERE.transform.position;
        //LEFT_EYE.transform.position = LEFT_EYE_SPHERE.transform.position;
        //LEFT_EYE_OUTER.transform.position = LEFT_EYE_OUTER_SPHERE.transform.position;
        //RIGHT_EYE_INNER.transform.position = RIGHT_EYE_INNER_SPHERE.transform.position;
        //RIGHT_EYE.transform.position = RIGHT_EYE_SPHERE.transform.position;
        //RIGHT_EYE_OUTER.transform.position = RIGHT_EYE_OUTER_SPHERE.transform.position;
        //LEFT_EAR.transform.position = LEFT_EAR_SPHERE.transform.position;
        //RIGHT_EAR.transform.position = RIGHT_EAR.transform.position;
        //MOUTH_LEFT.transform.position = MOUTH_LEFT_SPHERE.transform.position;
        //MOUTH_RIGHT.transform.position = MOUTH_RIGHT_SPHERE.transform.position;
        //LEFT_SHOULDER.transform.position = LEFT_SHOULDER_SPHERE.transform.position;
        //RIGHT_SHOULDER.transform.position = RIGHT_SHOULDER_SPHERE.transform.position;
        //LEFT_ELBOW.transform.position = LEFT_ELBOW_SPHERE.transform.position;
        //RIGHT_ELBOW.transform.position = RIGHT_ELBOW_SPHERE.transform.position;
        //LEFT_WRIST.transform.position = LEFT_WRIST_SPHERE.transform.position;
        //RIGHT_WRIST.transform.position = RIGHT_WRIST_SPHERE.transform.position;
        //LEFT_PINKY.transform.position = LEFT_PINKY_SPHERE.transform.position;
        //RIGHT_PINKY.transform.position = RIGHT_PINKY_SPHERE.transform.position;
        //LEFT_INDEX.transform.position = LEFT_INDEX_SPHERE.transform.position;
        //RIGHT_INDEX.transform.position = RIGHT_INDEX_SPHERE.transform.position;
        //LEFT_THUMB.transform.position = LEFT_THUMB_SPHERE.transform.position;
        //RIGHT_THUMB.transform.position = RIGHT_THUMB_SPHERE.transform.position;
        //LEFT_HIP.transform.position = LEFT_HIP_SPHERE.transform.position;
        //RIGHT_HIP.transform.position = RIGHT_HIP_SPHERE.transform.position;
        //LEFT_KNEE.transform.position = LEFT_KNEE_SPHERE.transform.position; 
        //RIGHT_KNEE.transform.position = RIGHT_KNEE_SPHERE.transform.position;
        //LEFT_ANKLE.transform.position = LEFT_ANKLE_SPHERE.transform.position;
        //RIGHT_ANKLE.transform.position = RIGHT_ANKLE_SPHERE.transform.position;
        //LEFT_HEEL.transform.position = LEFT_HEEL_SPHERE.transform.position;
        //RIGHT_HEEL.transform.position = RIGHT_HEEL_SPHERE.transform.position;
        //LEFT_FOOT_INDEX.transform.position = LEFT_FOOT_INDEX_SPHERE.transform.position;
        //RIGHT_FOOT_INDEX.transform.position = RIGHT_FOOT_INDEX_SPHERE.transform.position;

    }
}
