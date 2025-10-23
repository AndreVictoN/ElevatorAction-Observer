using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Unity.Netcode;

public class ElevatorManager : NetworkSubject
{
    private List<int> _alreadyPassed = new();
    private float _initialYPosition;
    private Tween _currentTween;
    [SerializeField]private int _currentFloor;
    private float _scaleY;
    [SerializeField]private bool _wentUp;
    private bool _isChangingFloor = false;
    private int _movementConstant;
    private Dictionary<int, EventsEnum> _floorEventsKeys = new Dictionary<int, EventsEnum>();
    [SerializeField]private bool _playerIn;
    [SerializeField]private bool _playerOnCommand;
    [SerializeField]private bool _playerIsGoingUp;
    private bool _playerIsSubscribed;

    void Awake()
    {
        _playerIsSubscribed = false;
    }

    void Start()
    {
        ResetSetup();
    }

    private void ResetSetup()
    {
        if (GameManager.Instance.GetPlayerSet()){ SetPlayerSubscription(); }

        Subscribe(FindObjectsOfType<InvisibleWalls>());
        Subscribe(FindObjectsOfType<EnemySpawner>());
        InsertIntoDictionary();
        Notify(EventsEnum.FIRST_FLOOR);
        _initialYPosition = transform.position.y;
        _scaleY = transform.localScale.y;
        _movementConstant = 2;
        _playerIn = false;
    }

    void Update()
    {
        if (!_playerIsSubscribed && GameManager.Instance.GetPlayerSet()){ SetPlayerSubscription(); }
        
        HandleMovement();
        CleanAlreadyPassed();
        PlayerControls();
    }

    private void SetPlayerSubscription()
    {
        Subscribe(PlayerController.NetInstance);
        _playerIsSubscribed = true;
    }

    private void CleanAlreadyPassed()
    {
        if (GameManager.Instance.GetPlayerSet() && PlayerController.NetInstance.GetCurrentFloor() == _currentFloor)
        {
            _alreadyPassed.Clear();
        }
    }

    private void HandleMovement()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;
        if (_isChangingFloor) return;
        float y = this.gameObject.transform.position.y;

        if(Mathf.Approximately(y, _initialYPosition))
        {
            _currentFloor = 1;
            if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);
            if(!_playerOnCommand) StartCoroutine(ChangeFloor(_currentFloor, false));
        }
            
        for(int i = 1; i < Enum.GetNames(typeof(EventsEnum)).Length - 2; i++)
        {
            if(Mathf.Approximately(y, _initialYPosition - (i * _movementConstant)))
            {
                _currentFloor = i + 1;
                if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);
                if(!_playerOnCommand) StartCoroutine(ChangeFloor(_currentFloor, false));

                break;
            }
        }
    }

    private void PlayerControls()
    {
        if (GameManager.Instance.IsGamePaused() == true) return;
        
        if (GameManager.Instance.GetPlayerSet() && _playerIn) PlayerController.NetInstance.SetCurrentFloor(_currentFloor);

        if (_playerIn && Input.GetKeyDown(KeyCode.UpArrow) && _currentFloor != 1)
        {
            _playerOnCommand = true;
            _playerIsGoingUp = true;
            StartCoroutine(ChangeFloor(_currentFloor, true));
        }
        else if (_playerIn && Input.GetKeyDown(KeyCode.DownArrow) && _currentFloor != 20)
        {
            _wentUp = false;
            _playerOnCommand = true;
            _playerIsGoingUp = false;
            StartCoroutine(ChangeFloor(_currentFloor, true));
        }
    }

    public IEnumerator ChangeFloor(int currentFloor, bool isPlayer)
    {
        _isChangingFloor = true;

        if (!isPlayer) yield return new WaitForSeconds(3f);

        _currentTween?.Kill();

        CheckCurrentFloor(currentFloor);

        yield return _currentTween.WaitForCompletion();
        _isChangingFloor = false;
        _playerIsGoingUp = false;
    }

    private void CheckCurrentFloor(int currentFloor)
    {
        if (!GameManager.Instance.GetPlayerSet()) return;

        for (int i = 1; i <= 20; i++)
        {
            if (currentFloor <= PlayerController.NetInstance.GetCurrentFloor() - 2) _wentUp = false;
            if (currentFloor == i)
            {
                if (i == 1)
                {
                    _currentTween = transform.DOLocalMoveY(_initialYPosition - 2, 2f);
                    Notify(EventsEnum.SECOND_FLOOR);
                    _wentUp = false;
                    break;
                }
                else if (i == 20)
                {
                    _currentTween = transform.DOLocalMoveY(_initialYPosition - 18 * 2, 2f);
                    Notify(EventsEnum.NINETEENTH_FLOOR);
                    _wentUp = true;
                    break;
                }
                else if (i == 2)
                {
                    if ((PlayerController.NetInstance.GetCurrentFloor() < currentFloor && _alreadyPassed.Contains(PlayerController.NetInstance.GetCurrentFloor() + 2)) || _wentUp || _playerIsGoingUp)
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition, 2f);
                        Notify(EventsEnum.FIRST_FLOOR);
                        _wentUp = true;
                    }
                    else
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - 2 * 2, 2f);
                        Notify(EventsEnum.THIRD_FLOOR);
                        _wentUp = false;
                    }
                    break;
                }
                else
                {
                    if ((PlayerController.NetInstance.GetCurrentFloor() < currentFloor && _alreadyPassed.Contains(PlayerController.NetInstance.GetCurrentFloor() + 2)) || _wentUp || _playerIsGoingUp)
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - (currentFloor - 2) * 2, 2f);
                        Notify(_floorEventsKeys[currentFloor - 1]);
                        _wentUp = true;
                    }
                    else
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - currentFloor * 2, 2f);
                        Notify(_floorEventsKeys[currentFloor + 1]);
                        _wentUp = false;
                    }
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == PlayerController.NetInstance.gameObject)
        {
            _playerIn = true;
            Notify(EventsEnum.PLAYER_IN_ELEVATOR);
            collision.gameObject.transform.SetParent(this.gameObject.transform);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (PlayerController.NetInstance.isActiveAndEnabled)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _playerIn = false;
                _playerOnCommand = false;
                Notify(EventsEnum.PLAYER_NOT_IN_ELEVATOR);
                if (collision.gameObject.activeSelf == true && this.gameObject.activeSelf == true) StartCoroutine(Unparent(collision.gameObject));
            }
        }
    }

    private IEnumerator Unparent(GameObject gO)
    {
        yield return null;
        gO.GetComponent<NetworkObject>().TryRemoveParent();
    }


    #region Floor Events Keys Dictionary
    private void InsertIntoDictionary()
    {
        int floorKey = 0;

        foreach(EventsEnum events in Enum.GetValues(typeof(EventsEnum)))
        {
            if(events == EventsEnum.PLAYER_IN_ELEVATOR || events == EventsEnum.PLAYER_NOT_IN_ELEVATOR) continue;

            floorKey++;
            _floorEventsKeys.Add(floorKey, events);
        }
    }
    #endregion
}