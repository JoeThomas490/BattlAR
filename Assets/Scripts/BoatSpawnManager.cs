using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatSpawnManager : MonoBehaviour
{

    public static BoatSpawnManager instance = null;

    public GameObject[] m_boats;
    public GameObject m_readyUpButton;

    public BoatComponent[] m_boatComponents;


    // Use this for initialization
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void ResetAllBoats()
    {
        for (int i = 0; i < m_boats.Length; i++)
        {
            m_boats[i].transform.localPosition = m_boatComponents[i].m_originalPosition;
            m_boatComponents[i].m_bOnGrid = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int spawnedCount = 0;

        //Loop through all the boat spawners
        for(int i = 0; i < m_boatComponents.Length; i++)
        {
            //If the boat component is on the grid then we know it's been "spawned"
            if(m_boatComponents[i].m_bOnGrid)
            {
                spawnedCount++;
            }
        }

        //If all our boats have been spawned
        if(spawnedCount >= m_boatComponents.Length)
        {
            //Activate the ready game button
            m_readyUpButton.SetActive(true);
        }
        else
        {
            m_readyUpButton.SetActive(false);
        }
    }
}
