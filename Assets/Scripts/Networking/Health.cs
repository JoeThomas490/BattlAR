using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
    [SyncVar(hook = "OnChangeHealth")]
    public int m_currentHealth = m_maxHealth;

    public const int m_maxHealth = 100;

    public RectTransform m_healthBarUI;

    public bool m_destroyOnDeath;

    public void TakeDamage(int damage)
    {
        if (!isServer)
        {
            return;
        }

        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            if (m_destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                m_currentHealth = m_maxHealth;
                RpcRespawn();
            }
        }

    }

    [ClientRpc]
    void RpcRespawn()
    {
        if (isLocalPlayer)
        {
            transform.position = Vector3.zero;
        }
    }

    public void OnChangeHealth(int health)
    {
        m_healthBarUI.sizeDelta = new Vector2(health, m_healthBarUI.sizeDelta.y);

    }
}
