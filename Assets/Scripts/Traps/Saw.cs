using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float movementDistance = 3f;
    [SerializeField] private string targetTag = "Player";

    private bool movingLeft;
    private float leftEdge;
    private float rightEdge;

    private void Awake()
    {
        leftEdge = transform.position.x - movementDistance;
        rightEdge = transform.position.x + movementDistance;
    }

    private void Update()
    {
        float limit = movingLeft ? leftEdge : rightEdge;
        float direction = movingLeft ? -1f : 1f;

        if ((movingLeft && transform.position.x > limit) || (!movingLeft && transform.position.x < limit))
        {
            transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);
        }
        else
        {
            movingLeft = !movingLeft;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        var damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
        {
            GameLogger.Warning(GameLogger.Category.Trap, $"{name}: '{collision.name}' no implementa IDamageable.", this);
            return;
        }

        damageable.TakeDamage(damage);
    }
}
