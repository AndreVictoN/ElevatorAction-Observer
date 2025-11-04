using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : NetworkBehaviour, IDestroyableObjects
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 0.5f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    private Transform _currentTarget;
    private Rigidbody2D _rigidbody;
    private Transform _playerTransform;

    private bool _canSeePlayer;
    private Animator _myAnimator;
    [SerializeField] private float _minShootDistance = 1f;
    private bool _isPlayerSet;

    void Awake()
    {
        _isPlayerSet = false;
        _myAnimator = this.gameObject.GetComponent<Animator>();
        _canSeePlayer = false;
        _rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        PatrolPointManager();

        if (GameManager.Instance.GetPlayerSet())
        {
            _isPlayerSet = true;

            foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None)){
                Physics2D.IgnoreCollision(player.gameObject/*PlayerController.NetInstance*/.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>(), true);
            }
        }
    }

    private void PatrolPointManager()
    {
        Vector3 startPos = transform.position;

        if (patrolPointA == null)
        {
            GameObject a = new GameObject("PatrolPointA");
            if(SceneManager.GetActiveScene().name == "Level02") a.transform.position = transform.position + new Vector3(-1.94f, 0f, 0f);
            else{a.transform.position = startPos + new Vector3(-1f, 0f, 0f);}
            patrolPointA = a.transform;
        }

        if (patrolPointB == null)
        {
            GameObject b = new GameObject("PatrolPointB");
            if(SceneManager.GetActiveScene().name == "Level02") b.transform.position = transform.position + new Vector3(1.94f, 0f, 0f);
            else{b.transform.position = startPos + new Vector3(1f, 0f, 0f);}
            patrolPointB = b.transform; 
        }
        
        _currentTarget = patrolPointA;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(GameManager.Instance.GetPlayerSet() && PlayerController.NetInstance != null) _playerTransform = PlayerController.NetInstance?.transform;
    }


    /*void Start()
    {
        if(GameManager.Instance.GetPlayerSet()) _playerTransform = PlayerController.NetInstance?.transform;
    }*/

    void Update()
    {
        if (!_isPlayerSet && GameManager.Instance.GetPlayerSet())
        {
            _isPlayerSet = GameManager.Instance.GetPlayerSet();
            foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None)){
                Physics2D.IgnoreCollision(player.gameObject/*PlayerController.NetInstance*/.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>(), true);
            }
            _playerTransform = PlayerController.NetInstance?.transform;
        }

        GameManager.Instance.GetEnemies().ForEach(e => {if(e != null && e.gameObject.activeSelf) Physics2D.IgnoreCollision(e.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>(), true);});
        if (_playerTransform != null)
        {
            Move();
        }
    }

    private void Move()
    {
        if (_currentTarget.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (_canSeePlayer)
        {
            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            if (distance > _minShootDistance)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _playerTransform.position,
                    _moveSpeed * Time.deltaTime
                );
                _myAnimator.Play("Enemy");
            }else{
                _rigidbody.velocity = Vector2.zero;
                _myAnimator.Play("enemyIdle");
            }
        }
        else
        {
            if (_currentTarget == null)_currentTarget = patrolPointA;

            transform.position = Vector2.MoveTowards(
                transform.position,
                _currentTarget.position,
                _moveSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, _currentTarget.position) < 0.05f)
            {
                _currentTarget = _currentTarget == patrolPointA ? patrolPointB : patrolPointA;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _canSeePlayer = true;
            _playerTransform = collision.gameObject.transform;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            _canSeePlayer = false;
            _playerTransform = collision.gameObject.transform;
        }
    }

    public void RequestDestroy(){ RequestDestroyThisServerRpc(); }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyThisServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    public bool IsSeeingPlayer() { return _canSeePlayer; }
}