using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Core.Singleton.Singleton<GameManager>, IObserver
{
    public GameObject pausedScreen;
    public GameObject networkMenu;
    private List<Enemy> _enemies = new();
    public List<AudioClip> audioClips = new();
    private List<EnemySpawner> _spawners = new();
    private List<AudioSource> _audioSources = new();
    private List<ElevatorManager> _elevators = new();
    public CinemachineVirtualCamera cinemachineCamera;
    public Dictionary<string, int> floorKeys = new Dictionary<string, int>();
    private bool _pausedGame = false;
    private bool _isPlayerSet;
    private bool _isHosting;
    private bool _isClient;
    private int _pastPlayerOneScore = 0;
    private int _pastPlayerTwoScore = 0;

    public void AddEnemy(Enemy enemy) { _enemies.Add(enemy); }
    public List<Enemy> GetEnemies() { return _enemies; }

    public void OnNotify(EventsEnum evt)
    {
        if (evt == EventsEnum.PlayerDestroyed)
        {
            GameOver();
        }
    }

    void Start()
    {
        if(!SceneManager.GetActiveScene().name.Equals("Level02")) StartNetwork();
        InsertIntoDictionary();
        _spawners.AddRange(GameObject.FindObjectsOfType<EnemySpawner>());
        _audioSources.AddRange(GameObject.FindObjectsOfType<AudioSource>());
        _elevators.AddRange(GameObject.FindObjectsOfType<ElevatorManager>());
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        ScoreManager.NetInstance.UpdateScoreP1ServerRpc(_pastPlayerOneScore);
        ScoreManager.NetInstance.UpdateScoreP2ServerRpc(_pastPlayerTwoScore);
    }

    private void StartNetwork()
    {
        if (PlayerPrefs.GetInt("isHosting") < 0) return;

        if (PlayerPrefs.GetInt("isHosting") == 1)
        {
            if(NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) NetworkManager.Singleton.StartHost();
            _isClient = false;
            _isHosting = true;
        }
        else if (PlayerPrefs.GetInt("isHosting") == 0)
        {
            if(NetworkManager.Singleton != null && !NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) NetworkManager.Singleton.StartClient();
            _isHosting = false;
            _isClient = true;
        }
    }

    void Update()
    {
        if (!_isPlayerSet && PlayerController.NetInstance != null)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            _isPlayerSet = true;
            if (players.Length > 1) GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>().CheckPlayerTwo();
            
            if(NetworkManager.Singleton.IsServer)
            {
                foreach(GameObject gO in players)
                {
                    if (gO.GetComponent<NetworkBehaviour>().IsServer) cinemachineCamera.Follow = gO.transform;
                }
            }
            else
            {
                foreach(GameObject gO in players)
                {
                    if (!gO.GetComponent<NetworkBehaviour>().IsServer) cinemachineCamera.Follow = gO.transform;
                }
            }
        }

        if (PlayerController.NetInstance != null)
        {
            if (_isPlayerSet && PlayerController.NetInstance.transform.localPosition.y <= -40.5f)
            {
                GameOver();
            }
        }

        if ((NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) && !_isHosting && _isClient) { SceneManager.LoadScene("MainMenu"); }

        PauseManagement();
    }

    public bool GetPlayerSet() { return _isPlayerSet; }

    public void GameOver()
    {
        StartCoroutine(DestroyAndShutdown());

        /*List<MonoBehaviour> allGameObjects = new();
        allGameObjects.AddRange(GameObject.FindObjectsOfType<MonoBehaviour>());
        allGameObjects.ForEach(gO => Destroy(gO));*/
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc(List<NetworkObjectReference> allNetworkObjects)
    {
        foreach(NetworkObjectReference nO in allNetworkObjects)
        {
            if(nO.TryGet(out NetworkObject netO)) if (netO.IsSpawned) netO.Despawn();
        }
    }

    private IEnumerator DestroyAndShutdown()
    {
        List<NetworkObjectReference> allNetworkObjects = new();

        foreach (NetworkObject nO in FindObjectsOfType<NetworkObject>())
        {
            if (nO.IsSpawned) allNetworkObjects.Add(new NetworkObjectReference(nO));
        }
        
        if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            foreach(NetworkObjectReference nO in allNetworkObjects)
            {
                if(nO.TryGet(out NetworkObject netO)) if (netO.IsSpawned) netO.Despawn();
            }
        }else if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            RequestDespawnServerRpc(allNetworkObjects);
        }

        yield return null;

        List<Teleporters> allGameObjects = new();
        allGameObjects.AddRange(GameObject.FindObjectsOfType<Teleporters>());
        allGameObjects.ForEach(tp => Destroy(tp));

        Destroy(FindObjectOfType<ElevatorManager>());

        yield return new WaitForEndOfFrame();
        DOTween.KillAll(false);
        yield return new WaitForEndOfFrame();
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();

        yield return null;

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void GoToNextLevel()
    {
        _pastPlayerOneScore = ScoreManager.NetInstance.GetPlayerOneScore();
        _pastPlayerTwoScore = ScoreManager.NetInstance.GetPlayerTwoScore();

        List<Enemy> allEnemies = new();
        allEnemies.AddRange(FindObjectsOfType<Enemy>());
        
        if (NetworkManager.Singleton.IsServer)
        {
            allEnemies.ForEach(e => Destroy(e.gameObject));
        }
        else {
            List<NetworkObjectReference> allEnemiesRef = new();

            foreach (Enemy e in allEnemies)
            {
                allEnemiesRef.Add(new NetworkObjectReference(e.GetComponent<NetworkObject>()));
            }
            
            RequestDestroyEnemiesServerRpc(allEnemiesRef);
        }

        DOTween.KillAll(false);
        RequestLoadSceneServerRpc();
        //NetworkManager.Singleton.SceneManager.LoadScene("Level02", LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDestroyEnemiesServerRpc(List<NetworkObjectReference> enemiesRef)
    {
        enemiesRef.ForEach(e => {
            if (e.TryGet(out NetworkObject enemy))
            {
                if (enemy != null && enemy.IsSpawned) { enemy.Despawn(true); }
            }
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestLoadSceneServerRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Level02", LoadSceneMode.Single);
    }

    private void PauseManagement()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!_pausedGame)
            {
                _pausedGame = true;

                if (!NetworkManager.Singleton.IsListening)
                {
                    foreach (AudioSource audioSource in _audioSources) { audioSource.Pause(); }
                    foreach (AudioSource audioSource in _audioSources) { if (audioSource.name == "SFX") { audioSource.clip = audioClips[0]; audioSource.Play(); } }
                    _enemies.ForEach(enemy => enemy.gameObject.SetActive(false));
                    _spawners.ForEach(spawner => spawner.gameObject.SetActive(false));
                    _elevators.ForEach(elevator => elevator.gameObject.SetActive(false));
                    pausedScreen.SetActive(true);
                } else { networkMenu.SetActive(true); }
            }else {
                _pausedGame = false;
                
                if (!NetworkManager.Singleton.IsListening)
                {
                    foreach (AudioSource audioSource in _audioSources) { if (audioSource.name == "BGSong") audioSource.Play(); }
                    _enemies.ForEach(enemy => enemy.gameObject.SetActive(true));
                    _spawners.ForEach(spawner => spawner.gameObject.SetActive(true));
                    _elevators.ForEach(elevator => elevator.gameObject.SetActive(true));
                    pausedScreen.SetActive(false);
                } else { networkMenu.SetActive(false); }
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
