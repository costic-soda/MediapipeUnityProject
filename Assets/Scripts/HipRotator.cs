using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipRotator : MonoBehaviour
{
    public Transform leftSphere;
    public Transform rightSphere;

    public Transform shoulderCube;
    public Transform hipCube;
    public Transform hipLeft;
    public Transform hipRight;
    public Transform hipFront;
    public Transform hipBack;
    public Transform hipUp;
    public Transform hipDown;

    public float angleX;
    public float angleY;
    public float angleZ;
    
    public Vector3 side1XZ;
    public Vector3 side2XZ;
    public Vector3 side1XY;
    public Vector3 side2XY;
    public Vector3 side1YZ;
    public Vector3 side2YZ;

    public GameObject hipTarget;
    
    //public Transform hipLeftPos;
    // Start is called before the first frame update
    void Start()
    {
        hipTarget.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        side1XZ = new Vector3(leftSphere.transform.position.x - hipCube.transform.position.x, 0, leftSphere.transform.position.z - hipCube.transform.position.z);
        side2XZ = new Vector3(hipLeft.transform.position.x - hipCube.transform.position.x, 0, hipLeft.transform.position.z - hipCube.transform.position.z);

        side1XY = new Vector3(shoulderCube.transform.position.x - hipCube.transform.position.x, shoulderCube.transform.position.y - hipCube.transform.position.y);
        side2XY = new Vector3(0, 1, 0);

        side1YZ = new Vector3();
        side2YZ = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        side1XZ = new Vector3(leftSphere.transform.position.x - hipCube.transform.position.x, 0, leftSphere.transform.position.z - hipCube.transform.position.z);
        side2XZ = new Vector3(hipLeft.transform.position.x - hipCube.transform.position.x, 0, hipLeft.transform.position.z - hipCube.transform.position.z);

        side1XY = new Vector3(shoulderCube.transform.position.x - hipCube.transform.position.x, shoulderCube.transform.position.y - hipCube.transform.position.y);
        side2XY = new Vector3(0, 1, 0);

        Debug.Log(hipLeft.transform.position);
        if(leftSphere.transform.position.z - hipCube.transform.position.z >= 0)
        {
            angleY = Vector3.Angle(side1XZ, side2XZ);
        }
        else
        {
            angleY = -1 * Vector3.Angle(side1XZ, side2XZ);
        }
        //angleZ = Vector3.Angle(side1XY, side2XY);
        if (shoulderCube.transform.position.x - hipCube.transform.position.x >= 0)
        {
            angleZ = Vector3.Angle(side1XY, side2XY);
        }
        else
        {
            angleZ = -1 * Vector3.Angle(side1XY, side2XY);

        }
        hipTarget.transform.rotation = Quaternion.Euler(new Vector3(hipTarget.transform.rotation.eulerAngles.x, angleY, /*hipTarget.transform.rotation.eulerAngles.z*/angleZ));
    }
}
