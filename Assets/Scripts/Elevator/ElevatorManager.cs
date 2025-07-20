using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class ElevatorManager : Subject
{
    private List<int> _alreadyPassed = new();
    private float _initialYPosition;
    private Tween _currentTween;
    [SerializeField]private int _currentFloor;
    private float _scaleY;
    [SerializeField]private bool _wentUp;
    private bool _isChangingFloor = false;
    private float[] _movementConstant;
    private Dictionary<int, EventsEnum> _floorEventsKeys = new Dictionary<int, EventsEnum>();
    
    void Start()
    {
        Subscribe(PlayerController.Instance);
        Subscribe(FindObjectsOfType<InvisibleWalls>());
        InsertIntoDictionary();
        Notify(EventsEnum.FIRST_FLOOR);
        _initialYPosition = transform.position.y;
        _scaleY = transform.localScale.y;
        _movementConstant = new float[2] { 1.4f*_scaleY, 0.255f};
    }

    void Update()
    {
        HandleMovement();
        CleanAlreadyPassed();
    }

    private void CleanAlreadyPassed()
    {
        if(PlayerController.Instance.GetCurrentFloor() == _currentFloor)
        {
            _alreadyPassed.Clear();
        }
    }

    /*
    private void HandleMovement()
    {
        if(_isChangingFloor) return;
        float y = this.gameObject.transform.position.y;

        if(Mathf.Approximately(y, _initialYPosition))
        {
            _currentFloor = 1;
            if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);
            StartCoroutine(ChangeFloor(_currentFloor));
        }
        
        for(int i = 1; i < Enum.GetNames(typeof(EventsEnum)).Length; i++)
        {
            if(Mathf.Approximately(y, _initialYPosition - (i * _movementConstant[0]) - (i * _movementConstant[1])))
            {
                _currentFloor = i + 1;
                if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);
                StartCoroutine(ChangeFloor(_currentFloor));

                break;
            }
        }
    }*/
    private void HandleMovement() {
        float currentY = this.gameObject.transform.position.y;
        float tolerance = 0.01f;

        for (int i = 0; i < 20; i++)
        {
            float expectedY = _initialYPosition - (2.22f * i);
            if (Mathf.Abs(currentY - expectedY) < tolerance)
            {
                _currentFloor = i + 1;

                if (!_alreadyPassed.Contains(_currentFloor))
                    _alreadyPassed.Add(_currentFloor);

                // Uncomment to trigger events if needed
                // Notify((EventsEnum)_currentFloor);

                StartCoroutine(ChangeFloor(_currentFloor));
                break;
            }
        }
    }

    /*
    public IEnumerator ChangeFloor(int currentFloor)
    {
        _isChangingFloor = true;

        yield return new WaitForSeconds(3f);

        _currentTween?.Kill();

        CheckCurrentFloor(currentFloor);

        yield return _currentTween.WaitForCompletion();
        _isChangingFloor = false;
    }

    private void CheckCurrentFloor(int currentFloor)
    {
        for(int i = 1; i <= 13; i++){
            if(currentFloor == i){
                if(i == 1){
                    _currentTween = transform.DOLocalMoveY(_initialYPosition - _movementConstant[0] - _movementConstant[1], 2f);
                    Notify(EventsEnum.SECOND_FLOOR);
                    _wentUp = false;
                    break;
                }else if(i == 13){
                    _currentTween = transform.DOLocalMoveY(_initialYPosition - 11*_movementConstant[0] - 11*_movementConstant[1], 2f);
                    Notify(EventsEnum.TWELFTH_FLOOR);
                    _wentUp = true;
                    break;
                }else if (i == 2){
                    if((PlayerController.Instance.GetCurrentFloor() < currentFloor && _alreadyPassed.Contains(PlayerController.Instance.GetCurrentFloor() + 2)) || _wentUp)
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition, 2f);
                        Notify(EventsEnum.FIRST_FLOOR);
                        _wentUp = true;
                    }else{
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - 2*_movementConstant[0] - 2*_movementConstant[1], 2f);
                        Notify(EventsEnum.THIRD_FLOOR);
                        _wentUp = false;
                    }
                    break;
                }else{
                    if((PlayerController.Instance.GetCurrentFloor() < currentFloor && _alreadyPassed.Contains(PlayerController.Instance.GetCurrentFloor() + 2)) || _wentUp)
                    {
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - (currentFloor-2)*_movementConstant[0] - (currentFloor-2)*_movementConstant[1], 2f);
                        Notify(_floorEventsKeys[currentFloor-1]);
                        _wentUp = true;
                    }else{
                        _currentTween = transform.DOLocalMoveY(_initialYPosition - currentFloor*_movementConstant[0] - currentFloor*_movementConstant[1], 2f);
                        Notify(_floorEventsKeys[currentFloor+1]);
                        _wentUp = false;
                    }
                    break;
                }
            }
        }
    }*/
    public IEnumerator ChangeFloor(int currentFloor){
        yield return new WaitForSeconds(3f);

        _currentTween?.Kill();

        // Middle state (used as "buffer")
        float middleY = _scaleY - 1.24f;

        int playerFloor = PlayerController.Instance.GetCurrentFloor();

        // Going up logic
        bool goingUp = (playerFloor < currentFloor && _alreadyPassed.Contains(currentFloor + 1)) || _wentUp;

        if (currentFloor == 1)
        {
            _currentTween = transform.DOLocalMoveY(middleY, 2f);
            Notify(EventsEnum.SECOND_FLOOR);
            _wentUp = false;
        }
        else if (currentFloor == 20)
        {
            // Max floor: just go back down
            _currentTween = transform.DOLocalMoveY(_initialYPosition - (2.22f * 18), 2f); // floor 19's position
            Notify((EventsEnum)19); // Assuming you have enums like FIRST_FLOOR = 1, etc.
            _wentUp = false;
        }
        else
        {
            if (goingUp)
            {
                // Go up
                float newY = _initialYPosition - (2.22f * (currentFloor - 2)); // Go to floor above
                _currentTween = transform.DOLocalMoveY(newY, 2f);
                Notify((EventsEnum)(currentFloor - 1));
                _wentUp = true;
            }
            else
            {
                // Go down
                float newY = _initialYPosition - (2.22f * currentFloor); // Go to floor below
                _currentTween = transform.DOLocalMoveY(newY, 2f);
                Notify((EventsEnum)(currentFloor + 1));
                _wentUp = false;
            }
        }
    }

    #region Floor Events Keys Dictionary
    private void InsertIntoDictionary()
    {
        int floorKey = 0;

        foreach(EventsEnum events in Enum.GetValues(typeof(EventsEnum)))
        {
            floorKey++;
            _floorEventsKeys.Add(floorKey, events);
        }
    }
    #endregion
}
