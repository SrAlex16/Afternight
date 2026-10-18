# Guía de migración del refactor

## 1. Scripts que se BORRAN (sustituidos por otros nuevos)

| Antiguo | Sustituido por |
|---|---|
| `Player/PlayerMovement.cs` | `Player/PlayerController.cs` |
| `Menus/ScreenController.cs` | `Player/PlayerController.cs` |
| `Player/PlayerAttack.cs` | `Player/PlayerCombat.cs` |
| `Menus/ScreenAttackController.cs` | `Player/PlayerCombat.cs` |
| `Fireball.cs` (el duplicado muerto) | `Combat/Projectile.cs` |

**Antes de borrarlos**: en el editor de Unity, cualquier GameObject que tuviera estos
componentes va a perder sus referencias en el Inspector. Prepárate para volver a arrastrar
las referencias (Rigidbody2D ya no hace falta arrastrarlo, se coge solo con `GetComponent`,
pero `firePoint`, `projectilePrefab`, `swordHitbox`, los AudioClips, etc. sí).

## 2. Componentes nuevos que hay que añadir al GameObject del jugador

En el GameObject `Player` (o como se llame en tu escena), deberías acabar con estos
componentes en vez de los antiguos:

- `PlayerInput` (nuevo — ver punto 3 para los botones táctiles)
- `PlayerController` (requiere Rigidbody2D + PlayerInput en el mismo objeto)
- `PlayerCombat` (requiere PlayerInput)
- `PlayerStats` (requiere SpriteRenderer)

Y en el GameObject de la hitbox de la espada (el hijo que antes se activaba/desactivaba a
mano): añade `MeleeHitbox`.

## 3. Botones táctiles (arregla el bug de "no puedo pulsar dos a la vez")

Cada botón de la UI móvil (mover izq., mover dcha., saltar, atacar, cast, dash) necesita el
componente `TouchButton` (no hace falta que sea un `Button` de Unity, con que tenga un
`Image`/`Graphic` para que el raycaster de UI lo detecte y el componente `TouchButton` es
suficiente). Luego arrastra cada uno de esos botones a los campos correspondientes del
componente `PlayerInput` en el jugador.

Importante: si tu `Canvas` no tiene ya un `EventSystem` en la escena, Unity te avisará al
añadir el primer elemento de UI — déjalo con la configuración por defecto, ya soporta
multitáctil sin tocar nada más.

## 4. Layers para la invulnerabilidad del jugador

`PlayerStats` ahora pide explícitamente `playerLayer` y `hazardLayer` en el Inspector (antes
estaban hardcodeadas como `10` y `11`). Configúralas con las layers reales que uses en tu
proyecto para el jugador y para enemigos/trampas.

## 5. Tags que deben existir en el proyecto

`Player`, `Enemy`. Si usabas `Sword` como tag en algún sitio, ya no hace falta — la espada
ahora se resuelve con el componente `MeleeHitbox`, no por tag.

## 6. Activar los logs de desarrollo

Los logs `GameLogger.Info` / `GameLogger.Verbose` solo se compilan si añades el símbolo
`GAME_LOGGING` en **Project Settings > Player > Other Settings > Scripting Define Symbols**
(para la plataforma que estés compilando). Sin ese símbolo, esas llamadas desaparecen del
build — cero coste en el APK final. Los `Warning` y `Error` siempre se quedan.

## 7. Sobre el sistema día/noche de `SmallEnemy`

En el original había un intento de "los enemigos hacen más daño pasado un minuto" que
dependía de un `DayNightSystem2D` que no venía en los scripts que revisamos, y que además
estaba roto (comparaba con una variable que nunca se inicializaba). Lo he quitado del
`SmallEnemy` nuevo. Si ese sistema te importa, pásame `DayNightSystem2D.cs` y lo engancho
bien como un multiplicador de daño limpio.

## 8. Trampas nuevas

- `FallingSpikeTrap`: detecta al jugador por debajo con un raycast, avisa un momento (se
  pone roja) y cae. Bueno para pasillos verticales.
- `PoisonCloudTrap`: zona de daño continuo mientras el jugador está dentro — pensada para
  que valga la pena cruzarla con el dash en vez de caminando.

## 9. Testing recomendado

Antes de dar por bueno el refactor, prueba en este orden:
1. Movimiento + salto + wall jump con teclado en el editor.
2. Lo mismo con los botones táctiles en un build de Android (o Play Mode + Simulator táctil).
3. Mantener pulsado mover + pulsar saltar A LA VEZ (el bug original) — debería funcionar ya.
4. Dash, cooldown del dash, y que se resetee al tocar suelo si `dashResetsOnGround` está activo.
5. Ataque cuerpo a cuerpo contra un `SmallEnemy` (con `EnemyHealth` añadido) y ataque a
   distancia contra el mismo — ambos deberían hacer daño real ahora.
6. Las trampas existentes + las 2 nuevas.
