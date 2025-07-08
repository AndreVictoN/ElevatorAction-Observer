using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IObserver
{
    [SerializeField] private Door _spawnDoor;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private int _floorNumber = 1;
    private bool _isActive = false;
    private Rigidbody2D _rigidbody;
    private Vector2 _direction;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().color = Color.red;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    void Start()
    {

        ElevatorManager elevatorManager = FindObjectOfType<ElevatorManager>();
        if (elevatorManager != null)
        {
            elevatorManager.Subscribe(this);
        }


        Deactivate();
    }

    void Update()
    {
        if (_isActive)
        {
            Move();
        }
    }

    private void Move()
    {
        _rigidbody.velocity = _direction * _moveSpeed;
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
            if (Random.value > 0.4f) // 60% de chance de spawnar
            {
                Activate();
            }
        }
        else
        {
            Deactivate();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Enemy"))
        {
            _direction *= -1; // Inverte a direção ao colidir com paredes ou outros inimigos
        }
    }
}