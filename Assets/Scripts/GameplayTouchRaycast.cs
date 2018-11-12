using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameplayTouchRaycast : MonoBehaviour
{

    EventSystem m_eventSystem;

    void Start()
    {
        m_eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (m_eventSystem.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if(AuthorizeSelection.m_bIsAuthorizing)
            {
                return;
            }

            if (Input.touchCount >= 3)
            {
                ARNetworkManager.instance.TrySendTouchFire("A1");
            }

            if(touch.phase == TouchPhase.Ended)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    //If the raycast has hit a boat
                    if (hit.transform.tag == "EnemyCell")
                    {
                        Debug.Log("Trying to send touch fire command");
                        ARNetworkManager.instance.TrySendTouchFire(hit.transform.name);
                    }
                }
            }
        }
    }
}
