using System;
using UnityEngine;

/// <summary>
/// Vida de un enemigo. Antes SmallEnemy.cs llevaba un "int health" propio decrementado
/// directamente en OnTriggerEnter2D comparando tags a mano ("fireball", "Sword"). Extraerlo
/// aquí como IDamageable permite que CUALQUIER cosa que haga daño (proyectiles del jugador,
/// espada, futuras trampas que dañen enemigos) lo use sin tener que conocer los tags a mano,
/// y además todos los enemigos nuevos lo reutilizan sin duplicar código.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int startingHealth = 3;

    [Header("Sound params")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip deadClip;

    public int CurrentHealth { get; private set; }
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = startingHealth;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHealth <= 0) return; // ya está muerto, evita doble-muerte por dos triggers el mismo frame

        int intAmount = Mathf.Max(1, Mathf.RoundToInt(amount));
        CurrentHealth -= intAmount;

        GameLogger.Info(GameLogger.Category.Enemy, $"{gameObject.name} recibe {intAmount} de daño ({CurrentHealth}/{startingHealth} vida restante).", this);

        if (CurrentHealth > 0)
        {
            PlaySafe(hurtClip);
        }
        else
        {
            PlaySafe(deadClip);
            OnDied?.Invoke();
            GameLogger.Info(GameLogger.Category.Enemy, $"{gameObject.name} destruido.", this);
            Destroy(gameObject);
        }
    }

    private void PlaySafe(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
