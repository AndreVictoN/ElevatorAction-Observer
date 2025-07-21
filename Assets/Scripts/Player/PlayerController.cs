using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

using UnityEngine.Diagnostics;

public class PlayerController : Core.Singleton.Singleton<PlayerController>, IObserver
{
    public Rigidbody2D myRigidBody;
    
    [SerializeField]private List<InvisibleWalls> _invisibleWalls;
    [SerializeField] private float _crouchingJumpForce;
    [SerializeField] private int _currentElevatorFloor;
    [SerializeField] private float _crouchingSpeed;
    [SerializeField] private float _baseJumpForce;
    [SerializeField] private int _myCurrentFloor;
    [SerializeField] private float _baseSpeed;
    private string _tagElevator = "Elevator";
    private string _tagFloor = "Floor";
    private bool _isOnElevator = false;
    public bool _canEnterDoor = false;
    private bool _isCrouching = false;
    private string _tagDoor = "Door";
    private bool _isOnFloor = false;
    private Vector3 _defaultScale;
    public bool _inDoor = false;
    private float _currentSpeed;
    private Tween _currentTween;
    private Door _currentDoor;
    private float _jumpForce;

    #region Observer
    public void OnNotify(EventsEnum evt)
    {
        string name = evt.ToString();

        int floor = name switch
        {
            "FIRST_FLOOR" => 1,
            "SECOND_FLOOR" => 2,
            "THIRD_FLOOR" => 3,
            "FOURTH_FLOOR" => 4,
            "FIFTH_FLOOR" => 5,
            "SIXTH_FLOOR" => 6,
            "SEVENTH_FLOOR" => 7,
            "EIGHTH_FLOOR" => 8,
            "NINTH_FLOOR" => 9,
            "TENTH_FLOOR" => 10,
            "ELEVENTH_FLOOR" => 11,
            "TWELFTH_FLOOR" => 12,
            "THIRTEENTH_FLOOR" => 13,
            _ => _currentElevatorFloor
        };

        if(evt == EventsEnum.PLAYER_IN_ELEVATOR)
        {
            _isOnElevator = true;
        }else if(evt == EventsEnum.PLAYER_NOT_IN_ELEVATOR)
        {
            _isOnElevator = false;
        }

        _currentElevatorFloor = floor;
    }
    #endregion

    void Start()
    {
        ResetSetup();
    }

    private void ResetSetup()
    {
        myRigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        _defaultScale = this.gameObject.transform.localScale;
        _currentElevatorFloor = 1;
        _isCrouching = false;
        _myCurrentFloor = 1;
        _isOnFloor = false;
        _isOnElevator = false;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        HandleJump();
        HandleActions();
        HandleDoorBehaviour();
        HandleInvisibleWallCollision();
    }

