using UnityEngine;

/// <summary>
/// Antes: TakeDamage(9999) hardcodeado para simular muerte instantánea.
/// Ahora: un flag explícito "instantKill" (más legible que un número mágico) que, si está
/// activo, ignora el valor de damage y aplica un daño arbitrariamente alto real, o puedes
/// desactivarlo y usar damage como una trampa "normal" configurable desde el inspector.
/// </summary>
public class Spikes : MonoBehaviour
{
    [SerializeField] private bool instantKill = true;
    [SerializeField] private float damage = 1f;
    [SerializeField] private string targetTag = "Player";

    private const float InstantKillDamage = 9999f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        var damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
        {
            GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
            return;
        }

        damageable.TakeDamage(instantKill ? InstantKillDamage : damage);
    }
}
