using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Networking;

using UnityEngine.SceneManagement;

using Vuforia;

public class Menu : MonoBehaviour
{
    public RectTransform m_hostBtn;
    public RectTransform m_joinBtn;
    public RectTransform m_connectBtn;

    public RectTransform m_joinTxtBox;
    public RectTransform m_backBtn;
    public RectTransform m_exitBtn;

    public RectTransform m_loadingTxtRect;
    Text m_loadingTxt;

    public RectTransform m_settingsPanel;
    bool m_settingsPanelActive;


    public InputField m_ipAddressField;

    public Text m_errorText;

    bool m_isLoading;

    bool m_consoleActive;

    int m_loadingTxtIncrementer;
    float m_loadingTxtDeltaT;

    // Use this for initialization
    void Start()
    {
        //Disable background plane object
        BackgroundPlaneBehaviour bg = GameObject.Find("ARCamera").transform.GetChild(0).GetComponent<BackgroundPlaneBehaviour>();
        if(bg != null)
        {
            bg.enabled = false;
        }

        DontDestroyOnLoad(this);

        m_loadingTxtIncrementer = 0;
        m_loadingTxtDeltaT = 0;

        m_loadingTxt = m_loadingTxtRect.GetComponent<Text>();
        m_settingsPanelActive = false;
        m_consoleActive = false;
    }

    void Update()
    {
        if (m_isLoading)
        {
            Debug.Log("loading text update");
            m_loadingTxtDeltaT += Time.deltaTime;
            if (m_loadingTxtDeltaT > 0.1f)
            {
                Debug.Log("increment counter");
                m_loadingTxtIncrementer++;
                if (m_loadingTxtIncrementer > 3)
                {
                    m_loadingTxtIncrementer = 0;
                }

                switch (m_loadingTxtIncrementer)
                {
                    case 0:
                        m_loadingTxt.text = "Loading    ";
                        break;
                    case 1:
                        m_loadingTxt.text = "Loading.   ";
                        break;
                    case 2:
                        m_loadingTxt.text = "Loading..  ";
                        break;
                    case 3:
                        m_loadingTxt.text = "Loading... ";
                        break;
                }

                m_loadingTxtDeltaT = 0;
            }
        }
    }

    public void ActivateJoinScreen()
    {
        m_hostBtn.gameObject.SetActive(false);
        m_joinBtn.gameObject.SetActive(false);

        m_exitBtn.gameObject.SetActive(false);

        m_connectBtn.gameObject.SetActive(true);

        m_errorText.gameObject.SetActive(true);
        m_errorText.GetComponentInChildren<Text>().text = "";
        m_joinTxtBox.gameObject.SetActive(true);

        m_backBtn.gameObject.SetActive(true);
        
    }

    public void ActivateMainScreen()
    {
        m_errorText.gameObject.SetActive(false);
        m_joinTxtBox.gameObject.SetActive(false);
        m_backBtn.gameObject.SetActive(false);
        m_connectBtn.gameObject.SetActive(false);

        m_hostBtn.gameObject.SetActive(true);
        m_joinBtn.gameObject.SetActive(true);
        m_exitBtn.gameObject.SetActive(true);
    }

    public void HostGame()
    {
       StartCoroutine(LoadGame("Gameplay", StartHost));
    }

    public void JoinHost()
    {
       StartCoroutine(LoadGame("Gameplay", StartClient));
    }

    void StartHost()
    {
        NetworkClient client = ARNetworkManager.instance.StartHost();
        Debug.Log("Starting host");
    }

    void StartClient()
    {
        string value = m_ipAddressField.text;
        ARNetworkManager.instance.networkAddress = value;
        ARNetworkManager.instance.StartClient();
    }

    private IEnumerator LoadGame(string strLevel, System.Action OnFinish)
    {
        Debug.Log("Loading Level");
        m_isLoading = true;
        m_hostBtn.gameObject.SetActive(false);
        m_joinBtn.gameObject.SetActive(false);
        m_connectBtn.gameObject.SetActive(false);

        m_loadingTxtRect.gameObject.SetActive(true);

        m_joinTxtBox.gameObject.SetActive(false);
        while(m_isLoading)
        {
            yield return SceneManager.LoadSceneAsync(strLevel);

            m_isLoading = false;
        }

        Debug.Log("Level Load complete");
        OnFinish();

        Destroy(this.gameObject);
    }



    public void ToggleSettingsPanel()
    {
        m_settingsPanelActive = !m_settingsPanelActive;
        m_settingsPanel.gameObject.SetActive(m_settingsPanelActive);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void ToggleConsole()
    {
        m_consoleActive = !m_consoleActive;
        GameObject.Find("Console").GetComponent<ARShips.Console>().enabled = m_consoleActive;
    }
}
