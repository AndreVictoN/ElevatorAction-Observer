using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class PlayerProjectile : ProjectileBase
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(_tagEnemy) && !collision.gameObject.CompareTag("EyeSight"))
        {
            if (NetworkManager.Singleton.IsServer) { collision.gameObject.GetComponent<NetworkObject>().Despawn(); }
            else { Destroy(collision.gameObject); }

            GameManager.Instance.GetEnemies().Remove(collision.gameObject.GetComponent<Enemy>());
            GameManager.Instance.UpdateScore(100);

            if (NetworkManager.Singleton.IsServer) this.gameObject.GetComponent<NetworkObject>().Despawn();
            else { Destroy(this.gameObject); }
        }
    }
}
