using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Door _spawnDoor;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private int _floorNumber = 1;
    private int gravity = 1;
    private Rigidbody2D _rigidbody;
    private Vector2 _direction;
    private Transform _playerTransform;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        Physics2D.IgnoreCollision(PlayerController.Instance.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Nenhum objeto com tag 'Player' encontrado na cena!");
        }
    }

    void Update()
    {
        if (_playerTransform != null)
        {
            Move();
        }
    }

    private void Move()
    {
        if (_playerTransform == null) return;
        transform.position = Vector2.MoveTowards(transform.position, _playerTransform.position, _moveSpeed * Time.deltaTime);
    }

    /*public void Activate()
    {
        _isActive = true;
        transform.position = _spawnDoor.transform.position;
        _direction = Random.value > 0.5f ? Vector2.left : Vector2.right;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        _isActive = false;
        gameObject.SetActive(false);
    }*/

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Enemy"))
        {
           gravity *= 0;
        }
    }
}