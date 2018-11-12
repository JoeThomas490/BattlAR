using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NetworkManagerMenu : MonoBehaviour
{

    public NetworkManager manager;
    [SerializeField]
    public bool showGUI = true;
    [SerializeField]
    public int offsetX;
    [SerializeField]
    public int offsetY;

    public string serverIP = "192.168.1.167";

    // Runtime variable
    bool showServer = false;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    void Update()
    {
    }

    void OnGUI()
    {
        if (!showGUI)
            return;

        int xpos = 10 + offsetX;
        int ypos = 40 + offsetY;
        int spacing = 200;

        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
        {
            if (GUI.Button(new Rect(xpos, ypos, 1000, 180), "LAN Host(H)"))
            {
                //Debug.Log("")
                manager.StartHost();
            }
            ypos += spacing;

            if (GUI.Button(new Rect(xpos, ypos, 105, 180), "LAN Client(C)"))
            {
                manager.StartClient();
            }
            manager.networkAddress = GUI.TextField(new Rect(xpos + 100, ypos, 500, 180), manager.networkAddress);
            ypos += spacing;
        }
        else
        {
            if (NetworkServer.active)
            {
                GUI.Label(new Rect(xpos, ypos, 300, 20), "Server: port=" + manager.networkPort);
                ypos += spacing;
            }
            if (NetworkClient.active)
            {
                GUI.Label(new Rect(xpos, ypos, 300, 20), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                ypos += spacing;
            }
        }

        if (NetworkClient.active && !NetworkServer.active && !ClientScene.ready)
        {
            Debug.Log("Trying to ready client..");
            ClientScene.Ready(manager.client.connection);

            if (ClientScene.localPlayers.Count == 0)
            {
                Debug.Log("Adding local player");
                ClientScene.AddPlayer(0);
            }
        }

        if (NetworkServer.active || NetworkClient.active)
        {
            if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Stop (X)"))
            {
                manager.StopHost();
            }
            ypos += spacing;
        }
    }
}
