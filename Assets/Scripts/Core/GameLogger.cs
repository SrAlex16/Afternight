using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Logger centralizado del juego.
///
/// Por qué existe en vez de usar Debug.Log directamente:
/// - Los logs se pueden filtrar por categoría (Player, Enemy, Combat, Pool...) y por nivel,
///   así en producción se puede bajar el ruido sin borrar ni comentar ninguna llamada.
/// - Los métodos Info/Verbose están marcados con [Conditional("GAME_LOGGING")], así que si esa
///   directiva de compilación no está activa (ver Project Settings > Player > Scripting Define
///   Symbols), el compilador ELIMINA esas llamadas por completo del build de Android. Cero coste
///   en release, logs completos en editor/desarrollo.
/// - Warning y Error nunca se eliminan: siempre queremos verlos, incluso en build final,
///   porque son señal de que algo va mal de verdad.
/// </summary>
public static class GameLogger
{
    public enum Category
    {
        General,
        Player,
        Enemy,
        Combat,
        Pool,
        UI,
        Trap
    }

    // Activa/desactiva categorías concretas desde el propio código si hace falta silenciar ruido
    // puntual sin tocar Scripting Define Symbols (útil mientras depuras una sola categoría).
    public static bool VerboseEnabled = true;

    [Conditional("GAME_LOGGING")]
    public static void Info(Category category, string message, UnityEngine.Object context = null)
    {
        Debug.Log($"[{category}] {message}", context);
    }

    [Conditional("GAME_LOGGING")]
    public static void Verbose(Category category, string message, UnityEngine.Object context = null)
    {
        if (!VerboseEnabled) return;
        Debug.Log($"[{category}:Verbose] {message}", context);
    }

    public static void Warning(Category category, string message, UnityEngine.Object context = null)
    {
        Debug.LogWarning($"[{category}] {message}", context);
    }

    public static void Error(Category category, string message, UnityEngine.Object context = null)
    {
        Debug.LogError($"[{category}] {message}", context);
    }

    public static void Exception(Category category, Exception exception, UnityEngine.Object context = null)
    {
        Debug.LogError($"[{category}] Excepción capturada: {exception.Message}\n{exception.StackTrace}", context);
    }
}
