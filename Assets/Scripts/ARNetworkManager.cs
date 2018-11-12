using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class ARNetworkManager : NetworkManager
{
    private short serverMessage = 123;

    bool m_bClientReadyToPlay;

    bool m_bClientReplayReady;

    int m_boatsHit = 0;

    int m_clientScore = 0;
    int m_enemyScore = 0;

    public enum GAME_STATE
    {
        DISCONNECTED,
        INTRO,
        GAMEPLAY,
        GAMEPLAY_TURN,
        GAMEPLAY_WAIT,
        GAMEOVER_WIN,
        GAMEOVER_LOSE
    }

    public GAME_STATE m_currentState;

    public static ARNetworkManager instance = null;

    void Start()
    {
        //Setup ARNetworkManager instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                instance = null;
            }
        }

        ChangeGameState(GAME_STATE.DISCONNECTED);
    }

    #region Network Manager Overrides

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("OnClientConnect");
        base.OnClientConnect(conn);

        Debug.Log("Registering ReceiveMessage handler");
        client.RegisterHandler(serverMessage, ReceiveMessage);

        if (NetworkServer.active)
        {
            Debug.Log("Registering ServerReceiveMessage handler");
            NetworkServer.RegisterHandler(serverMessage, ServerReceiveMessage);
        }

        if (m_currentState == GAME_STATE.DISCONNECTED)
        {
            Debug.Log("Changing state to INTRO");
            ChangeGameState(GAME_STATE.INTRO);
        }

    }

    // called when disconnected from a server
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect");
        base.OnClientDisconnect(conn);

        ChangeGameState(GAME_STATE.DISCONNECTED);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        Debug.Log("OnServerConnect");
        base.OnServerConnect(conn);
    }

    #endregion

    public void ReadyUp()
    {
        m_bClientReadyToPlay = true;
        SendServerMessage("clientReady", "setup");
    }

    public void RequestReplay()
    {
        m_bClientReplayReady = true;
        SendServerMessage("clientReady", "replay");
    }

    public void Disconnect()
    {
        if (NetworkServer.active)
        {
            Debug.Log("Disconnecting host..");
            StopHost();
        }
        else
        {
            Debug.Log("Disconnecting client..");
            StopClient();
        }
    }

    public void TrySendTouchFire(string tag)
    {
        if (m_currentState == GAME_STATE.GAMEPLAY_TURN)
        {
            SendServerMessage(tag, "fire");
        }
    }

    void ChangeGameState(GAME_STATE newState)
    {
        Debug.Log("Change to new state : " + newState);

        switch (newState)
        {
            case GAME_STATE.INTRO:
                GUIManager.instance.ToggleListenButton(false);
                GUIManager.instance.ToggleReplayMessagePanel(false);

                GameObject.Find("ARCamera").GetComponent<TouchRaycast>().enabled = true;

                GridCellManager.instance.ResetEnemyGrid();
                GridCellManager.instance.ToggleEnemyGrid(false);

                GridCellManager.instance.ToggleLoseMessage(false);
                GridCellManager.instance.ToggleWinMessage(false);

                GridCellManager.instance.ToggleBoatPlane(true);

                m_boatsHit = 0;
                m_bClientReadyToPlay = false;
                m_bClientReplayReady = false;

                BoatSpawnManager.instance.ResetAllBoats();

                break;

            case GAME_STATE.GAMEPLAY:
                GridCellManager.instance.ToggleEnemyGrid(true);
                //GUIManager.instance.ToggleListenButton(true);
                //GameObject.Find("ARCamera").GetComponent<TouchRaycast>().enabled = false;
                break;
            case GAME_STATE.GAMEPLAY_TURN:
                GUIManager.instance.ToggleListenButton(true);
                break;
            case GAME_STATE.GAMEPLAY_WAIT:
                GUIManager.instance.ToggleListenButton(false);
                break;
            case GAME_STATE.GAMEOVER_WIN:
                m_clientScore++;
                GUIManager.instance.ToggleListenButton(false);
                GUIManager.instance.ToggleReplayButton(true);
                GameObject.Find("ARCamera").GetComponent<GameplayTouchRaycast>().enabled = false;
                GridCellManager.instance.ToggleWinMessage(true);
                break;
            case GAME_STATE.GAMEOVER_LOSE:
                m_enemyScore++;
                GUIManager.instance.ToggleListenButton(false);
                GUIManager.instance.ToggleReplayButton(true);
                GameObject.Find("ARCamera").GetComponent<GameplayTouchRaycast>().enabled = false;
                GridCellManager.instance.ToggleLoseMessage(true);
                break;
            case GAME_STATE.DISCONNECTED:

                break;
        }

        m_currentState = newState;
    }

    void ReceiveMessage(NetworkMessage message)
    {
        string text = message.ReadMessage<StringMessage>().value;
        Debug.Log("Received message : " + text);

        string[] splitText = text.Split(':');

        Debug.Log("Connection ID : " + splitText[0]);
        int connid = int.Parse(splitText[0]);

        if (splitText[1].Equals("gameplay"))
        {
            if (splitText[2].Equals("startGame"))
            {
                MessageController.instance.AddMessageTween("Let's play!!");
                ChangeGameState(GAME_STATE.GAMEPLAY);

                //If this client is the host
                if (client.connection.connectionId == 0)
                {
                    //Host turn is first
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                    {
                        MessageController.instance.AddMessageTween("It's your turn first! Good luck!");
                        ChangeGameState(GAME_STATE.GAMEPLAY_TURN);
                        SendServerMessage("wait", "gameplay");
                    }
                    //Client turn is first
                    else
                    {
                        MessageController.instance.AddMessageTween("It's your enemy's turn first! Good luck!");
                        ChangeGameState(GAME_STATE.GAMEPLAY_WAIT);
                        SendServerMessage("turn", "gameplay");
                    }
                }
            }
        }
        else if (splitText[1].Equals("replay"))
        {
            if (splitText[2].Equals("replay"))
            {
                MessageController.instance.AddMessageTween("Score is - YOU : " + m_clientScore + " ENEMY : " + m_enemyScore);
                ChangeGameState(GAME_STATE.INTRO);
            }
        }


        //Get rid of messages sent to itself
        //**************************************************************************
        if (client.connection.connectionId == connid)
        {
            Debug.Log("Removing message due to it being same client");
            return;
        }

        //Compare the tag part of the message being sent
        Debug.Log("Tag : " + splitText[1]);

        //If we're receiving a "fire" message
        if (splitText[1].Equals("fire"))
        {
            Debug.Log("Firing at cell " + splitText[2]);

            MessageController.instance.AddMessageTween("Enemy fired at your boat in cell " + splitText[2] + " !");


            GridCell cell = GridCellManager.instance.GetGridCellByTag(splitText[2]);
            if (cell != null)
            {
                if (cell.m_blockingObj != null)
                {
                    if (!cell.m_bIsHit)
                    {
                        Debug.Log("Cell " + splitText[2] + " HIT!!");
                        cell.m_bIsHit = true;
                        //cell.GetComponent<Renderer>().material.color = Color.red;

                        MessageController.instance.AddMessageTween("It was a hit!");

                        SendServerMessage(splitText[2], "hit");
                        m_boatsHit++;
                        Debug.Log("Boats hit = " + m_boatsHit);

                        //If all boat components are hit
                        if (m_boatsHit >= 17)
                        {
                            Debug.Log("ALL BOATS HIT");
                            MessageController.instance.AddMessageTween("Oh no, you lose!! Better luck next time");
                            ChangeGameState(GAME_STATE.GAMEOVER_LOSE);

                            SendServerMessage("win", "gameover");
                        }
                    }
                }
                else
                {
                    SendServerMessage(splitText[2], "miss");

                    MessageController.instance.AddMessageTween("It was a miss!");

                }
            }

            MessageController.instance.AddMessageTween("It's your turn! Fire away!");
            ChangeGameState(GAME_STATE.GAMEPLAY_TURN);
            SendServerMessage("wait", "gameplay");
        }

        else if (splitText[1].Equals("hit"))
        {
            GridCellManager.instance.SetEnemyCell(splitText[2], true);
        }
        else if (splitText[1].Equals("miss"))
        {
            GridCellManager.instance.SetEnemyCell(splitText[2], false);
        }

        //If we're receiving a "setup" message
        else if (splitText[1].Equals("setup"))
        {

            //If it's a client ready message
            if (splitText[2].Equals("clientReady"))
            {
                //If we're both ready, start combat
                if (m_bClientReadyToPlay)
                {
                    SendServerMessage("bothReady", "setup");
                }
                else
                {
                    MessageController.instance.AddMessageTween("Enemy is ready to play.. Place your ships down to ready up!");
                }
            }

            if (splitText[2].Equals("bothReady"))
            {
                SendServerMessage("startGame", "gameplay");
            }
        }

        else if (splitText[1].Equals("replay"))
        {
            if (splitText[2].Equals("clientReady"))
            {
                if (m_bClientReplayReady)
                {
                    SendServerMessage("bothReady", "replay");
                }
            }

            if (splitText[2].Equals("bothReady"))
            {
                SendServerMessage("replay", "replay");
            }
        }

        else if (splitText[1].Equals("gameplay"))
        {
            if (splitText[2].Equals("turn"))
            {
                ChangeGameState(GAME_STATE.GAMEPLAY_TURN);
            }
            else if (splitText[2].Equals("wait"))
            {
                MessageController.instance.AddMessageTween("Enemy's turn..");
                ChangeGameState(GAME_STATE.GAMEPLAY_WAIT);
            }
        }
        else if (splitText[1].Equals("gameover"))
        {
            if (splitText[2].Equals("win"))
            {
                MessageController.instance.AddMessageTween("You win! Congratulations!!");
                ChangeGameState(GAME_STATE.GAMEOVER_WIN);
            }
        }
    }

    public void SendServerMessage(string text)
    {
        StringMessage message = new StringMessage();
        message.value = text;

        Debug.Log("Sending message : " + text);

        client.Send(serverMessage, message);
    }

    public void SendServerMessage(string text, string tag)
    {
        StringMessage message = new StringMessage();

        message.value = tag + ":" + text;
        Debug.Log("Sending message : " + message.value);

        client.Send(serverMessage, message);
    }

    void ServerReceiveMessage(NetworkMessage message)
    {
        StringMessage myMessage = new StringMessage();

        myMessage.value = message.conn.connectionId + ":" + message.ReadMessage<StringMessage>().value;

        Debug.Log("Sending " + myMessage.value + "to all clients");

        NetworkServer.SendToAll(serverMessage, myMessage);
    }
}
