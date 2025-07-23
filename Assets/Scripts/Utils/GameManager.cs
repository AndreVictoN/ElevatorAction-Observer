using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Core.Singleton.Singleton<GameManager>
{
    private bool _pausedGame = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!_resetGame) _pausedGame = true;
            else _pausedGame = false;
        }
    }

    public bool IsGamePaused(){return _pausedGame;}
}
