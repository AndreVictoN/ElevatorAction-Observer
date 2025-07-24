using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerProjectile : ProjectileBase
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(_tagEnemy) && !collision.gameObject.CompareTag("EyeSight"))
        {
            Destroy(collision.gameObject);
            GameManager.Instance.GetEnemies().Remove(collision.gameObject.GetComponent<Enemy>());
            GameManager.Instance.UpdateScore(100);
            Destroy(this.gameObject);
        }
    }
}
