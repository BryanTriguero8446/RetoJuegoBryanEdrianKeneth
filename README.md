# Juego Unity — Reto "Juego"

Proyecto de juego 3D en tercera persona desarrollado en **Unity 2022.3 LTS** para
la asignatura de Programación Multimedia (UNIFRANZ). El proyecto cubre las 4
etapas del reto: locomoción, combate y animación, IA con NavMesh, y game loop + UI.

## Controles
| Acción | Tecla |
|--------|-------|
| Moverse | WASD / Flechas |
| Saltar | Espacio |
| Rotar cámara | Mouse |
| Disparar | Click izquierdo |
| Recargar | R |
| Menú / Jugar / Reintentar | Click en los botones |

## Jerarquía de Scripts
```
Assets/Scripts/
├── Player/
│   ├── PlayerMovement.cs      Movimiento, rotación y estados (Idle/Run/Jump) con CharacterController
│   ├── PlayerAnimator.cs      Puente PlayerMovement → Animator (Speed, IsGrounded, Jump)
│   ├── WeaponSystem.cs        Disparo (proyectiles), munición y recarga, eventos para UI
│   ├── PlayerHealth.cs        Salud del jugador, implementa IDamageable
│   ├── ArmShootAnimation.cs   Animación procedural del brazo al disparar
│   └── PlayerInitializer.cs   Validación/auto-setup de componentes del Player
├── Enemy/
│   ├── EnemyAI.cs             IA con NavMeshAgent: Idle → Chase → Attack
│   ├── EnemyHealth.cs         Salud del enemigo (IDamageable) + evento estático de muerte
│   └── TargetDummy.cs         Objetivo de práctica con feedback visual
├── Combat/
│   ├── IDamageable.cs         Interfaz de daño (desacopla combate de tipos concretos)
│   ├── Projectile.cs          Bala física: aplica daño vía IDamageable + VFX de impacto
│   └── BulletFactory.cs       Fábrica de balas y VFX
├── Core/
│   ├── GameManager.cs         Game loop: estados, victoria/derrota, pausa, reinicio
│   └── ScoreManager.cs        Contador de puntos (reacciona a muertes de enemigos)
└── UI/
    ├── HUDDisplay.cs          HUD en partida: salud, munición, recarga, crosshair
    └── GameStateUI.cs         Menú principal, puntaje, pantallas de Victoria/Game Over
```

## Arquitectura (decisiones de diseño)
- **Interfaz `IDamageable`**: jugador, enemigos y dummies reciben daño por el mismo
  contrato, sin que el sistema de combate conozca sus clases concretas.
- **Eventos en lugar de referencias directas**: `WeaponSystem`, `PlayerHealth`,
  `EnemyHealth`, `GameManager` y `ScoreManager` exponen eventos (`OnAmmoChanged`,
  `OnHealthChanged`, `OnStateChanged`, `OnScoreChanged`, `OnAnyEnemyDeath`). La UI
  se suscribe a ellos, de modo que la lógica del juego no depende de la visualización.
- **Game loop centralizado**: `GameManager` decide victoria (todos los enemigos
  derrotados) y derrota (muerte del jugador) y controla `Time.timeScale` y el cursor.

## Etapas del reto
1. **Locomoción** — `PlayerMovement` + Cinemachine FreeLook.
2. **Combate y animación** — `WeaponSystem`, `Projectile`, Animator con Blend Tree.
3. **IA e interacción** — `EnemyAI` (NavMeshAgent) + daño por `IDamageable`.
4. **Game loop y UI** — `GameManager`, `ScoreManager`, `GameStateUI` (menú, score, game over).

## Requisitos
- Unity 2022.3 LTS o superior
- Paquetes: Cinemachine, AI Navigation (NavMesh)

## Configuración en el Editor
Ver **`SETUP_EDITOR.md`** para los pasos de configuración del Animator (animaciones
del personaje), horneado del NavMesh, prefab de enemigo y escena de juego.
