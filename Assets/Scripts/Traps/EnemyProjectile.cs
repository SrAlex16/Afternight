using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float resetTime = 3f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private string targetTag = "Player";

    private float lifeTime;
    private ObjectPool<EnemyProjectile> pool;

    public void SetPool(ObjectPool<EnemyProjectile> owningPool) => pool = owningPool;

    public void OnSpawned() => lifeTime = 0f;
    public void OnDespawned() { }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime, 0f, 0f);

        lifeTime += Time.deltaTime;
        if (lifeTime > resetTime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pool != null)
            pool.Release(this);
        else
            gameObject.SetActive(false);
    }
}
