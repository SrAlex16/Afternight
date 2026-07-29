using System.Collections;
using UnityEngine;

/// <summary>
/// Trampa nueva: un pincho suspendido que detecta al jugador pasando por debajo, se sacude
/// (telegraph) durante un aviso breve y luego cae. Si golpea al jugador, daño. Tras el impacto
/// (contra el jugador o contra el suelo) espera un tiempo y vuelve a subir a su posición inicial.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FallingSpikeTrap : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float warningDuration = 0.5f;

    [Header("Caída")]
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private float damage = 2f;
    [SerializeField] private string targetTag = "Player";

    [Header("Reset")]
    [SerializeField] private float resetDelay = 1.5f;
    [SerializeField] private float returnSpeed = 4f;

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isBusy; // true mientras avisa, cae o vuelve a subir: evita re-triggers

    private void Awake()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (isBusy) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, detectionRange, playerLayer);
        if (hit.collider != null)
        {
            StartCoroutine(TriggerRoutine());
        }
    }

    private IEnumerator TriggerRoutine()
    {
        isBusy = true;

        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        GameLogger.Verbose(GameLogger.Category.Trap, $"{name}: pincho avisando antes de caer.", this);
        yield return new WaitForSeconds(warningDuration);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        // Cae hasta chocar con algo o hasta pasarse muy por debajo de su posición inicial (red de seguridad).
        float safetyLimitY = initialPosition.y - detectionRange * 3f;
        while (transform.position.y > safetyLimitY)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(resetDelay);

        while (Vector3.Distance(transform.position, initialPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = initialPosition;
        isBusy = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBusy || !collision.CompareTag(targetTag)) return;

        var damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.TakeDamage(damage);
        else
            GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionRange);
    }
}
