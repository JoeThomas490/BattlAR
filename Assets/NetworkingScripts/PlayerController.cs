using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public GameObject m_bulletPrefab;
    public Transform m_bulletSpawnTransform;

    //Use this to call init on local players
    public override void OnStartLocalPlayer()
    {
        GetComponent<Renderer>().material.color = Color.blue;
    }


    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

#if UNITY_STANDALONE

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);


        if(Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }
#endif
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 2)
            {
                CmdFire();
            }
            else
            {
                if (Input.GetTouch(0).phase == TouchPhase.Stationary)
                {
                    var x = Time.deltaTime * 150.0f;
                    transform.Rotate(0, x, 0);
                }
            }
        }
#endif
    }

    [Command]
    void CmdFire()
    {
        var bullet = (GameObject)Instantiate(m_bulletPrefab, m_bulletSpawnTransform.position, m_bulletSpawnTransform.rotation);

        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6f;

        NetworkServer.Spawn(bullet);

        Destroy(bullet, 2.0f);
    }
}