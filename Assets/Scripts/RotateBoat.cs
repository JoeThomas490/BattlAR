using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBoat : MonoBehaviour
{

    void Start()
    {
    }


    public void RotateLeft()
    {
        Debug.Log("Rotate left hit");
        transform.parent.Rotate(Vector3.up, -90);
    }

    public void RotateRight()
    {
        Debug.Log("Rotate right hit");
        transform.parent.Rotate(Vector3.up, 90);

    }
}