    private void HandleMovement()
    {
        if(_isCrouching){_currentSpeed = _crouchingSpeed;}
        else{_currentSpeed = _baseSpeed;}

        if(Input.GetKey(KeyCode.RightArrow) && !_inDoor)
        {
            myRigidBody.velocity = new Vector2(_currentSpeed, myRigidBody.velocity.y);
        }else if(Input.GetKey(KeyCode.LeftArrow) && !_inDoor)
        {
            myRigidBody.velocity = new Vector2(-_currentSpeed, myRigidBody.velocity.y);
        }
        
        if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            myRigidBody.velocity = Vector2.zero;
        }
    }

    private void HandleJump()
    {
        if(_isCrouching){_jumpForce = _crouchingJumpForce;}
        else{_jumpForce = _baseJumpForce;}

        if(_isOnFloor && Input.GetKeyDown(KeyCode.UpArrow) && !_inDoor && !_isOnElevator)
        {
            myRigidBody.velocity = Vector2.up * _jumpForce;
        }
    }

    private void HandleActions()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow) && _isOnFloor && !_inDoor && !_isOnElevator)
        {
            _currentTween?.Kill();
            
            if(!_isCrouching)
            {
                _currentTween = transform.DOScaleY(_defaultScale.y / 2, 0.1f);
                _isCrouching = true;
            }else{
                _currentTween = transform.DOScaleY(_defaultScale.y, 0.1f);
                _isCrouching = false;
            }
        }
    }

    private void HandleInvisibleWallCollision()
    {
        if(_currentElevatorFloor < _myCurrentFloor || _currentElevatorFloor > _myCurrentFloor + 1 || _currentElevatorFloor == _myCurrentFloor)
        {
            for(int i = _myCurrentFloor - 1; i < _invisibleWalls.Count; i++)
            {
                Physics2D.IgnoreCollision(_invisibleWalls[i].GetComponent<BoxCollider2D>(), this.gameObject.GetComponent<BoxCollider2D>(), true);
            }
        }else{
            for(int i = _myCurrentFloor - 1; i < _invisibleWalls.Count; i++)
            {
                Physics2D.IgnoreCollision(_invisibleWalls[i].GetComponent<BoxCollider2D>(), this.gameObject.GetComponent<BoxCollider2D>(), false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag(_tagFloor))
        {        
            _isOnFloor = true;

            CheckFloor(collision);
        }else if(collision.gameObject.CompareTag(_tagElevator))
        {
            _myCurrentFloor = _currentElevatorFloor;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag(_tagFloor))
        {
            _isOnFloor = false;
        }
    }

    private void CheckFloor(Collision2D col)
    {
        float y = col.transform.parent.localPosition.y;

        switch (y)
        {
            case float f when Mathf.Abs(f - 1.112f) < 0.01f:
                _myCurrentFloor = 1;
                break;
            case float f when Mathf.Abs(f + 1.112f) < 0.01f:
                _myCurrentFloor = 2;
                break;
            case float f when Mathf.Abs(f + 3.336f) < 0.01f:
                _myCurrentFloor = 3;
                break;
            case float f when Mathf.Abs(f + 5.336f) < 0.01f:
                _myCurrentFloor = 4;
                break;
            case float f when Mathf.Abs(f + 7.336f) < 0.01f:
                _myCurrentFloor = 5;
                break;
            case float f when Mathf.Abs(f + 9.336f) < 0.01f:
                _myCurrentFloor = 6;
                break;
            case float f when Mathf.Abs(f + 11.336f) < 0.01f:
                _myCurrentFloor = 7;
                break;
            case float f when Mathf.Abs(f + 13.336f) < 0.01f:
                _myCurrentFloor = 8;
                break;
            case float f when Mathf.Abs(f + 15.336f) < 0.01f:
                _myCurrentFloor = 9;
                break;
            case float f when Mathf.Abs(f + 17.336f) < 0.01f:
                _myCurrentFloor = 10;
                break;
            case float f when Mathf.Abs(f + 19.336f) < 0.01f:
                _myCurrentFloor = 11;
                break;
            case float f when Mathf.Abs(f + 21.336f) < 0.01f:
                _myCurrentFloor = 12;
                break;
            case float f when Mathf.Abs(f + 23.336f) < 0.01f:
                _myCurrentFloor = 13;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(_tagDoor))
        {
            _canEnterDoor = true;
            _currentDoor = collision.gameObject.GetComponent<Door>();
        }
    }

    private void HandleDoorBehaviour()
    {
        if(_currentDoor != null)
        {
            if(_canEnterDoor && Input.GetKeyDown(KeyCode.X) && _currentDoor.GetIsActive())
            {
                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -1;
                _canEnterDoor = false;
                _inDoor = true;
            }else if(_inDoor && Input.GetKeyDown(KeyCode.X) && _currentDoor.GetIsActive())
            {
                _currentDoor.SetIsActive(false);

                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
                _inDoor = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag(_tagDoor))
        {
            _canEnterDoor = true;

            _currentDoor = null;
        }
    }
    
    public void SetCurrentFloor(int floor)
    {
        _myCurrentFloor = floor;
    }

    public int GetCurrentFloor() { return _myCurrentFloor; }
}
