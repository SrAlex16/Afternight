using UnityEngine;

/// <summary>
/// Ataque cuerpo a cuerpo y ataque a distancia del jugador.
///
/// Sustituye a PlayerAttack.cs y ScreenAttackController.cs (duplicados, uno para teclado/ratón
/// y otro para botones táctiles). Lee el input desde PlayerInput igual que PlayerController.
///
/// BUG ARREGLADO: en el PlayerAttack original, cooldownTimer solo se reseteaba en CastAttack(),
/// nunca en Attack() (el mandoble), así que el ataque cuerpo a cuerpo no tenía cooldown real y
/// se podía spamear cada frame. Ahora ambos ataques comparten el mismo cooldown correctamente.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque cuerpo a cuerpo")]
    [SerializeField] private GameObject swordHitbox;
    [SerializeField] private float swordActiveDuration = 0.25f;

    [Header("Ataque a distancia")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int projectilePoolSize = 6;
    [SerializeField] private Transform firePoint;

    [Header("General")]
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordAttackClip;
    [SerializeField] private AudioClip castAttackClip;

    private Animator animator;
    private PlayerController playerController;
    private PlayerInput input;
    private ObjectPool<Projectile> projectilePool;
    private float cooldownTimer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        input = GetComponent<PlayerInput>();
        cooldownTimer = attackCooldown;

        if (projectilePrefab != null)
        {
            projectilePool = new ObjectPool<Projectile>(projectilePrefab, projectilePoolSize, transform.parent);
        }
        else
        {
            GameLogger.Warning(GameLogger.Category.Combat, "PlayerCombat sin projectilePrefab asignado; el ataque a distancia no funcionará.", this);
        }

        if (swordHitbox != null) swordHitbox.SetActive(false);
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer < attackCooldown) return;
        if (playerController != null && !playerController.CanAttack()) return;
        if (PauseMenu.GameIsPaused) return;

        if (input.AttackPressedThisFrame)
        {
            MeleeAttack();
        }
        else if (input.CastPressedThisFrame)
        {
            RangedAttack();
        }
    }

    private void MeleeAttack()
    {
        cooldownTimer = 0f;
        PlaySafe(swordAttackClip);
        animator?.SetTrigger("attack");

        if (swordHitbox != null)
        {
            swordHitbox.SetActive(true);
            Invoke(nameof(DisableSwordHitbox), swordActiveDuration);
        }

        GameLogger.Info(GameLogger.Category.Combat, "Ataque cuerpo a cuerpo.", this);
    }

    private void DisableSwordHitbox()
    {
        if (swordHitbox != null) swordHitbox.SetActive(false);
    }

    private void RangedAttack()
    {
        if (projectilePool == null || firePoint == null)
        {
            GameLogger.Error(GameLogger.Category.Combat, "No se puede lanzar proyectil: falta projectilePool o firePoint.", this);
            return;
        }

        cooldownTimer = 0f;
        PlaySafe(castAttackClip);
        animator?.SetTrigger("cast");

        Projectile projectile = projectilePool.Get();
        projectile.SetPool(projectilePool);
        projectile.transform.position = firePoint.position;
        projectile.Launch(Mathf.Sign(transform.localScale.x));

        GameLogger.Info(GameLogger.Category.Combat, "Ataque a distancia lanzado.", this);
    }

    private void PlaySafe(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
