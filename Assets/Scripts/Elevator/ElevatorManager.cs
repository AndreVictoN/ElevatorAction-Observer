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

    void Start()
    {
        Subscribe(PlayerController.Instance);
        Subscribe(FindObjectsOfType<InvisibleWalls>());
        _initialYPosition = transform.position.y;
        _scaleY = transform.localScale.y;
    }

    void Update()
    {
        HandleMovement();
    }

    /*
    private void HandleMovement()
    {
        if(this.gameObject.transform.position.y == _initialYPosition)
        {
            _currentFloor = 1;
            if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);

            //Notify(EventsEnum.FIRST_FLOOR);
            StartCoroutine(ChangeFloor(_currentFloor));
        }else if(this.gameObject.transform.position.y == _initialYPosition - 2.22f)
        {
            _currentFloor = 2;
            if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);

            //Notify(EventsEnum.SECOND_FLOOR);
            StartCoroutine(ChangeFloor(_currentFloor));
        }else if(this.gameObject.transform.position.y == -_initialYPosition)
        {
            _currentFloor = 3;
            if(!_alreadyPassed.Contains(_currentFloor)) _alreadyPassed.Add(_currentFloor);

            StartCoroutine(ChangeFloor(_currentFloor));
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
        yield return new WaitForSeconds(3f);

        _currentTween?.Kill();

        if(currentFloor == 1)
        {
            _currentTween = transform.DOLocalMoveY(_scaleY - 1.24f, 2f);
            Notify(EventsEnum.SECOND_FLOOR);
            _wentUp = false;
        }else if(currentFloor == 2)
        {
            if((PlayerController.Instance.GetCurrentFloor() < currentFloor && _alreadyPassed.Contains(currentFloor + 1)) || _wentUp)
            {
                _currentTween = transform.DOLocalMoveY(_initialYPosition, 2f);
                Notify(EventsEnum.FIRST_FLOOR);
                _wentUp = true;
            }else{
                _currentTween = transform.DOLocalMoveY(-_initialYPosition, 2f);
                Notify(EventsEnum.THIRD_FLOOR);
                _wentUp = false;
            }
        }else if(currentFloor == 3)
        {
            _currentTween = transform.DOLocalMoveY(_scaleY - 1.24f, 2f);
            Notify(EventsEnum.SECOND_FLOOR);
            _wentUp = true;
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
}
