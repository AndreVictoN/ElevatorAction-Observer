using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour, IObserver
{
    private Dictionary<string, int> _floorKeys = new Dictionary<string, int>();
    [SerializeField] private List<GameObject> _enemiesSpawned = new();
    [SerializeField] private GameObject enemyPrefab;
    private Coroutine _currentCoroutine = null;
    private GameObject _enemyInstance;
    //public bool canSpawnEnemy = false;

    void Awake()
    {
        InsertIntoDictionary();

        Transform greatGrandparent = transform.parent?.parent?.parent;

        if (greatGrandparent != null)
        {
            Transform invisibleWallsTransform = greatGrandparent.transform.Find("InvisibleWalls");

            if (invisibleWallsTransform != null)
            {
                string tag = invisibleWallsTransform.gameObject.tag;
                this.gameObject.tag = tag;
            }
        }
    }

    public void OnNotify(EventsEnum evt)
    {
        if (this.gameObject.activeSelf == false) return;
        if (evt == EventsEnum.PLAYER_IN_ELEVATOR || evt == EventsEnum.PLAYER_NOT_IN_ELEVATOR) return;
        if (GameManager.Instance.IsGamePaused()) return;
        //if(!canSpawnEnemy) return;

        if (_floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] || _floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] - 1 || _floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] + 1)
        {
            if (_enemiesSpawned.Count < 2) StartCoroutine(SpawnEnemy());
        }
    }

    void Update()
    {
        if (!GameManager.Instance.GetPlayerSet()) return;
        
        if (SceneManager.GetActiveScene().name == "Level02" && _currentCoroutine == null)
        {
            _currentCoroutine = StartCoroutine(SpawnEnemy());
        }
        else if (SceneManager.GetActiveScene().name == "Level01" && PlayerController.NetInstance.GetCurrentFloor() > PlayerController.NetInstance.GetElevatorFloor() + 5 && _floorKeys[this.gameObject.tag] == PlayerController.NetInstance.GetCurrentFloor())
        {
            _currentCoroutine = StartCoroutine(SpawnEnemy());
        }
    }

    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(6f);
        if(_enemiesSpawned.Count < 2)
        {
            _enemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            _enemyInstance.transform.SetParent(null);
            
            Enemy enemyScript = _enemyInstance.GetComponent<Enemy>();
            if(enemyScript != null)
            {
                GameManager.Instance.AddEnemy(enemyScript);
            }

            _enemiesSpawned.Add(_enemyInstance);
        }

        _currentCoroutine = null;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (_enemiesSpawned.Contains(enemy))
        {
            _enemiesSpawned.Remove(enemy);
        }
    }

    #region Floor Events Keys Dictionary
    private void InsertIntoDictionary()
    {
        int floorKey = 0;

        foreach(EventsEnum events in Enum.GetValues(typeof(EventsEnum)))
        {
            if(events == EventsEnum.PLAYER_IN_ELEVATOR || events == EventsEnum.PLAYER_NOT_IN_ELEVATOR) continue;

            floorKey++;
            _floorKeys.Add(events.ToString(), floorKey);
        }
    }
    #endregion
}
