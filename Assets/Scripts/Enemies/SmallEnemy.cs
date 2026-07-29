using UnityEngine;

/// <summary>
/// Cambios respecto al original:
/// - Ya NO gestiona su propia vida (int health) ni compara tags "fireball"/"Sword" a mano:
///   eso ahora es responsabilidad de EnemyHealth + IDamageable (componente separado, añádelo
///   al mismo GameObject). El daño le llega automáticamente vía Projectile/espada sin que este
///   script tenga que saber nada de quién le golpeó.
/// - Se quita el print("sword") de depuración (usa GameLogger.Verbose si hace falta trazar esto).
/// - Comparaciones de tag con CompareTag en vez de "==" (evita alocación de string).
/// - BUG NO ARREGLADO A PROPÓSITO, aclarado: el original tenía un sistema a medias de "más daño
///   pasado un minuto" comparando Time.realtimeSinceStartup con un "currentTime" que nunca se
///   inicializaba desde este script (dependía de un DayNightSystem2D que no estaba en el repo
///   que revisamos). Lo he quitado de aquí porque tal y como estaba no hacía lo que parecía
///   pretender. Si ese sistema día/noche sigue vivo, dime cómo funciona y lo enchufo bien,
///   como un multiplicador de daño limpio en vez de comparar timestamps sueltos.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class SmallEnemy : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float colliderDistance = 1f;
    [SerializeField] private int range = 1;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask playerLayer;

    [Header("Ataque")]
    [SerializeField] private int damage = 1;

    private float cooldownTimer = Mathf.Infinity;
    private IDamageable playerDamageable;

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight() && cooldownTimer >= attackCooldown)
        {
            DamagePlayer();
            cooldownTimer = 0f;
        }
    }

    private bool PlayerInSight()
    {
        if (boxCollider == null)
        {
            GameLogger.Warning(GameLogger.Category.Enemy, $"{name}: SmallEnemy sin boxCollider asignado.", this);
            return false;
        }

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
        {
            playerDamageable = hit.collider.GetComponent<IDamageable>();
        }

        return hit.collider != null;
    }

    private void DamagePlayer()
    {
        if (playerDamageable == null)
        {
            GameLogger.Warning(GameLogger.Category.Enemy, $"{name}: jugador detectado pero sin componente IDamageable.", this);
            return;
        }

        playerDamageable.TakeDamage(damage);
        GameLogger.Verbose(GameLogger.Category.Enemy, $"{name} ataca al jugador por {damage}.", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }
}
