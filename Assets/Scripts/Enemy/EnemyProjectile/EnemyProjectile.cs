using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private GameObject _hitEffect;

    private Vector2 _direction;
    private float _timer;

    void Start()
    {
        _timer = _lifetime;
    }

    void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction.normalized;
        
        // Rotaciona o projétil na direção do movimento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (_hitEffect != null)
        {
            Instantiate(_hitEffect, transform.position, Quaternion.identity);
        }
    }
}