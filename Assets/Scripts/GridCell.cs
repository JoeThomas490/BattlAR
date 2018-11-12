using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{

    public string m_sTag;
    public bool m_bIsFree;

    public bool m_bIsHit;


    public GameObject m_blockingObj;

    void Start()
    {
        m_sTag = gameObject.name;
        m_bIsFree = true;
        m_bIsHit = false;

        m_blockingObj = null;

        //Add grid cell to manager list
        GridCellManager.instance.AddGridCell(this);
    }
}
