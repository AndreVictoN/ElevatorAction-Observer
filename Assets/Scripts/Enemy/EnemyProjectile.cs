using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] public float _speed = 7f;
    [SerializeField] public float _lifetime = 2f;
    [SerializeField] public GameObject _hitEffect;

    public Vector2 _direction;
    private float _timer;

    public void Initialize(Vector2 direction)
    {
        _direction = direction.normalized;
        _timer = _lifetime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
{

        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Projectile")) return;

        SpawnHitEffect();
        Destroy(gameObject);
    }

    private void SpawnHitEffect()
    {
        if (_hitEffect != null)
        {
            Instantiate(_hitEffect, transform.position, Quaternion.identity);
        }
    }
    public void SetDefaults(float speed, float lifetime)
    {
        _speed = speed;
        _lifetime = lifetime;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Projectile")) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Obstacle"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

}