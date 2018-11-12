using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellManager : MonoBehaviour
{
    public static GridCellManager instance = null;

    public GameObject m_allyGrid;
    public GameObject m_enemyGrid;

    public GameObject m_allyWinMessage;
    public GameObject m_allyLoseMessage;

    public GameObject m_enemyWinMessage;
    public GameObject m_enemyLoseMessage;

    public GameObject m_boatPlane;

    //List of grid cell objects with a tag so we can
    //reference the cells with their name.
    SortedList<string, GridCell> m_gridCellList;

    [SerializeField]
    int cellCount;

    //Hardcoded value for now however this will need to be counted (even tho it should be 18 everytime)
    public int m_boatCount = 18;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }


        //Initialise grid cell list
        m_gridCellList = new SortedList<string, GridCell>();
    }

    /// <summary>
    ///Get's a grid cell game object based on it's tag
    ///E.g "A3" , "E9" etc.
    /// </summary>
    /// <param name="tag">Tag of cell to be found</param>
    /// <returns>A grid cell gameobject if found, null if not</returns>
    public GridCell GetGridCellByTag(string tag)
    {
        GridCell cell;
        m_gridCellList.TryGetValue(tag, out cell);
        return cell;
    }

    public void AddGridCell(GridCell cell)
    {
        m_gridCellList.Add(cell.m_sTag, cell);
    }

    public GridCell GetGridCellByPosition(Vector3 position)
    {
        GridCell cell = new GridCell();
        for(int i = 0; i  < m_gridCellList.Count; i++)
        {
            if(m_gridCellList.Values[i].transform.position == position)
            {
                cell = m_gridCellList.Values[i];
            }
        }

        return cell;
    }

    void Update()
    {
        //for(int i = 0; i < m_gridCellList.Count; i++)
        //{
        //    GridCell cell = m_gridCellList.Values[i];

        //    if(cell.m_bIsFree)
        //    {
        //        cell.gameObject.GetComponent<Renderer>().material.color = Color.green;
        //    }
        //    else
        //    {
        //        cell.gameObject.GetComponent<Renderer>().material.color = Color.red;
        //    }
        //}
    }

    public void ToggleWinMessage(bool active)
    {
        m_allyWinMessage.SetActive(active);
        m_enemyWinMessage.SetActive(active);
    }

    public void ToggleLoseMessage(bool active)
    {
        m_allyLoseMessage.SetActive(active);
        m_enemyLoseMessage.SetActive(active);
    }

    public void ToggleBoatPlane(bool active)
    {
        m_boatPlane.SetActive(active);
    }


    public void ResetEnemyGrid()
    {
        GridCell[] enemyCells = m_enemyGrid.GetComponentsInChildren<GridCell>();

        for(int i = 0; i < enemyCells.Length; i++)
        {
            enemyCells[i].GetComponent<Renderer>().material.color = Color.black;
        }
    }

    public void ToggleEnemyGrid(bool active)
    {
        m_enemyGrid.SetActive(active);
    }

    public void SetEnemyCell(string tag, bool hit)
    {
        Transform cell = m_enemyGrid.transform.Find(tag);

        if(cell != null)
        {
            if(hit)
            {
                cell.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                cell.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     