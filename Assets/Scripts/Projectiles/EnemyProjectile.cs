using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class EnemyProjectile : ProjectileBase
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(_tagPlayer))
        {
            if (!collision.GetComponent<PlayerController>().IsInDoor())
            {
                if (NetworkManager.Singleton.IsServer) { collision.gameObject.GetComponent<NetworkObject>().Despawn(); }
                else { Destroy(collision.gameObject); }
            }

            if (!NetworkManager.Singleton.IsServer) Destroy(this.gameObject);
            else this.gameObject.GetComponent<NetworkObject>().Despawn();
        }
    }
}