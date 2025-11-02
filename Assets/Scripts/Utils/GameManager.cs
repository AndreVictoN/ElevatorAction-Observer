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
        StartNetwork();
        InsertIntoDictionary();
        _spawners.AddRange(GameObject.FindObjectsOfType<EnemySpawner>());
        _audioSources.AddRange(GameObject.FindObjectsOfType<AudioSource>());
        _elevators.AddRange(GameObject.FindObjectsOfType<ElevatorManager>());
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            NetworkObjectReference playerRef = new NetworkObjectReference(player.NetworkObject);
            player.transform.position = new Vector2(-2.72f, 1.875f);
            //SetSpriteClientRpc(playerRef);
        }

        /*foreach (ulong clientId in clientsCompleted)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObject = client.PlayerObject;
                SetSpriteClientRpc(playerObject);
            }
        }*/
    }

    [ClientRpc]
    private void SetSpriteClientRpc(NetworkObjectReference playerRef)
    {
        if (playerRef.TryGet(out NetworkObject player))
        {
            if(player.TryGetComponent(out PlayerController playerCont))
            {
                var sprite = playerCont.GetComponent<SpriteRenderer>();
                sprite.enabled = true;
                sprite.sortingOrder = 2;
            }
        }
        
        //player.GetComponent<SpriteRenderer>().enabled = true;
        //player.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    private void StartNetwork()
    {
        if (PlayerPrefs.GetInt("isHosting") < 0) return;

        if (PlayerPrefs.GetInt("isHosting") == 1)
        {
            NetworkManager.Singleton.StartHost();
            _isClient = false;
            _isHosting = true;
        }
        else if (PlayerPrefs.GetInt("isHosting") == 0)
        {
            NetworkManager.Singleton.StartClient();
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
            if (players.Length > 1) { cinemachineCamera.Follow = players[1]?.transform; GameObject.FindGameObjectWithTag("ScoreManager").GetComponent<ScoreManager>().CheckPlayerTwo(); }
            else { cinemachineCamera.Follow = PlayerController.NetInstance.transform; }
        }

        if (PlayerController.NetInstance != null)
        {
            if (_isPlayerSet && PlayerController.NetInstance.transform.localPosition.y <= -40.5f)
            {
                GameOver();
            }
        }
        
        if(!NetworkManager.Singleton.IsListening && !_isHosting && _isClient) { SceneManager.LoadScene("MainMenu"); }

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
    private void RequestDespawnServerRpc(List<NetworkObject> allNetworkObjects)
    {
        foreach(NetworkObject nO in allNetworkObjects)
        {
            if (nO.IsSpawned) nO.Despawn();
        }
    }

    private IEnumerator DestroyAndShutdown()
    {
        List<NetworkObject> allNetworkObjects = new();
        allNetworkObjects.AddRange(GameObject.FindObjectsOfType<NetworkObject>());
        RequestDespawnServerRpc(allNetworkObjects);

        yield return null;

        List<Teleporters> allGameObjects = new();
        allGameObjects.AddRange(GameObject.FindObjectsOfType<Teleporters>());
        allGameObjects.ForEach(tp => Destroy(tp));

        Destroy(FindObjectOfType<ElevatorManager>());

        yield return new WaitForEndOfFrame();
        DOTween.KillAll(false);
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();

        yield return null;

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void GoToNextLevel()
    {
        List<Enemy> allEnemies = new();
        allEnemies.AddRange(GameObject.FindObjectsOfType<Enemy>());


        allEnemies.ForEach(e => Destroy(e.gameObject));

        DOTween.KillAll(false);
        RequestLoadSceneServerRpc();
        NetworkManager.Singleton.SceneManager.LoadScene("Level02", LoadSceneMode.Single);
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
