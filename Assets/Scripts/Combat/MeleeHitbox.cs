using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Va en el GameObject hijo del jugador que representa la hitbox de la espada
/// (el que PlayerCombat activa/desactiva durante el ataque cuerpo a cuerpo).
/// Antes esto dependía de que el enemigo comprobara collision.tag == "Sword"; ahora la propia
/// hitbox es responsable de aplicar el daño vía IDamageable, igual que hacen los proyectiles.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private string targetTag = "Enemy";

    // Evita golpear al mismo enemigo varias veces mientras la hitbox sigue activa el mismo swing.
    private readonly HashSet<IDamageable> hitThisSwing = new HashSet<IDamageable>();

    private void OnEnable()
    {
        hitThisSwing.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(targetTag)) return;

        var damageable = collision.GetComponent<IDamageable>();
        if (damageable == null)
        {
            GameLogger.Warning(GameLogger.Category.Combat, $"MeleeHitbox golpeó a '{collision.name}' pero no implementa IDamageable.", this);
            return;
        }

        if (hitThisSwing.Contains(damageable)) return;

        hitThisSwing.Add(damageable);
        damageable.TakeDamage(damage);
        GameLogger.Info(GameLogger.Category.Combat, $"Espada golpea a {collision.name} por {damage}.", this);
    }
}
