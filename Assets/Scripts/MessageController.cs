using System.Collections;
using System.Collections.Generic;

using DG.Tweening;


using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    public static MessageController instance = null;

    public RectTransform m_messagePanel;
    Text m_messageText;

    int increment = 0;

    float originalPosY;
    float activePosY;

    int m_messageIndx = -1;
    bool m_messagePlaying = false;


    List<Message> m_messageQueue;

    struct Message
    {
        public Sequence sequence;
        public string textMessage;
    }

    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            instance = null;
        }

        m_messageQueue = new List<Message>();


        m_messageText = m_messagePanel.GetComponentInChildren<Text>();


        originalPosY = m_messagePanel.localPosition.y;
        activePosY = m_messagePanel.localPosition.y - 240.0f;
    }

    void TryPlayNextMessage()
    {

        m_messageQueue[0].sequence.Kill(true);
        m_messageQueue.RemoveAt(0);

        m_messagePlaying = false;


        if(m_messageQueue.Count > 0)
        {
            if (m_messageQueue[0].sequence != null)
            {
                PlayTween();
            }
        }
    }

    public void AddMessageTween(string message)
    {

        Message newMessage;

        newMessage.textMessage = message;

        Debug.Log("Adding message '" + message + "' to the queue");

        Sequence tweenSequence = DOTween.Sequence();
        
        tweenSequence.Append(m_messagePanel.DOLocalMoveY(activePosY, 1.25f).SetEase(Ease.OutQuad));
        tweenSequence.Append(m_messagePanel.DOLocalMoveY(activePosY, 2.0f));
        tweenSequence.Append(m_messagePanel.DOLocalMoveY(originalPosY, 1.25f).SetEase(Ease.OutQuad));

        tweenSequence.OnComplete(() =>
        {
            TryPlayNextMessage();
        });

        newMessage.sequence = tweenSequence;
        m_messageQueue.Add(newMessage);

        m_messageIndx++;
        if(!m_messagePlaying)
        {
            PlayTween();
        }
    }

    void PlayTween()
    {
        m_messageQueue[0].sequence.Play();
        m_messageText.text = m_messageQueue[0].textMessage;

        m_messagePlaying = true;
    }
}
