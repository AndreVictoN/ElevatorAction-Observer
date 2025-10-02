using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProjectileBase : NetworkBehaviour, IDestroyableObjects
{
    protected string _tagPlayer = "Player";
    [SerializeField] protected float speed;
    protected string _tagEnemy = "Enemy";
    protected Rigidbody2D _myRigidBody;
    protected int multiplier;
    private GameObject _myParent;
    private bool _isMoving;

    void Awake()
    {
        _isMoving = false;
        _myRigidBody = this.gameObject.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        //if (transform.parent != null && transform.parent.parent != null)
        //{
        //SetMovement(transform/*.parent.parent*/.localScale.x);
        //}

        if (this.gameObject.CompareTag("EnemyProjectile") && IsHost) { _myParent = this.transform.parent.gameObject; }

        this.gameObject.transform.SetParent(null);
        StartCoroutine(DestroyCoroutine());
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(1f);

        DestroyThisGameObject();
    }

    protected void DestroyThisGameObject()
    {
        if (IsServer)
        {
            if (this.gameObject.GetComponent<NetworkObject>().IsSpawned) GetComponent<NetworkObject>().Despawn();
        }
        else if (IsClient && !IsServer)
        {
            RequestDestroyThisServerRpc();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyThisServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    void FixedUpdate()
    {
        if (_isMoving && _myParent != null) return;
        SetMovement();
    }

    void Update()
    {
        if (_myRigidBody != null && !GameManager.Instance.IsGamePaused()) _myRigidBody.velocity = new Vector2(multiplier * speed, _myRigidBody.velocity.y);
    }

    protected void SetMovement(/*float localScaleX*/)
    {
        if (_isMoving) return;

        float localScaleX = 1f;

        if (this.gameObject.CompareTag("PlayerProjectile"))
        {
            if ((IsHost || IsClient) && PlayerController.NetInstance.IsSpawned) localScaleX = PlayerController.NetInstance.transform.localScale.x;
            else if ((!IsHost && !IsClient) && PlayerController.NetInstance != null) localScaleX = PlayerController.NetInstance.transform.localScale.x;
        }
        else if (this.gameObject.CompareTag("EnemyProjectile"))
        {
            if (_myParent == null) return;
            if ((IsHost || IsClient) && _myParent.GetComponent<NetworkObject>().IsSpawned) localScaleX = _myParent.transform.localScale.x;
            else if (!IsHost && !IsClient && transform.parent.gameObject != null) localScaleX = _myParent.transform.localScale.x;
        }

        multiplier = (localScaleX < 0) ? -1 : 1;

        _isMoving = true;
    }

    public void SetMyParent(GameObject parent)
    {
        _myParent = parent;
        if(!IsHost) Debug.Log(_myParent);
    }
}
