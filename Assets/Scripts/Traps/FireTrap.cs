using System.Collections;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private float activeTime = 1f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private Color warningColor = Color.red;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool triggered;
    private bool active;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        if (!triggered)
        {
            StartCoroutine(ActivateFireTrap());
        }

        if (active)
        {
            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
            }
        }
    }

    private IEnumerator ActivateFireTrap()
    {
        triggered = true;
        if (spriteRenderer != null) spriteRenderer.color = warningColor;

        yield return new WaitForSeconds(activationDelay);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        active = true;
        animator?.SetBool("activated", true);

        yield return new WaitForSeconds(activeTime);

        active = false;
        triggered = false;
        animator?.SetBool("activated", false);
    }
}
