/// <summary>
/// Cualquier cosa que pueda recibir daño (jugador, enemigos, objetos destructibles futuros...).
///
/// Por qué existe: el bug original era que Projectile.cs, al chocar con un enemigo, hacía
/// collision.GetComponent&lt;PlayerStats&gt;() — pero PlayerStats es del jugador, los enemigos
/// no lo tienen. Con IDamageable, tanto PlayerStats como EnemyHealth lo implementan, y el
/// proyectil/trampa que sea solo necesita pedir GetComponent&lt;IDamageable&gt;() sin
/// preocuparse de si el objetivo es el jugador o un enemigo.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
