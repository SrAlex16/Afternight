using UnityEngine;

/// <summary>
/// Única fuente de verdad del input del jugador. Antes esto estaba duplicado: PlayerMovement
/// leía teclado directamente y ScreenController recibía llamadas sueltas desde botones de UI,
/// cada uno con su propia copia de la lógica de movimiento/salto/ataque.
///
/// Ahora PlayerController y PlayerCombat solo preguntan a ESTA clase "¿cuál es el input ahora
/// mismo?", sin que les importe si viene de teclado (útil en el editor) o de los botones táctiles
/// del móvil. Ambas fuentes están siempre activas a la vez: se combinan con OR, así puedes seguir
/// probando con teclado en el editor sin desactivar nada.
///
/// Los campos de los botones táctiles son opcionales (pueden dejarse vacíos si el juego se prueba
/// solo con teclado); todo el acceso a ellos está protegido con comprobaciones de null.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Botones táctiles (opcional, para build de Android)")]
    [SerializeField] private TouchButton moveLeftButton;
    [SerializeField] private TouchButton moveRightButton;
    [SerializeField] private TouchButton jumpButton;
    [SerializeField] private TouchButton attackButton;
    [SerializeField] private TouchButton castButton;
    [SerializeField] private TouchButton dashButton;

    [Header("Teclado (para probar en el editor)")]
    [SerializeField] private bool enableKeyboard = true;

    public float Horizontal { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressedThisFrame { get; private set; }
    public bool AttackPressedThisFrame { get; private set; }
    public bool CastPressedThisFrame { get; private set; }
    public bool DashPressedThisFrame { get; private set; }

    private bool jumpHeldPrevFrame;
    private bool attackHeldPrevFrame;
    private bool castHeldPrevFrame;
    private bool dashHeldPrevFrame;

    private void Update()
    {
        Horizontal = ReadHorizontal();

        bool jumpHeldNow = IsHeld(jumpButton) || (enableKeyboard && Input.GetKey(KeyCode.Space));
        bool attackHeldNow = IsHeld(attackButton) || (enableKeyboard && Input.GetKey(KeyCode.Mouse0));
        bool castHeldNow = IsHeld(castButton) || (enableKeyboard && Input.GetKey(KeyCode.E));
        bool dashHeldNow = IsHeld(dashButton) || (enableKeyboard && Input.GetKey(KeyCode.LeftShift));

        JumpHeld = jumpHeldNow;
        JumpPressedThisFrame = jumpHeldNow && !jumpHeldPrevFrame;
        AttackPressedThisFrame = attackHeldNow && !attackHeldPrevFrame;
        CastPressedThisFrame = castHeldNow && !castHeldPrevFrame;
        DashPressedThisFrame = dashHeldNow && !dashHeldPrevFrame;

        jumpHeldPrevFrame = jumpHeldNow;
        attackHeldPrevFrame = attackHeldNow;
        castHeldPrevFrame = castHeldNow;
        dashHeldPrevFrame = dashHeldNow;
    }

    private float ReadHorizontal()
    {
        float value = 0f;

        bool leftHeld = IsHeld(moveLeftButton) || (enableKeyboard && Input.GetKey(KeyCode.A));
        bool rightHeld = IsHeld(moveRightButton) || (enableKeyboard && Input.GetKey(KeyCode.D));

        if (leftHeld) value -= 1f;
        if (rightHeld) value += 1f;

        return value;
    }

    private static bool IsHeld(TouchButton button)
    {
        return button != null && button.IsPressed;
    }
}
