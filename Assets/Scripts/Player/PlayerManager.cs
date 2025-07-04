using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Rigidbody2D myRigidBody;
    
    [SerializeField] private float _crouchingJumpForce;
    [SerializeField] private float _crouchingSpeed;
    [SerializeField] private float _baseJumpForce;
    [SerializeField] private float _baseSpeed;
    private string _tagFloor = "Floor";
    private bool _isCrouching = false;
    private bool _isOnFloor = false;
    private Vector3 _defaultScale;
    private float _currentSpeed;
    private Tween _currentTween;
    private float _jumpForce;

    void Start()
    {
        myRigidBody = this.gameObject.GetComponent<Rigidbody2D>();
        _isCrouching = false;
        _isOnFloor = false;
        _defaultScale = this.gameObject.transform.localScale;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        HandleJump();
        HandleActions();
    }

    private void HandleMovement()
    {
        if(_isCrouching){_currentSpeed = _crouchingSpeed;}
        else{_currentSpeed = _baseSpeed;}

        if(Input.GetKey(KeyCode.RightArrow))
        {
            myRigidBody.velocity = new Vector2(_currentSpeed, myRigidBody.velocity.y);
        }else if(Input.GetKey(KeyCode.LeftArrow))
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

        if(_isOnFloor && Input.GetKeyDown(KeyCode.UpArrow))
        {
            myRigidBody.velocity = Vector2.up * _jumpForce;
        }
    }

    private void HandleActions()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow) && _isOnFloor)
        {
            if(_currentTween != null) _currentTween.Kill();
            
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag(_tagFloor))
        {
            _isOnFloor = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag(_tagFloor))
        {
            _isOnFloor = false;
        }
    }
}
