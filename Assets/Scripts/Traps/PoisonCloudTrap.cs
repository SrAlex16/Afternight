using UnityEngine;

/// <summary>
/// Trampa nueva: zona de gas tóxico que hace daño periódico mientras el jugador permanece
/// dentro (no al entrar una vez, como las demás trampas). Pensada para pasillos donde conviene
/// cruzar rápido — combina bien con el dash: cruzarla a pie duele mucho más que dasheando.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PoisonCloudTrap : MonoBehaviour
{
    [SerializeField] private float damagePerTick = 1f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private string targetTag = "Player";

    private float tickTimer;
    private IDamageable playerInside;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        playerInside = collision.GetComponent<IDamageable>();
        if (playerInside == null)
        {
            GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
            return;
        }

        tickTimer = tickInterval; // daña inmediatamente al entrar, no espera un ciclo completo
        GameLogger.Verbose(GameLogger.Category.Trap, "Jugador entra en la nube tóxica.", this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;
        playerInside = null;
    }

    private void Update()
    {
        if (playerInside == null) return;

        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            playerInside.TakeDamage(damagePerTick);
        }
    }
}
