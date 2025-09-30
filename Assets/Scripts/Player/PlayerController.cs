using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;

public class PlayerController : Core.Singleton.NetworkSingleton<PlayerController>, IObserver
{
    public Rigidbody2D myRigidBody;
    public Animator myAnimator;
    
    [SerializeField]private List<Teleporters> _teleporters = new();
    [SerializeField]private List<InvisibleWalls> _invisibleWalls;
    [SerializeField] private float _crouchingJumpForce;
    [SerializeField] private int _currentElevatorFloor;
    [SerializeField] private float _crouchingSpeed;
    [SerializeField] private float _baseJumpForce;
    [SerializeField] private int _myCurrentFloor;
    private string _tagFinalDoor = "FinalDoor";
    [SerializeField] private float _baseSpeed;
    private string _tagElevator = "Elevator";
    private Vector2 _defaultColliderOffset;
    private Vector2 _defaultColliderSize;
    public bool _canChangeLevel = false;
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
    [SerializeField]private bool _collidingDown;
    [SerializeField]private bool _collidingUp;
    private Teleporters _currentTeleporter;
    private bool _isChangingFloor;
    private Door _currentDoor;
    private float _jumpForce;
    private float _elevatorX;
    private bool _isWalking;
    private bool _isJumping;
    private float _playerHealth;
    private bool _inElevator;

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
            "FOURTEENTH_FLOOR" => 14,
            "FIFTEENTH_FLOOR" => 15,
            "SIXTEENTH_FLOOR" => 16,
            "SEVENTEENTH_FLOOR" => 17,
            "EIGHTEENTH_FLOOR" => 18,
            "NINETEENTH_FLOOR" => 19,
            "TWENTIETH_FLOOR" => 20,
            _ => _currentElevatorFloor
        };

        if (evt == EventsEnum.PLAYER_IN_ELEVATOR)
        {
            _elevatorX = 0.5f / this.transform.localScale.x;
            this.gameObject.transform.localScale = new Vector2(_elevatorX, this.transform.localScale.y);
            _isOnElevator = true;
        }
        else if (evt == EventsEnum.PLAYER_NOT_IN_ELEVATOR)
        {
            this.gameObject.transform.localScale = _defaultScale;
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
        if (_invisibleWalls.Count <= 0)
        {
            _invisibleWalls.Clear();
            _invisibleWalls.AddRange(GameObject.FindObjectsOfType<InvisibleWalls>());
        }
        
        _teleporters.AddRange(GameObject.FindObjectsOfType<Teleporters>());
        myRigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        _defaultScale = this.gameObject.transform.localScale;
        _currentElevatorFloor = 1;
        _isCrouching = false;
        _myCurrentFloor = 1;
        _isOnFloor = false;
        _isOnElevator = false;
        _collidingDown = false;
        _collidingUp = false;
        _defaultColliderOffset = this.gameObject.GetComponent<BoxCollider2D>().offset;
        _defaultColliderSize = this.gameObject.GetComponent<BoxCollider2D>().size;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMovement();
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleJump();
        HandleActions();
        HandleAnimation();
        HandleDoorBehaviour();
        HandleInvisibleWallCollision();

        if(this.gameObject.transform.parent != null)
        {
            if(this.gameObject.transform.parent.gameObject.CompareTag("Elevator"))
            {
                _inElevator = true;
            }else{
                _inElevator = false;
            }
        }else{
            _inElevator = false;
        }
    }

    public bool IsInElevtor(){return _inElevator;}

    private void HandleMovement()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;

        if (_isCrouching) { _currentSpeed = _crouchingSpeed; }
        else { _currentSpeed = _baseSpeed; }
        if(_isOnElevator)
        {
            float direction = Mathf.Sign(this.transform.localScale.x);
            _elevatorX = direction * (0.75f / 0.6f);
        }

        if(Input.GetKey(KeyCode.RightArrow) && !_inDoor && !_isChangingFloor)
        {
            if(!_isCrouching && !_isChangingFloor) {myRigidBody.velocity = new Vector2(_currentSpeed, myRigidBody.velocity.y); _isWalking = true;}
            if(!_isOnElevator){this.gameObject.transform.localScale = new Vector2(0.75f, this.gameObject.transform.localScale.y);}
            else{if(_elevatorX < 0)this.gameObject.transform.localScale = new Vector2(-_elevatorX, this.gameObject.transform.localScale.y);}
        }else if(Input.GetKey(KeyCode.LeftArrow) && !_inDoor && !_isChangingFloor)
        {
            if(!_isCrouching && !_isChangingFloor) {myRigidBody.velocity = new Vector2(-_currentSpeed, myRigidBody.velocity.y); _isWalking = true;}
            if(!_isOnElevator){this.gameObject.transform.localScale = new Vector2(-0.75f, this.gameObject.transform.localScale.y);}
            else{if(_elevatorX > 0)this.gameObject.transform.localScale = new Vector2(-_elevatorX, this.gameObject.transform.localScale.y);}
        }
        
        if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || _isChangingFloor)
        {
            myRigidBody.velocity = Vector2.zero;
        }

        if(myRigidBody.velocity == Vector2.zero) _isWalking = false;
    }

    private void HandleAnimation()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;
        
        if (_isWalking && !_isJumping && !_isCrouching)
        {
            myAnimator.Play("samusRunning");
        }
        else if (!_isJumping && !_isCrouching)
        {
            myAnimator.Play("samusIdle");
        }
        else if (_isJumping && !_isCrouching)
        {
            myAnimator.Play("samusJump");
        }
        else if (!_isJumping && !_isWalking && _isCrouching)
        {
            myAnimator.Play("samusCrouch");
        }
    }

    private void HandleJump()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;

        if (_isCrouching) { _jumpForce = _crouchingJumpForce; }
        else { _jumpForce = _baseJumpForce; }

        if(_isOnFloor && Input.GetKeyDown(KeyCode.UpArrow) && !_inDoor && !_isOnElevator)
        {
            myRigidBody.velocity = Vector2.up * _jumpForce;
            _isJumping = true;
        }

        if(myRigidBody.velocity.y == 0)
        {
            _isJumping = false; 
        }
    }

    private void HandleActions()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;
        
        if (Input.GetKeyDown(KeyCode.DownArrow) && _isOnFloor && !_inDoor && !_isOnElevator)
        {
            _currentTween?.Kill();

            if (!_isCrouching)
            {
                this.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(_defaultColliderSize.x, _defaultColliderSize.y * 0.2f);
                this.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(_defaultColliderOffset.x, -0.43f);
                _isCrouching = true;
            }
            else
            {
                this.gameObject.GetComponent<BoxCollider2D>().size = _defaultColliderSize;
                this.gameObject.GetComponent<BoxCollider2D>().offset = _defaultColliderOffset;
                _isCrouching = false;
            }
        }

        if(_currentTeleporter != null && Input.GetKeyDown(KeyCode.X) && _isOnFloor && !_inDoor && (_collidingUp || _collidingDown))
        {
            _isChangingFloor = true;
            _currentTween?.Kill();
            _currentTween = _currentTeleporter.MovePlayer(this.gameObject, this.gameObject.transform.localPosition.y);
            this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            _currentTween.OnComplete(() => {this.gameObject.GetComponent<BoxCollider2D>().enabled = true; _isChangingFloor = false;});
        }
        
        if(Input.GetKeyDown(KeyCode.X) && _isOnFloor && !_inDoor && _canChangeLevel)
        {
            this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
            this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            
            StartCoroutine(ChangeLevel());
        }
    }

    private IEnumerator ChangeLevel()
    {
        yield return new WaitForSeconds(0.3f);
        if(SceneManager.GetActiveScene().name == "Level01") GameManager.Instance.GoToNextLevel();
        else{GameManager.Instance.GameOver();};
    }

    private void HandleInvisibleWallCollision()
    {
        if(_invisibleWalls.Count <= 0) return;

        if(_currentElevatorFloor < _myCurrentFloor || _currentElevatorFloor == _myCurrentFloor + 1 || _currentElevatorFloor == _myCurrentFloor)
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
        if(collision.gameObject.CompareTag(_tagFloor) || collision.gameObject.CompareTag("FinalFloor"))
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
        Transform parent = col.transform.parent;
        string tag = null;

        if(parent != null)
        {
            foreach(Transform sibling in parent)
            {
                if(sibling.gameObject.name == "InvisibleWalls")
                {
                    tag = sibling.gameObject.tag;
                    break;
                }
            }

            if(tag != null) _myCurrentFloor = GameManager.Instance.floorKeys[tag];
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(_tagDoor))
        {
            _canEnterDoor = true;
            _currentDoor = collision.gameObject.GetComponent<Door>();
        }else if(collision.CompareTag(_tagFinalDoor))
        {
            _canChangeLevel = true;
        }else if(_teleporters.Contains(collision.gameObject.GetComponent<Teleporters>()))
        {
            if(collision.gameObject.GetComponent<Teleporters>() as UpTeleporter)
            {
                _collidingUp = true; _collidingDown = false;
            }else if(collision.gameObject.GetComponent<Teleporters>() as DownTeleporter){
                _collidingUp = false; _collidingDown = true;
            }

            _currentTeleporter = collision.gameObject.GetComponent<Teleporters>();
        }
    }

    private void HandleDoorBehaviour()
    {
        if(_currentDoor != null)
        {
            if(_canEnterDoor && Input.GetKeyDown(KeyCode.X) && _currentDoor.GetIsActive())
            {
                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
                GameManager.Instance.PlayAudio(1);
                _canEnterDoor = false;
                _inDoor = true;
            }else if(_inDoor && Input.GetKeyDown(KeyCode.X) && _currentDoor.GetIsActive())
            {
                _currentDoor.SetIsActive(false);

                this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
                this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
                GameManager.Instance.UpdateScore(500);
                GameManager.Instance.PlayAudio(2);
                _inDoor = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag(_tagDoor))
        {
            _canEnterDoor = false;

            _currentDoor = null;
        }else if(collision.CompareTag(_tagFinalDoor))
        {
            _canChangeLevel = false;
        }else if(_teleporters.Contains(collision.gameObject.GetComponent<Teleporters>()))
        {
            _collidingUp = false; _collidingDown = false;

            _currentTeleporter = null;
        }
    }
    
    public void SetCurrentFloor(int floor)
    {
        _myCurrentFloor = floor;
    }

    public int GetCurrentFloor() { return _myCurrentFloor; }
    public int GetElevatorFloor() { return _currentElevatorFloor; }
    public bool IsCrouching(){ return _isCrouching; }
    public bool IsInDoor(){ return _inDoor; }
}
