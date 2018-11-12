using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizeSelection : MonoBehaviour
{
	public GridCellManager gridCellManager;
	public SpeechToTextExample speechToText;

	public Text m_txtCellName;
	public RectTransform m_panelAuthorize;
	public Button m_btnYes, m_btnNo;

    public static bool m_bIsAuthorizing;

	// Use this for initialization
	void Start ()
	{
		m_bIsAuthorizing = false;

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(m_bIsAuthorizing == true)
		{
			speechToText.m_bIsAuthorizing = true;
		}
		else
		{
			speechToText.m_bIsAuthorizing = false;
		}   		
	}

	public void Authorize(string cellName)
	{
		m_bIsAuthorizing = true;

		m_txtCellName.text = cellName;
		m_panelAuthorize.gameObject.SetActive(true);

		m_btnYes.onClick.AddListener(() => {

            ARNetworkManager.instance.SendServerMessage(cellName, "fire");
			m_panelAuthorize.gameObject.SetActive(false);

			m_btnYes.onClick.RemoveAllListeners();
			m_btnNo.onClick.RemoveAllListeners();

			m_bIsAuthorizing = false;
		});

		m_btnNo.onClick.AddListener(() =>
		{
			m_panelAuthorize.gameObject.SetActive(false);
			m_bIsAuthorizing = false;
			m_btnNo.onClick.RemoveAllListeners();
			m_btnYes.onClick.RemoveAllListeners();
		});

	}
}
