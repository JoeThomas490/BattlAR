using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchRaycast : MonoBehaviour
{
    public GameObject m_boatComponentPrefab;

    public GameObject m_gridPlane;

    public LayerMask m_cellLayer;

    EventSystem m_eventSystem;

    GameObject m_activeDragObject;
    GameObject m_lastActiveObject;

    GameObject m_lastSpawnPrefab;

    GridCell m_lastActiveCell;

    List<GridCell> m_lastActiveCells;

    Vector3 m_originalPos;

    void Start()
    {
        m_lastActiveCells = new List<GridCell>();
        m_eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    HandleTouchBegan(touch);
                    break;

                case TouchPhase.Moved:
                    HandleTouchMoved(touch);
                    break;

                case TouchPhase.Ended:
                    HandleTouchEnded(touch);
                    break;
            }
        }
    }

    void HandleTouchBegan(Touch touch)
    {
        //Stop raycast if we're over GUI
        if (m_eventSystem.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        TryGetActiveObject(touch);

        GetReadyButton(touch);
    }

    void TryGetActiveObject(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //If the raycast has hit a boat
            if (hit.transform.tag == "Boat")
            {
                
                BoatComponent[] boats = hit.transform.parent.GetComponentsInChildren<BoatComponent>();
                //If the boat is already on the grid
                if (boats[0].m_bOnGrid)
                {
                    //Get the parent of the boat component
                    m_activeDragObject = hit.transform.parent.gameObject;

                    //Loop through each boat component
                    foreach (BoatComponent boat in boats)
                    {
                        //Get the grid cell the boat is on
                        GridCell bCell = GridCellManager.instance.GetGridCellByPosition(boat.transform.position);
                        //If this is a valid cell (i.e on the grid)
                        if (bCell != null)
                        {
                            //Add the cell to the last active cell list
                            m_lastActiveCells.Add(bCell);
                        }
                    }
                }
                //If the boat isn't already on the grid
                else
                {
                    //Instantiate a copy of the "boat spawner"
                    //m_activeDragObject = Instantiate(hit.transform.parent.gameObject, m_gridPlane.transform.parent);

                    m_activeDragObject = hit.transform.parent.gameObject;

                    //Set the last spawn prefab to this "boat spawner"
                    m_lastSpawnPrefab = hit.transform.parent.gameObject;
                    //Set the spawner to not be active
                    //m_lastSpawnPrefab.SetActive(false);

                }

                //If we don't have a last active object
                if (m_lastActiveObject == null)
                {
                    //Set the last active object to this new object
                    m_lastActiveObject = m_activeDragObject;
                    //Activate it's rotate canvas
                    m_lastActiveObject.transform.Find("RotateButtonCanvas").gameObject.SetActive(true);
                }
                else
                {
                    //Clicked on a different object
                    if (m_activeDragObject != m_lastActiveObject)
                    {
                        m_lastActiveObject.transform.Find("RotateButtonCanvas").gameObject.SetActive(false);
                        m_lastActiveObject = m_activeDragObject;
                        m_lastActiveObject.transform.Find("RotateButtonCanvas").gameObject.SetActive(true);
                    }
                }

            }
        }
    }

    void GetReadyButton(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //If the raycast has hit a boat
            if (hit.transform.tag == "Ready")
            {
                //Ready up in here..
                Debug.Log("Client ready");
                ARNetworkManager.instance.ReadyUp();

                GridCellManager.instance.ToggleBoatPlane(false);

                hit.transform.gameObject.SetActive(false);

                if (m_lastActiveObject != null)
                {
                    //Activate it's rotate canvas
                    m_lastActiveObject.transform.Find("RotateButtonCanvas").gameObject.SetActive(false);
                } 

                this.enabled = false;
            }
        }
    }

    void HandleTouchMoved(Touch touch)
    {
        if (m_activeDragObject != null)
        {
            //Get current cell finger is on
            GridCell cell = GetCellFromRay(touch, m_cellLayer);

            //Store the last position
            Vector3 lastPos;

            //If this is a valid cell
            if (cell != null)
            {
                //Store last position
                lastPos = m_activeDragObject.transform.position;

                //Move boat to new position
                m_activeDragObject.transform.position = cell.transform.position;

                BoatComponent[] boats = m_activeDragObject.GetComponentsInChildren<BoatComponent>();

                bool allCellsFree = CheckIfFree(boats);

                //If all the cells are free
                if (allCellsFree)
                {
                    //Set the new position of the object
                    m_activeDragObject.transform.position = cell.transform.position;

                    //Make sure they're set as now on the grid
                    foreach (BoatComponent b in boats)
                    {
                        b.m_bOnGrid = true;
                    }

                    if (m_lastActiveCells.Count > 0)
                    {

                        if (m_lastActiveCells[0] != null)
                        {
                            //If we have moved onto a new grid cell
                            if (cell.gameObject != m_lastActiveCells[0].gameObject)
                            {
                                //Set the old cells to be free
                                foreach (GridCell c in m_lastActiveCells)
                                {
                                    c.m_bIsFree = true;
                                    c.m_blockingObj = null;
                                }
                            }
                        }
                    }

                    //Clear the last active cells list
                    m_lastActiveCells.Clear();
                    m_lastActiveCells.Capacity = 0;

                    //Loop through the boats
                    foreach (BoatComponent b in boats)
                    {
                        //Get every cell the boat is touching
                        GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                        //Add it to the last active cell list
                        m_lastActiveCells.Add(bCell);
                        bCell.m_bIsFree = false;
                        bCell.m_blockingObj = b.transform.parent.gameObject;
                    }
                }
                //If the cells aren't free then move back to original position
                else
                {
                    m_activeDragObject.transform.position = lastPos;
                }
            }

            //Move boat object when not attached to the grid
            else
            {
                foreach (GridCell lastCell in m_lastActiveCells)
                {
                    lastCell.m_bIsFree = true;
                    lastCell.m_blockingObj = null;
                }

                //GridCellManager.instance.SetGridCell(m_lastActiveCell);

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.tag == "GroundingPlane")
                    {
                        BoatComponent[] boats = m_activeDragObject.GetComponentsInChildren<BoatComponent>();
                        foreach (BoatComponent b in boats)
                        {
                            b.m_bOnGrid = false;
                        }
                        m_activeDragObject.transform.position = hit.point;
                    }
                }
            }

        }
    }

    void HandleTouchEnded(Touch touch)
    {
        if (m_activeDragObject != null)
        {
            //Get current cell finger is on
            GridCell cell = GetCellFromRay(touch, m_cellLayer);

            //Store the last position
            Vector3 lastPos;

            BoatComponent[] boats = m_activeDragObject.GetComponentsInChildren<BoatComponent>();

            //If this is a valid cell
            if (cell != null)
            {
                //Store last position
                lastPos = m_activeDragObject.transform.position;

                //Move boat to new position
                m_activeDragObject.transform.position = cell.transform.position;

                bool allCellsFree = CheckIfFree(boats);

                //If all the cells are free
                if (allCellsFree)
                {
                    //Set the new position of the object
                    m_activeDragObject.transform.position = cell.transform.position;

                    //Make sure they're set as now on the grid
                    foreach (BoatComponent b in boats)
                    {
                        b.m_bOnGrid = true;
                    }

                    if (m_lastActiveCells.Count > 0)
                    {

                        if (m_lastActiveCells[0] != null)
                        {
                            //If we have moved onto a new grid cell
                            if (cell.gameObject != m_lastActiveCells[0].gameObject)
                            {
                                //Set the old cells to be free
                                foreach (GridCell c in m_lastActiveCells)
                                {
                                    c.m_bIsFree = true;
                                    c.m_blockingObj = null;
                                }
                            }
                        }
                    }

                    //Clear the last active cells list
                    m_lastActiveCells.Clear();
                    m_lastActiveCells.Capacity = 0;

                    //Loop through the boats
                    foreach (BoatComponent b in boats)
                    {
                        //Get every cell the boat is touching
                        GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                        //Add it to the last active cell list
                        m_lastActiveCells.Add(bCell);
                        bCell.m_bIsFree = false;
                        bCell.m_blockingObj = b.transform.parent.gameObject;
                    }
                }
                //If the cells aren't free then move back to original position
                else
                {
                    m_activeDragObject.transform.position = lastPos;
                }
            }
            //If we haven't finished the touch on the grid
            else
            {
                Vector3 originalPos = m_activeDragObject.transform.GetChild(0).GetComponent<BoatComponent>().m_originalPosition;
                m_activeDragObject.transform.localPosition = originalPos;
                //Quaternion originalRot = m_activeDragObject.transform.GetChild(0).GetComponent<BoatComponent>().m_originalRotation;

                //m_activeDragObject.transform.rotation = originalRot;

                m_lastActiveObject.transform.Find("RotateButtonCanvas").gameObject.SetActive(false);
                m_lastActiveObject = null;
            }

            foreach (BoatComponent b in boats)
            {
                GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                bCell.m_bIsFree = false;
                bCell.m_blockingObj = b.transform.parent.gameObject;
            }

            m_lastActiveCells.Clear();

            //if (m_activeDragObject.GetComponentInChildren<BoatComponent>().m_bOnGrid == false)
            //{
            //    Destroy(m_activeDragObject);
            //}
        }

        m_activeDragObject = null;
    }

    GridCell GetCellFromRay(Touch touch, LayerMask layerMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        GridCell cell = new GridCell();

        if (m_eventSystem.IsPointerOverGameObject(touch.fingerId))
        {
            return cell;
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform.tag == "Cell")
            {
                cell = hit.transform.GetComponent<GridCell>();
            }
        }

        return cell;
    }

    bool CheckIfFree(BoatComponent[] boats)
    {
        bool allCellsFree = true;

        foreach (BoatComponent b in boats)
        {
            //Get the cell 
            GridCell testCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
            //If it's a valid cell
            if (testCell != null)
            {
                //If it's blocked and being blocked by a different boat
                if (!testCell.m_bIsFree && testCell.m_blockingObj != b.transform.parent.gameObject)
                {
                    //Return out early
                    allCellsFree = false;
                    return allCellsFree;
                }

            }
            else
            {
                allCellsFree = false;
                return allCellsFree;
            }
        }

        return allCellsFree;
    }

    #region Rotate

    public void RotateLeft()
    {
        BoatComponent[] boats = m_lastActiveObject.GetComponentsInChildren<BoatComponent>();

        if (boats[0].m_bOnGrid)
        {
            if (m_lastActiveCells.Count == 0)
            {
                //Loop through the boats
                foreach (BoatComponent b in boats)
                {
                    //Get every cell the boat is touching
                    GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                    //Add it to the last active cell list
                    m_lastActiveCells.Add(bCell);
                    bCell.m_bIsFree = false;
                    bCell.m_blockingObj = b.transform.parent.gameObject;
                }
            }

            m_lastActiveObject.transform.Rotate(Vector3.up, 90);

            //Get all the boat components within the active object

            GridCell cell = GridCellManager.instance.GetGridCellByPosition(m_lastActiveObject.transform.position);

            bool allCellsFree = CheckIfFree(boats);

            if (allCellsFree)
            {
                //Make sure they're set as now on the grid
                foreach (BoatComponent b in boats)
                {
                    b.m_bOnGrid = true;
                }

                //Set the old cells to be free
                foreach (GridCell c in m_lastActiveCells)
                {
                    c.m_bIsFree = true;
                    c.m_blockingObj = null;
                }

                //Clear the last active cells list
                m_lastActiveCells.Clear();

                //Loop through the boats
                foreach (BoatComponent b in boats)
                {
                    //Get every cell the boat is touching
                    GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                    bCell.m_bIsFree = false;
                    bCell.m_blockingObj = b.transform.parent.gameObject;
                }
            }
            else
            {
                m_lastActiveObject.transform.Rotate(Vector3.up, -90);
            }
        }
    }

    public void RotateRight()
    {
        BoatComponent[] boats = m_lastActiveObject.GetComponentsInChildren<BoatComponent>();

        if (boats[0].m_bOnGrid)
        {
            if (m_lastActiveCells.Count == 0)
            {
                //Loop through the boats
                foreach (BoatComponent b in boats)
                {
                    //Get every cell the boat is touching
                    GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                    //Add it to the last active cell list
                    m_lastActiveCells.Add(bCell);
                    bCell.m_bIsFree = false;
                    bCell.m_blockingObj = b.transform.parent.gameObject;
                }
            }

            m_lastActiveObject.transform.Rotate(Vector3.up, -90);

            //Get all the boat components within the active object

            GridCell cell = GridCellManager.instance.GetGridCellByPosition(m_lastActiveObject.transform.position);

            bool allCellsFree = CheckIfFree(boats);

            if (allCellsFree)
            {
                //Make sure they're set as now on the grid
                foreach (BoatComponent b in boats)
                {
                    b.m_bOnGrid = true;
                }

                //Set the old cells to be free
                foreach (GridCell c in m_lastActiveCells)
                {
                    c.m_bIsFree = true;
                    c.m_blockingObj = null;
                }

                //Clear the last active cells list
                m_lastActiveCells.Clear();

                //Loop through the boats
                foreach (BoatComponent b in boats)
                {
                    //Get every cell the boat is touching
                    GridCell bCell = GridCellManager.instance.GetGridCellByPosition(b.transform.position);
                    bCell.m_bIsFree = false;
                    bCell.m_blockingObj = b.transform.parent.gameObject;
                }
            }
            else
            {
                m_lastActiveObject.transform.Rotate(Vector3.up, 90);
            }
        }
    }

    #endregion

}
