using UnityEngine;

public class Patrol : MonoBehaviour
{
    [Header("Patrol points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;

    [Header("Enemy")]
    [SerializeField] private Transform enemy;

    [Header("Movement parameters")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float idleDuration = 1f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private Vector3 initScale;
    private bool movingLeft;
    private float idleTimer;

    private void Awake()
    {
        if (enemy == null) enemy = transform;
        initScale = enemy.localScale;
    }

    private void OnDisable()
    {
        if (animator != null) animator.SetBool("moving", false);
    }

    private void Update()
    {
        bool atLeftEdge = movingLeft && enemy.position.x < leftEdge.position.x;
        bool atRightEdge = !movingLeft && enemy.position.x > rightEdge.position.x;

        if (atLeftEdge || atRightEdge)
        {
            DirectionChange();
        }
        else
        {
            MoveInDirection(movingLeft ? -1 : 1);
        }
    }

    private void DirectionChange()
    {
        if (animator != null) animator.SetBool("moving", false);
        idleTimer += Time.deltaTime;
        if (idleTimer > idleDuration)
        {
            movingLeft = !movingLeft;
        }
    }

    private void MoveInDirection(int direction)
    {
        idleTimer = 0f;
        if (animator != null) animator.SetBool("moving", true);

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * direction, initScale.y, initScale.z);
        enemy.position += new Vector3(Time.deltaTime * direction * speed, 0f, 0f);
    }
}
