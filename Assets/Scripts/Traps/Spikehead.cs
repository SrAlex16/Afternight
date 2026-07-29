using UnityEngine;

/// <summary>
/// BUG ARREGLADO: el original calculaba "directions[i]" como transform.position ± right*range
/// (es decir, POSICIONES del mundo) y luego las pasaba directamente como parámetro dirección a
/// Physics2D.Raycast(origin, direction, ...). Eso solo funcionaba "por casualidad" cerca del
/// origen del mundo (0,0,0); en cualquier otra posición el raycast apuntaba a un sitio
/// completamente distinto al que parecía. Ahora se separan claramente los vectores DIRECCIÓN
/// (para el raycast) de los PUNTOS DESTINO (para el Lerp del movimiento).
/// </summary>
public class Spikehead : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string targetTag = "Player";

    private Vector3 destination;
    private float attackTimer = Mathf.Infinity;

    private static readonly Vector3[] LocalDirections =
    {
        Vector3.left, Vector3.right, Vector3.up, Vector3.down
    };

    private void Start()
    {
        Stop();
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f) CheckForPlayer();
        transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * speed);
    }

    private void CheckForPlayer()
    {
        foreach (Vector3 localDir in LocalDirections)
        {
            Vector3 worldDirection = transform.TransformDirection(localDir);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDirection, range, playerLayer);

            if (hit.collider != null)
            {
                destination = transform.position + worldDirection * range;
                attackTimer = 0f;
                GameLogger.Verbose(GameLogger.Category.Trap, $"{name} detecta al jugador, se lanza hacia {worldDirection}.", this);
                return; // en cuanto detecta una dirección válida, no hace falta seguir mirando
            }
        }
    }

    private void Stop()
    {
        destination = transform.position;
        attackTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
            else
                GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);

            Stop();
        }
        else
        {
            Stop(); // se detiene también al chocar contra pared/suelo, en vez de desactivarse para siempre
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (Vector3 localDir in LocalDirections)
        {
            Vector3 worldDirection = transform.TransformDirection(localDir);
            Gizmos.DrawLine(transform.position, transform.position + worldDirection * range);
        }
    }
}
