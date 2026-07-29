using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Botón táctil para los controles en pantalla (mover izquierda/derecha, saltar, atacar, dash...).
///
/// POR QUÉ ARREGLA EL BUG DE "NO PUEDO PULSAR DOS BOTONES A LA VEZ":
/// El problema típico en la versión anterior es usar Button.onClick (que solo dispara un evento
/// puntual al soltar, sin estado de "mantenido pulsado") y/o depender de un único puntero/input
/// compartido entre botones. Aquí cada botón:
///   1. Mantiene su PROPIO estado booleano (IsPressed), independiente de cualquier otro botón.
///   2. Usa IPointerDownHandler/IPointerUpHandler, que en uGUI se disparan por cada puntero
///      (pointerId) de forma independiente — el sistema de eventos de Unity ya soporta múltiples
///      punteros simultáneos (cada dedo en pantalla es un pointerId distinto), el problema nunca
///      fue una limitación técnica sino cómo se leía el estado.
///   3. Si el dedo se arrastra fuera del botón sin soltar, se cuenta como "soltado" (OnPointerExit)
///      para que no se quede pegado un input fantasma.
///
/// Con esto, PlayerInput puede leer "está el botón de mover-izquierda pulsado" Y "está el botón
/// de salto pulsado" en el mismo frame, sin que se pisen entre ellos.
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool IsPressed { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Si el dedo se arrastra fuera del botón, lo tratamos como soltado para no dejar
        // el input "pegado" a true indefinidamente.
        if (eventData.dragging || eventData.pointerId != -1)
        {
            IsPressed = false;
        }
    }

    private void OnDisable()
    {
        // Si el botón se desactiva (p.ej. al pausar) mientras estaba pulsado, evitamos que
        // se quede el estado en true para siempre.
        IsPressed = false;
    }
}
