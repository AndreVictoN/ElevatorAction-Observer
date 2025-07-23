using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour, IObserver
{
    private Dictionary<string, int> _floorKeys = new Dictionary<string, int>();
    [SerializeField] private List<GameObject> _enemiesSpawned = new();
    [SerializeField] private GameObject enemyPrefab;
    private GameObject _enemyInstance;

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
        if(evt == EventsEnum.PLAYER_IN_ELEVATOR || evt == EventsEnum.PLAYER_NOT_IN_ELEVATOR) return;

        if (_floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] || _floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] - 1 || _floorKeys[evt.ToString()] == _floorKeys[this.gameObject.tag] + 1)
        {
            if(_enemiesSpawned.Count < 2)StartCoroutine(SpawnEnemy());
        }
    }
    IEnumerator SpawnEnemy()
{
    yield return new WaitForSeconds(6f);
    if (_enemiesSpawned.Count < 2) 
    {
        _enemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        
        Enemy enemyScript = _enemyInstance.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            GameObject projectile = GameObject.FindWithTag("Projectile");
                if (projectile != null && projectile.GetComponent<EnemyProjectile>() != null)
                {
                    enemyScript.SetProjectilePrefab(projectile);
                    enemyScript.ConfigureProjectile(speed: 7f, lifetime: 2f);
            }
        }

        _enemiesSpawned.Add(_enemyInstance);
    }
}

    #region Floor Events Keys Dictionary
    private void InsertIntoDictionary()
    {
        int floorKey = 0;

        foreach (EventsEnum events in Enum.GetValues(typeof(EventsEnum)))
        {
            if (events == EventsEnum.PLAYER_IN_ELEVATOR || events == EventsEnum.PLAYER_NOT_IN_ELEVATOR) continue;

            floorKey++;
            _floorKeys.Add(events.ToString(), floorKey);
        }
    }
    #endregion
}
