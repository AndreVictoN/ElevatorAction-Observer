using Unity.Netcode;
using UnityEngine;

public class Shooter : NetworkBehaviour
{
    public GameObject projectile;
    //private GameObject _instance = null;
    [SerializeField] private float _timeToShoot = 1;
    private bool _firstShoot = true;

    void Awake()
    {
        _firstShoot = true;
    }

    void Update()
    {
        if (this.gameObject.transform.parent.tag == "Player")
        {
            if (!GameManager.Instance.IsGamePaused() && Input.GetKeyDown(KeyCode.Z) && !transform.parent.gameObject.GetComponent<PlayerController>().IsInDoor() && _timeToShoot == 0)
            {
                if (!IsOwner) return;
                CheckSpawnInServer();

                GameManager.Instance.PlayAudio(3);
                _timeToShoot = 0.5f;
            }

            if (transform.parent.gameObject.GetComponent<PlayerController>().IsCrouching())
            {
                this.transform.localPosition = new Vector2(this.transform.localPosition.x, -0.33f);
            }
            else
            {
                this.transform.localPosition = new Vector2(this.transform.localPosition.x, 0.09f);
            }
        }

        if (transform.parent != null && transform.parent.GetComponent<Enemy>() != null)
        {
            if (!GameManager.Instance.IsGamePaused() && _timeToShoot == 0 && transform.parent.GetComponent<Enemy>().IsSeeingPlayer())
            {
                if (!_firstShoot)
                {
                    CheckSpawnInServer();
                    GameManager.Instance.PlayAudio(4);
                }
                else
                {
                    _firstShoot = false;
                }
                _timeToShoot = 1f;
            }
        }

        //if (_instance != null && _instance.gameObject.GetComponent<NetworkObject>().IsSpawned) _instance.transform.SetParent(this.gameObject.transform);

        /*if (_instance != null && _instance.GetComponent<NetworkObject>().IsSpawned && _instance.CompareTag("EnemyProjectile")) { _instance.transform.SetParent(this.gameObject.transform.parent); }
        _instance = null;*/

        if (_timeToShoot > 0) _timeToShoot -= Time.deltaTime;
        else if (_timeToShoot < 0) _timeToShoot = 0;
    }

    private void CheckSpawnInServer()
    {
        if (IsHost) { SpawnProjectile(transform.position); }
        else if (IsClient && !IsHost) { RequestSpawnThisServerRpc(transform.position); }
        else { Instantiate(projectile, transform.position, Quaternion.identity); }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnThisServerRpc(Vector3 position)
    {
        SpawnProjectile(position);
    }

    private void SpawnProjectile(Vector3 position)
    {
        if (!IsServer) return;

        GameObject projectileInstance;
        projectileInstance = Instantiate(projectile, position, Quaternion.identity);
        if(NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) projectileInstance.GetComponent<NetworkObject>().Spawn(true);

        if (projectileInstance.CompareTag("PlayerProjectile")) projectileInstance.GetComponent<PlayerProjectile>().SetMySpawner(this.transform.parent.gameObject);
        
        if (projectileInstance.CompareTag("EnemyProjectile"))
        {
            projectileInstance.GetComponent<ProjectileBase>().SetMyParent(this.transform.parent.gameObject);
            projectileInstance.GetComponent<NetworkObject>().TrySetParent(this.transform.parent.gameObject, true);
            //projectileInstance.transform.SetParent(transform.parent);
        }
    }
}
