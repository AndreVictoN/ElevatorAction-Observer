using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private float _detectionRange = 5f;

    private float _nextFireTime;
    private Transform _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= _detectionRange && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / _fireRate;
        }
    }

    void Shoot()
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        Vector2 direction = (_player.position - _firePoint.position).normalized;
        GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity);
        
        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.SetDirection(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
}