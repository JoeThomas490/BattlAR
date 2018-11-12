using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camWorldPos = Camera.main.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(camWorldPos - transform.position, Vector3.up);
        lookRotation.x = 0;

        transform.rotation = lookRotation;
    }
}
