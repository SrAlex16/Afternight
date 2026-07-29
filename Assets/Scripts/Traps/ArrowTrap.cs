using UnityEngine;

/// <summary>
/// Antes usaba un array fijo de GameObjects y recorría todo el array buscando uno inactivo
/// (FindArrow()) cada vez que disparaba. Ahora usa ObjectPool&lt;EnemyProjectile&gt;, igual
/// que el ataque a distancia del jugador: mismo patrón en todo el proyecto.
/// </summary>
public class ArrowTrap : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyProjectile arrowPrefab;
    [SerializeField] private int poolSize = 4;

    private ObjectPool<EnemyProjectile> pool;
    private float cooldownTimer;

    private void Awake()
    {
        if (arrowPrefab != null)
        {
            pool = new ObjectPool<EnemyProjectile>(arrowPrefab, poolSize, transform);
        }
        else
        {
            GameLogger.Error(GameLogger.Category.Trap, $"{name}: ArrowTrap sin arrowPrefab asignado.", this);
        }
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= attackCooldown)
        {
            Fire();
            cooldownTimer = 0f;
        }
    }

    private void Fire()
    {
        if (pool == null || firePoint == null) return;

        EnemyProjectile arrow = pool.Get();
        arrow.SetPool(pool);
        arrow.transform.position = firePoint.position;
        arrow.transform.rotation = firePoint.rotation;
    }
}
