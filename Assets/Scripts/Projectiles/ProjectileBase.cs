using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    protected string _tagPlayer = "Player";
    [SerializeField] protected float speed;
    protected string _tagEnemy = "Enemy";
    protected Rigidbody2D _myRigidBody;
    protected int multiplier;

    void Awake()
    {
        _myRigidBody = this.gameObject.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SetMovement(this.gameObject.transform.parent.parent.localScale.x);
        this.gameObject.transform.SetParent(null);
        StartCoroutine(DestroyCoroutine());
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

    void Update()
    {
        if(_myRigidBody != null && !GameManager.Instance.IsGamePaused()) _myRigidBody.velocity = new Vector2(multiplier*speed, _myRigidBody.velocity.y);
    }

    protected void SetMovement(float localScaleX)
    {
        if(localScaleX < 0)
        {
            multiplier = -1;
        }
        else{
            multiplier = 1;
        }
    }
}
