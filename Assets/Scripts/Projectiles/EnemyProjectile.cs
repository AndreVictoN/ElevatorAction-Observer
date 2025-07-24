using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyProjectile : ProjectileBase
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(_tagPlayer))
        {
            if(!collision.GetComponent<PlayerController>().IsInDoor()) Destroy(collision.gameObject);
            Destroy(this.gameObject);
        }
    }
}