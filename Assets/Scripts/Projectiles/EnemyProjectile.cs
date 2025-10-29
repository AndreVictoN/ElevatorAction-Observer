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
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (!player.IsInDoor())
            {
                if ((IsClient && !IsHost) && player.gameObject.GetComponent<NetworkObject>().IsSpawned) { player.RequestDestroy(); }
                else if (IsHost && player.gameObject.GetComponent<NetworkObject>().IsSpawned) { player.gameObject.GetComponent<NetworkObject>().Despawn(); }
                else { Destroy(player.gameObject); }
                
                Notify(EventsEnum.PlayerDestroyed);
            }

            DestroyThisGameObject();
        }
    }
}