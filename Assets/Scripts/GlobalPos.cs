using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPos : MonoBehaviour
{
    public GameObject bruh;
    public Vector3 globalPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        globalPos = bruh.transform.position;
    }
}
