using UnityEngine;

/// <summary>
/// Proyectil del jugador (antes Projectile.cs y el Fireball.cs duplicado/muerto se fusionan
/// en esta única clase, ahora compatible con ObjectPool&lt;Projectile&gt;).
///
/// BUG ARREGLADO: antes hacía collision.GetComponent&lt;PlayerStats&gt;() sobre el enemigo
/// golpeado, y PlayerStats es del jugador — los enemigos nunca tenían ese componente, así que
/// el daño a distancia contra enemigos no funcionaba (o lanzaba NullReferenceException).
/// Ahora pide GetComponent&lt;IDamageable&gt;(), que tanto PlayerStats como EnemyHealth
/// implementan, así que funciona contra cualquiera de los dos sin acoplarse a ninguno.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private string targetTag = "Enemy";

    private Animator animator;
    private BoxCollider2D boxCollider;
    private float direction;
    private bool hasHit;
    private float lifetimeTimer;

    private ObjectPool<Projectile> pool;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    /// <summary>El dueño (PlayerCombat) le pasa el pool al que debe devolverse cuando termine.</summary>
    public void SetPool(ObjectPool<Projectile> owningPool)
    {
        pool = owningPool;
    }

    public void Launch(float launchDirection)
    {
        direction = launchDirection;

        float scaleX = Mathf.Abs(transform.localScale.x) * Mathf.Sign(launchDirection == 0 ? 1 : launchDirection);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    public void OnSpawned()
    {
        hasHit = false;
        lifetimeTimer = 0f;
        if (boxCollider != null) boxCollider.enabled = true;
    }

    public void OnDespawned()
    {
        // Nada extra que limpiar por ahora; existe el hook por si se añaden efectos de estela, etc.
    }

    private void Update()
    {
        if (hasHit) return;

        transform.Translate(speed * Time.deltaTime * direction, 0f, 0f);

        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer > maxLifetime)
        {
            GameLogger.Verbose(GameLogger.Category.Combat, "Proyectil desactivado por tiempo de vida máximo sin impactar.", this);
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        hasHit = true;
        if (boxCollider != null) boxCollider.enabled = false;
        if (animator != null) animator.SetTrigger("explode");

        if (collision.CompareTag(targetTag))
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                GameLogger.Info(GameLogger.Category.Combat, $"Proyectil impacta en {collision.name} por {damage} de daño.", this);
            }
            else
            {
                GameLogger.Warning(GameLogger.Category.Combat, $"Proyectil chocó contra '{collision.name}' (tag {targetTag}) pero no implementa IDamageable.", this);
            }
        }

        // Da tiempo a que se vea la animación de explosión antes de devolverlo al pool.
        Invoke(nameof(ReturnToPool), 0.3f);
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            // Fallback por si se usa el proyectil suelto, sin pool, en algún test.
            gameObject.SetActive(false);
        }
    }
}
