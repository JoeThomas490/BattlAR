using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

using UnityEngine.Networking;

public class GUIManager : MonoBehaviour
{
    public static GUIManager instance = null;

    public RectTransform m_settingsPanel;
    bool m_settingsPanelActive = false;

    public RectTransform m_exitPanel;
    bool m_exitPanelActive = false;

    public RectTransform m_instructionsPanel;
    bool m_instructionsPanelActive = false;

    public RectTransform m_listenButton;

    public RectTransform m_replayButton;
    public RectTransform m_replayMessagePanel;


    bool m_consoleActive = false;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void ToggleSettingsPanel()
    {
        m_settingsPanelActive = !m_settingsPanelActive;
        m_settingsPanel.gameObject.SetActive(m_settingsPanelActive);

        if(m_settingsPanelActive && m_exitPanelActive)
        {
            ToggleExitPanel();
        }
    }

    public void ExitGame()
    {
        ARNetworkManager.instance.Disconnect();
        SceneManager.LoadScene("Menu");
    }

    public void ToggleExitPanel()
    {
        m_exitPanelActive = !m_exitPanelActive;
        m_exitPanel.gameObject.SetActive(m_exitPanelActive);

        if(m_settingsPanelActive && m_exitPanelActive)
        {
            ToggleSettingsPanel();
        }
    }

    public void ToggleInstructionsPanel()
    {
        m_instructionsPanelActive = !m_instructionsPanelActive;
        m_instructionsPanel.gameObject.SetActive(m_instructionsPanelActive);
    }

    void Update()
    {
       
    }

    public void ToggleListenButton(bool active)
    {
        m_listenButton.gameObject.SetActive(active);
    }

    public void ToggleReplayButton(bool active)
    {
        m_replayButton.gameObject.SetActive(active);
    }

    public void ToggleReplayMessagePanel(bool active)
    {
        m_replayMessagePanel.gameObject.SetActive(active);
    }

    public void ToggleConsole()
    {
        m_consoleActive = !m_consoleActive;
        GameObject.Find("Console").GetComponent<ARShips.Console>().enabled = m_consoleActive;
    }

    public void RequestReplay()
    {
        ARNetworkManager.instance.RequestReplay();
        ToggleReplayMessagePanel(true);
        ToggleReplayButton(false);
    }
}
