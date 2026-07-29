using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Vida del jugador. Cambios respecto a la versión original:
/// - Implementa IDamageable, así cualquier trampa/proyectil/enemigo puede dañar al jugador
///   con GetComponent&lt;IDamageable&gt;() sin acoplarse a esta clase concreta.
/// - Expone OnHealthChanged para que la UI (HealthBarController) reaccione a eventos en vez
///   de leer currentHealth cada frame en Update().
/// - BUG ARREGLADO: en la Invunerability() original, la mitad "blanca" del parpadeo esperaba
///   1 segundo fijo en vez de invunerabilityDuration/(flashes*2), así que la duración real de
///   la invulnerabilidad no coincidía con el parámetro configurado en el inspector.
/// - Todos los valores relevantes son [SerializeField] configurables desde Unity.
/// - Los accesos a componentes/arrays llevan comprobaciones para no reventar con
///   NullReferenceException si falta algo en el inspector; en su lugar se loguea un error claro.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float startingHealth = 10f;
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private float deathMenuDelay = 1f;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private int flashes = 4;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask hazardLayer;

    [Header("Sound params")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip deadClip;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => startingHealth;
    public bool IsInvulnerable { get; private set; }

    /// <summary>Se dispara con (vidaActual, vidaMaxima) cada vez que la vida cambia.</summary>
    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        CurrentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (anim == null)
            GameLogger.Warning(GameLogger.Category.Player, "PlayerStats no encuentra Animator. Las animaciones de daño/muerte no se reproducirán.", this);
        if (deathMenu == null)
            GameLogger.Warning(GameLogger.Category.Player, "PlayerStats no tiene deathMenu asignado en el inspector.", this);
    }

    public void TakeDamage(float amount)
    {
        if (IsInvulnerable)
        {
            GameLogger.Verbose(GameLogger.Category.Player, $"Daño de {amount} ignorado (jugador invulnerable).", this);
            return;
        }

        if (amount < 0)
        {
            GameLogger.Warning(GameLogger.Category.Player, $"TakeDamage llamado con valor negativo ({amount}), se ignora.", this);
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, startingHealth);
        GameLogger.Info(GameLogger.Category.Player, $"Jugador recibe {amount} de daño. Vida: {CurrentHealth}/{startingHealth}", this);
        OnHealthChanged?.Invoke(CurrentHealth, startingHealth);

        if (CurrentHealth > 0)
        {
            PlaySafe(damageClip);
            SafeTrigger("hurt");
            StartCoroutine(InvulnerabilityRoutine());
        }
        else
        {
            HandleDeath();
        }
    }

    /// <summary>Cura al jugador. Reemplaza al antiguo método "Health(float)" con un nombre más claro.</summary>
    public void Heal(float amount)
    {
        if (amount < 0)
        {
            GameLogger.Warning(GameLogger.Category.Player, $"Heal llamado con valor negativo ({amount}), se ignora.", this);
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, startingHealth);
        OnHealthChanged?.Invoke(CurrentHealth, startingHealth);
    }

    private void HandleDeath()
    {
        PlaySafe(deadClip);
        SafeTrigger("die");
        OnDied?.Invoke();
        StartCoroutine(ShowDeathMenuAfterDelay());
    }

    private IEnumerator ShowDeathMenuAfterDelay()
    {
        yield return new WaitForSecondsRealtime(deathMenuDelay);

        if (deathMenu != null)
        {
            deathMenu.SetActive(true);
        }
        else
        {
            GameLogger.Error(GameLogger.Category.Player, "El jugador ha muerto pero no hay deathMenu asignado; no se mostrará ninguna pantalla.", this);
        }

        Time.timeScale = 0f;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        IsInvulnerable = true;

        // Solo si ambas layers están configuradas evitamos tocar la matriz de colisiones con -1/-1
        bool canToggleLayers = playerLayer.value != 0 && hazardLayer.value != 0;
        if (canToggleLayers)
            SetLayerCollision(true);

        float halfFlash = invulnerabilityDuration / (flashes * 2f);

        for (int i = 0; i < flashes; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
            yield return new WaitForSeconds(halfFlash);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(halfFlash);
        }

        if (canToggleLayers)
            SetLayerCollision(false);

        IsInvulnerable = false;
    }

    private void SetLayerCollision(bool ignore)
    {
        int playerLayerIndex = LayerMaskToLayer(playerLayer);
        int hazardLayerIndex = LayerMaskToLayer(hazardLayer);

        if (playerLayerIndex < 0 || hazardLayerIndex < 0)
        {
            GameLogger.Warning(GameLogger.Category.Player, "playerLayer/hazardLayer deben tener una única layer seleccionada para poder ignorar colisiones.", this);
            return;
        }

        Physics2D.IgnoreLayerCollision(playerLayerIndex, hazardLayerIndex, ignore);
    }

    private static int LayerMaskToLayer(LayerMask mask)
    {
        int value = mask.value;
        if (value == 0) return -1;
        int layer = 0;
        while ((value & 1) == 0)
        {
            value >>= 1;
            layer++;
            if (layer > 31) return -1;
        }
        return layer;
    }

    private void PlaySafe(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private void SafeTrigger(string triggerName)
    {
        if (anim == null) return;
        anim.SetTrigger(triggerName);
    }
}
