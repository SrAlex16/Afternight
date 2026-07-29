using UnityEngine;

/// <summary>
/// Pickup de vida. Es intencionadamente específico del jugador (usa PlayerStats.Heal en vez de
/// una interfaz genérica): curar no tiene sentido para trampas/enemigos, así que forzar una
/// interfaz "IHealable" aquí sería complejidad de más sin beneficio real.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField] private float healAmount = 1f;
    [SerializeField] private string targetTag = "Player";

    [Header("Sound params")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupClip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        var playerStats = collision.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            GameLogger.Warning(GameLogger.Category.Combat, $"{name}: '{collision.name}' no tiene PlayerStats.", this);
            return;
        }

        playerStats.Heal(healAmount);
        if (audioSource != null && pickupClip != null) audioSource.PlayOneShot(pickupClip);
        Destroy(gameObject);
    }
}
