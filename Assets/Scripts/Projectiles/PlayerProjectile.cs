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
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (IsClient && enemy.gameObject.GetComponent<NetworkObject>().IsSpawned) { enemy.RequestDestroy(); }
            else if (IsServer && enemy.gameObject.GetComponent<NetworkObject>().IsSpawned) { enemy.gameObject.GetComponent<NetworkObject>().Despawn(); }
            else { Destroy(enemy.gameObject); }

            GameManager.Instance.GetEnemies().Remove(enemy);
            GameManager.Instance.UpdateScore(100);

            DestroyThisGameObject();
        }
    }
}
