using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IObserver
{
    [SerializeField] private Door _spawnDoor;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private int _floorNumber = 1;
    private int gravity = 1;
    private bool _isActive = false;
    private Rigidbody2D _rigidbody;
    private Vector2 _direction;
    private Transform _playerTransform; // Renomeado para seguir convenção de nomenclatura

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().color = Color.red;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    void Start()
    {
        // Corrigido: FindGameObjectWithTag (singular) para pegar apenas um jogador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Nenhum objeto com tag 'Player' encontrado na cena!");
        }

        ElevatorManager elevatorManager = FindObjectOfType<ElevatorManager>();
        if (elevatorManager != null)
        {
            elevatorManager.Subscribe(this);
        }
        
        Deactivate();
    }

    void Update()
    {
        if (_isActive && _playerTransform != null)
        {
            Move();
        }
    }

    private void Move()
{
    if (_playerTransform == null) return;
    transform.position = Vector2.MoveTowards(transform.position, _playerTransform.position, _moveSpeed * Time.deltaTime);
}

    public void Activate()
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
    }

    public void OnNotify(EventsEnum evt)
    {
        int elevatorFloor = 1;
        
        if (evt == EventsEnum.FIRST_FLOOR) elevatorFloor = 1;
        else if (evt == EventsEnum.SECOND_FLOOR) elevatorFloor = 2;
        else if (evt == EventsEnum.THIRD_FLOOR) elevatorFloor = 3;

        if (elevatorFloor == _floorNumber && _spawnDoor.GetIsActive())
        {
                Activate();
        }
        else
        {
            Deactivate();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Enemy"))
        {
           gravity *= 0;
        }
    }
}