using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Antes: leía playerHealth.currentHealth / 10 en Update() cada frame, con un "10" hardcodeado
/// que no tenía relación real con la vida máxima del jugador.
/// Ahora: se suscribe a PlayerStats.OnHealthChanged y solo actualiza la barra cuando la vida
/// cambia de verdad, usando siempre la vida máxima real.
/// </summary>
public class HealthBarController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerHealth;
    [SerializeField] private Image currentHealthBar;

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            GameLogger.Error(GameLogger.Category.UI, "HealthBarController sin PlayerStats asignado.", this);
            return;
        }

        playerHealth.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (currentHealthBar == null) return;
        currentHealthBar.fillAmount = max > 0 ? current / max : 0f;
    }
}
