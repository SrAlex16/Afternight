using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Movimiento, salto, wall-jump y dash del jugador.
///
/// Sustituye a PlayerMovement.cs (teclado) y ScreenController.cs (botones táctiles), que eran
/// prácticamente el mismo código copiado dos veces y ya habían empezado a desincronizarse (p.ej.
/// wallJumpCoolDown arrancaba en 0 en uno y en 1 en el otro). Ahora solo hay una implementación,
/// y lee el input desde PlayerInput sin que le importe si viene de teclado o de pantalla táctil.
///
/// Todos los valores de balance son [SerializeField], nada de constantes hardcodeadas
/// (la gravedad "3" y los valores de empuje del wall-jump "10"/"3"/"6" que había sueltos en el
/// código original ahora son campos configurables desde el inspector).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpSpeed = 12f;
    [SerializeField] private float normalGravityScale = 3f;

    [Header("Wall jump / wall slide")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float gravityOnWall = 0.5f;
    [SerializeField] private float wallJumpCooldown = 0.2f;
    [SerializeField] private float wallJumpPushSpeed = 10f;
    [SerializeField] private Vector2 wallJumpVelocity = new Vector2(3f, 6f);

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private bool dashResetsOnGround = true;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip dashClip;

    public event Action OnJumped;
    public event Action OnDashed;

    private Rigidbody2D body;
    private Animator playerAnimator;
    private BoxCollider2D boxCollider;
    private PlayerInput input;

    private float wallJumpCooldownTimer;
    private float dashCooldownTimer;
    private bool isDashing;
    private bool wasGroundedLastFrame;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        input = GetComponent<PlayerInput>();

        if (playerAnimator == null)
            GameLogger.Warning(GameLogger.Category.Player, "PlayerController no encuentra Animator. No habrá animaciones de movimiento.", this);

        wallJumpCooldownTimer = wallJumpCooldown; // permite saltar desde el primer frame
        dashCooldownTimer = dashCooldown;
    }

    private void Update()
    {
        dashCooldownTimer += Time.deltaTime;

        if (isDashing) return; // durante el dash ignoramos el resto del input de movimiento

        HandleFacing();
        HandleAnimatorParams();
        HandleJumpInput();
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        wallJumpCooldownTimer += Time.fixedDeltaTime;
        if (wallJumpCooldownTimer <= wallJumpCooldown) return;

        body.velocity = new Vector2(input.Horizontal * moveSpeed, body.velocity.y);

        bool grounded = IsGrounded();
        if (OnWall() && !grounded)
        {
            body.gravityScale = gravityOnWall;
            body.velocity = Vector2.zero;
        }
        else
        {
            body.gravityScale = normalGravityScale;
        }

        if (dashResetsOnGround && grounded && !wasGroundedLastFrame)
        {
            dashCooldownTimer = dashCooldown;
        }
        wasGroundedLastFrame = grounded;
    }

    private void HandleFacing()
    {
        if (input.Horizontal > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (input.Horizontal < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void HandleAnimatorParams()
    {
        if (playerAnimator == null) return;
        playerAnimator.SetBool("isRunning", input.Horizontal != 0f);
        playerAnimator.SetBool("grounded", IsGrounded());
    }

    private void HandleJumpInput()
    {
        if (!input.JumpPressedThisFrame) return;
        if (wallJumpCooldownTimer <= wallJumpCooldown) return;

        Jump();
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            body.velocity = new Vector2(body.velocity.x, jumpSpeed);
            playerAnimator?.SetTrigger("jump");
            PlaySafe(jumpClip);
            OnJumped?.Invoke();
            GameLogger.Verbose(GameLogger.Category.Player, "Salto normal.", this);
        }
        else if (OnWall())
        {
            int facing = (int)Mathf.Sign(transform.localScale.x);

            if (Mathf.Approximately(input.Horizontal, 0f))
            {
                body.velocity = new Vector2(-facing * wallJumpPushSpeed, 0f);
                transform.localScale = new Vector3(-facing, transform.localScale.y, transform.localScale.z);
            }
            else
            {
                body.velocity = new Vector2(-facing * wallJumpVelocity.x, wallJumpVelocity.y);
            }

            PlaySafe(jumpClip);
            OnJumped?.Invoke();
            wallJumpCooldownTimer = 0f;
            GameLogger.Verbose(GameLogger.Category.Player, "Wall jump.", this);
        }
    }

    private void HandleDashInput()
    {
        if (!input.DashPressedThisFrame) return;
        if (dashCooldownTimer < dashCooldown) return;

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = 0f;

        float direction = Mathf.Sign(transform.localScale.x);
        float originalGravity = body.gravityScale;
        body.gravityScale = 0f;
        body.velocity = new Vector2(direction * dashSpeed, 0f);

        playerAnimator?.SetTrigger("dash");
        PlaySafe(dashClip);
        OnDashed?.Invoke();
        GameLogger.Info(GameLogger.Category.Player, "Dash iniciado.", this);

        yield return new WaitForSeconds(dashDuration);

        body.gravityScale = originalGravity;
        body.velocity = new Vector2(0f, body.velocity.y);
        isDashing = false;
    }

    /// <summary>Usado por PlayerCombat para saber si el jugador puede atacar ahora mismo.</summary>
    public bool CanAttack()
    {
        return !isDashing && Mathf.Approximately(input.Horizontal, 0f) && IsGrounded() && !OnWall();
    }

    public bool IsGrounded()
    {
        if (boxCollider == null) return false;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return hit.collider != null;
    }

    public bool OnWall()
    {
        if (boxCollider == null) return false;
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, new Vector2(transform.localScale.x, 0f), 0.1f, wallLayer);
        return hit.collider != null;
    }

    private void PlaySafe(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
