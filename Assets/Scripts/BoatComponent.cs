using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatComponent : MonoBehaviour
{

    public enum BoatType
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    };

    public BoatType m_boatType;

    public bool m_bOnGrid = false;

    public GridCell m_cell;

    public Vector3 m_originalPosition;
    public Quaternion m_originalRotation;
    // Use this for initialization
    void Start()
    {
        m_originalPosition = transform.parent.localPosition;
        m_originalRotation = transform.parent.localRotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void MoveBoat(Vector3 mPosition)
    {
        transform.parent.transform.position = mPosition;
    }
}
