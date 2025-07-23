using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Door _spawnDoor;
    [SerializeField] private float _moveSpeed = 0.5f;
    [SerializeField] private int _floorNumber = 1;
    
    [Header("Shooting Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _shootInterval = 2f;
    [SerializeField] private float _shootRange = 5f;
    
    private Rigidbody2D _rigidbody;
    private Transform _playerTransform;
    private float _nextShootTime;
    private int gravity = 1;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        if (PlayerController.Instance != null)
        {
            Physics2D.IgnoreCollision(PlayerController.Instance.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }

        if (_firePoint == null)
        {
            _firePoint = new GameObject("FirePoint").transform;
            _firePoint.SetParent(transform);
            _firePoint.localPosition = new Vector3(0.5f, 0.1f, 0);
        }if (_projectilePrefab == null)
{
    GameObject projectileInScene = GameObject.FindWithTag("Projectile");
    if (projectileInScene != null)
    {
        _projectilePrefab = projectileInScene;
    }
}
}
        
        public void SetProjectilePrefab(GameObject prefab)
    {
        _projectilePrefab = prefab;
    }
    void Start()
    {
        _playerTransform = PlayerController.Instance?.transform;
    }

    void Update()
    {
        if (_playerTransform != null)
        {
            Move();
            TryShoot();
        }
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(
            transform.position, 
            _playerTransform.position, 
            _moveSpeed * Time.deltaTime
        );
    }

    private void TryShoot()
    {
        if (Time.time >= _nextShootTime && 
            Vector2.Distance(transform.position, _playerTransform.position) < _shootRange)
        {
            Shoot();
            _nextShootTime = Time.time + _shootInterval;
        }
    }

    private void Shoot()
    {
        if (_projectilePrefab == null)
        {
            GameObject foundProjectile = GameObject.FindWithTag("Projectile");
            if (foundProjectile != null)
            {
                _projectilePrefab = foundProjectile;
            }
            else
            {
                _projectilePrefab = Resources.Load<GameObject>("Prefabs/spritesheet_10");
            }
        }

        if (_firePoint == null)
        {
            Debug.LogWarning("Cannot shoot - fire point missing");
            return;
        }

        GameObject projectile = Instantiate(
            _projectilePrefab,
            _firePoint.position,
            Quaternion.identity
        );

        projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        Vector2 direction = (_playerTransform.position - _firePoint.position).normalized;
        projectile.GetComponent<EnemyProjectile>()?.Initialize(direction);
    }
public void ConfigureProjectile(float speed, float lifetime)
{
    if (_projectilePrefab == null) return;

    EnemyProjectile ep = _projectilePrefab.GetComponent<EnemyProjectile>();
    if (ep != null)
    {
        ep.SetDefaults(speed, lifetime);
    }
}



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Enemy"))
        {
            gravity *= 0;
        }
    }
}