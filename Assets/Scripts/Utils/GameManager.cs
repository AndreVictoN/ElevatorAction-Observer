using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Core.Singleton.Singleton<GameManager>
{
    public TextMeshProUGUI score;
    public GameObject pausedScreen;
    private List<Enemy> _enemies = new();
    public List<AudioClip> audioClips = new();
    private List<EnemySpawner> _spawners = new();
    private List<AudioSource> _audioSources = new();
    private List<ElevatorManager> _elevators = new();
    public CinemachineVirtualCamera cinemachineCamera;
    public Dictionary<string, int> floorKeys = new Dictionary<string, int>();
    private bool _pausedGame = false;
    private int _score = 0;
    private bool _isPlayerSet;

    public void AddEnemy(Enemy enemy) { _enemies.Add(enemy); }
    public List<Enemy> GetEnemies(){return _enemies;}

    void Start()
    {
        InsertIntoDictionary();
        _spawners.AddRange(GameObject.FindObjectsOfType<EnemySpawner>());
        _audioSources.AddRange(GameObject.FindObjectsOfType<AudioSource>());
        _elevators.AddRange(GameObject.FindObjectsOfType<ElevatorManager>());
        score.text = "Score: 0";
    }

    public void UpdateScore(int sum)
    {
        _score += sum;
        score.text = "Score: " + _score;
    }

    void Update()
    {
        if (!_isPlayerSet && PlayerController.NetInstance != null)
        {
            _isPlayerSet = true;
            cinemachineCamera.Follow = PlayerController.NetInstance.transform;
        }

        if (PlayerController.NetInstance.IsDestroyed() || (_isPlayerSet && PlayerController.NetInstance.transform.localPosition.y <= -40.5f))
        {
            GameOver();
        }

        PauseManagement();
    }

    public bool GetPlayerSet() { return _isPlayerSet; }

    public void GameOver()
    {
        List<MonoBehaviour> allGameObjects = new();
        allGameObjects.AddRange(GameObject.FindObjectsOfType<MonoBehaviour>());
        allGameObjects.ForEach(gO => Destroy(gO));

        SceneManager.LoadScene("MainMenu");
    }

    public void GoToNextLevel()
    {
        List<MonoBehaviour> allGameObjects = new();
        allGameObjects.AddRange(GameObject.FindObjectsOfType<MonoBehaviour>());
        allGameObjects.ForEach(gO => Destroy(gO));

        SceneManager.LoadScene("Level02");
    }

    private void PauseManagement()
    {
        if (Input.GetKeyDown(KeyCode.Return) )
        {
            if (!_pausedGame)
            {
                _pausedGame = true;

                foreach(AudioSource audioSource in _audioSources){audioSource.Pause();}
                foreach(AudioSource audioSource in _audioSources){if(audioSource.name == "SFX") {audioSource.clip = audioClips[0]; audioSource.Play();}}
                _enemies.ForEach(enemy => enemy.gameObject.SetActive(false));
                _spawners.ForEach(spawner => spawner.gameObject.SetActive(false));
                _elevators.ForEach(elevator => elevator.gameObject.SetActive(false));

                pausedScreen.SetActive(true);
            }else {
                _pausedGame = false;

                foreach(AudioSource audioSource in _audioSources){if(audioSource.name == "BGSong") audioSource.Play();}
                _enemies.ForEach(enemy => enemy.gameObject.SetActive(true));
                _spawners.ForEach(spawner => spawner.gameObject.SetActive(true));
                _elevators.ForEach(elevator => elevator.gameObject.SetActive(true));

                pausedScreen.SetActive(false);
            }
        }
    }

    public void PlayAudio(int clipNumber)
    {
        foreach(AudioSource audioSource in _audioSources){if(audioSource.name == "SFX") {audioSource.clip = audioClips[clipNumber]; audioSource.Play();}}
    }

    public bool IsGamePaused(){return _pausedGame;}

    private void InsertIntoDictionary()
    {
        int floorKey = 0;

        foreach(EventsEnum events in Enum.GetValues(typeof(EventsEnum)))
        {
            if(events == EventsEnum.PLAYER_IN_ELEVATOR || events == EventsEnum.PLAYER_NOT_IN_ELEVATOR) continue;

            floorKey++;
            floorKeys.Add(events.ToString(), floorKey);
        }
    }
}
