# Guía de configuración en el Editor de Unity

Los scripts de las 4 etapas ya están en `Assets/Scripts/`. Lo que falta es
conectarlos en el editor. Sigue estos pasos en orden.

---

## 1. Animaciones del personaje (lo más importante)

### El problema actual
Tus FBX **no comparten el mismo tipo de rig**, por eso ninguna anima al Warrior:

| Archivo | Rig actual | Esqueleto |
|---------|-----------|-----------|
| `Warrior.fbx` (modelo) | **Generic** | Blender (`CharacterArmature`, `Foot.L`, `Root`) |
| `Run Right/Left/Backward`, `*Jump*`, `X Bot@...` | **Generic** | Mixamo (`mixamorig:...`) |
| `Player_Idle`, `Player_walck_Adelante`, `Player_walk_*` | **Humanoid** | (sin avatar asignado) |

Como son esqueletos distintos, una animación Generic solo reproduce en SU propio
esqueleto. El Warrior queda en T-pose.

### La solución más eficiente: **Humanoid retargeting**
Humanoid traduce cualquier animación humanoide a cualquier modelo humanoide,
sin importar el nombre de los huesos. Es lo mejor cuando mezclas Mixamo + clips
propios (tu caso). Pasos:

**A) Convertir el modelo Warrior a Humanoid (una sola vez)**
1. Selecciona `Assets/Models/FBX_Player/Warrior.fbx` en el Project.
2. Pestaña **Rig** → *Animation Type* = **Humanoid** → *Avatar Definition* =
   **Create From This Model** → **Apply**.
3. Pulsa **Configure...**. Si Unity muestra huesos en rojo (faltantes), arrástralos
   manualmente desde la jerarquía a los slots del cuerpo. Con nombres de Blender
   suele faltar mapear:
   - `LeftFoot` → hueso `Foot.L`  ·  `RightFoot` → `Foot.R`
   - `LeftLowerLeg` → `Shin.L` (o `LowerLeg.L`)  ·  derecha igual
   - `LeftUpperLeg` → `Thigh.L` (o `UpperLeg.L`)  ·  derecha igual
   - Revisa también manos/brazos si aparecen en rojo.
4. **Done** y **Apply**. El círculo del avatar debe quedar todo verde.

**B) Convertir TODAS las animaciones a Humanoid**
Para cada FBX de `Assets/Art/Animation/Player/` que quieras usar:
1. Selecciónalo → pestaña **Rig** → *Animation Type* = **Humanoid**.
2. *Avatar Definition* = **Copy From Other Avatar** → *Source* = el **Avatar del
   Warrior** (`WarriorAvatar`, que se generó en el paso A).
3. **Apply**. (Puedes seleccionar varios FBX a la vez y aplicar en lote.)
4. En la pestaña **Animation**, marca **Loop Time** en Idle/caminar/correr.

> Alternativa más simple (sin Mixamo): si solo usas los clips `Player_*` y estos
> fueron exportados con el esqueleto del Warrior, pon TODO en **Generic** con el
> avatar del Warrior. Es menos flexible (descartas los clips Mixamo) pero directo.
> Recomendado: Humanoid, porque aprovechas todas las animaciones.

### C) Armar el Animator Controller
El controller `Assets/Animation/PlayerAnimator.controller` ya existe pero está vacío.
`PlayerAnimator.cs` espera estos parámetros: **Speed** (Float), **IsGrounded** (Bool),
**Jump** (Trigger). Configúralo así:

1. Abre el controller (doble click) → ventana **Animator**.
2. En **Parameters** crea: `Speed` (Float), `IsGrounded` (Bool), `Jump` (Trigger).
   (Borra el parámetro de ejemplo "New Float".)
3. **Blend Tree de locomoción**:
   - Click derecho en el grid → *Create State → From New Blend Tree*. Llámalo `Locomotion` y ponlo como **default** (click derecho → Set as Layer Default State).
   - Doble click al Blend Tree → *Blend Type* = **1D**, *Parameter* = `Speed`.
   - Añade *Motion Fields* y arrastra los clips:
     - Threshold **0.0** → `Player_Idle`
     - Threshold **0.5** → `Player_walck_Adelante` (o `Run Backward`/walk)
     - Threshold **1.0** → `Run Right`/clip de correr hacia adelante
   - Marca *Automate Thresholds* off si los ajustas a mano.
4. **Salto**:
   - Crea un estado `Jump` y arrastra `X Bot@Jump Down` (o `Backward Jump`).
   - Transición `Locomotion → Jump`: condición `Jump` (trigger). Desactiva *Has Exit Time*.
   - Transición `Jump → Locomotion`: condición `IsGrounded == true`, con *Has Exit Time* o por duración.
5. Guarda. Selecciona el Player en la escena: componente **Animator** →
   *Controller* = `PlayerAnimator`, *Avatar* = `WarriorAvatar`, *Apply Root Motion* = **OFF**
   (el movimiento lo maneja `PlayerMovement`, no la animación).

> `PlayerMovement` ya expone `Speed` (0..1) e `IsGrounded`, y `PlayerAnimator.cs`
> los envía al Animator automáticamente. No tienes que tocar código.

---

## 2. Etapa 3 — Enemigo con NavMesh

1. **Hornear el NavMesh**:
   - *Window → AI → Navigation* (si no aparece, instala el paquete **AI Navigation**
     en Package Manager). Marca el suelo/escenario como **Navigation Static** →
     pestaña **Bake → Bake**. Debe aparecer la malla azul sobre el piso.
   - Con el paquete nuevo (NavMeshSurface): añade un GameObject con componente
     **NavMeshSurface** al escenario y pulsa **Bake**.
2. **Tag del jugador**: selecciona el Player → arriba, *Tag* = **Player**
   (créalo si no existe).
3. **Prefab de enemigo**:
   - Crea un GameObject (cápsula o un modelo) y añádele: **NavMeshAgent**,
     **EnemyAI**, **EnemyHealth**, y un **Collider** (no trigger).
   - En `EnemyAI` ajusta *Detection Range*, *Attack Range*, *Attack Damage*.
   - En `EnemyHealth` ajusta *Max Health* y *Score Value* (puntos al morir).
   - Arrástralo a `Assets/Prefabs/` para crear el prefab y coloca varios en la escena.
4. Verifica: al acercarte, el enemigo te persigue (Gizmos muestran los rangos al
   seleccionarlo) y te quita vida al estar a rango.

---

## 3. Etapa 4 — Game loop y UI

1. Crea un GameObject vacío llamado **GameManager** y añádele los componentes
   **GameManager**, **ScoreManager** y **GameStateUI**.
   - `GameManager` → *Start In Menu* = **ON** (arranca en el menú principal).
2. Asegúrate de que el Player tenga **PlayerHealth** y un **HUDDisplay** en la escena
   (el HUD ya muestra salud y munición; `GameStateUI` añade puntaje y menús).
3. Reproduce la escena:
   - Aparece el **menú principal** (juego en pausa). Botón **JUGAR** inicia.
   - Arriba a la derecha se ve **PUNTOS** (suben al derrotar enemigos).
   - Si tu vida llega a 0 → **GAME OVER**. Si derrotas a todos → **VICTORIA**.
   - Botones **REINTENTAR** / **MENÚ PRINCIPAL** recargan la escena.

> Nota: el menú es un overlay en la misma escena (no requiere una segunda escena
> ni configurar Build Settings). Si tu profesor pide una escena de menú aparte,
> puedes mover `GameStateUI` a una escena `MainMenu` y usar `SceneManager.LoadScene`.

---

## 4. Checklist final
- [ ] Warrior en Humanoid con avatar verde; clips en Humanoid (Copy From Warrior).
- [ ] Animator con parámetros Speed/IsGrounded/Jump y Blend Tree de locomoción.
- [ ] NavMesh horneado; Player con tag `Player`.
- [ ] Prefab de enemigo con NavMeshAgent + EnemyAI + EnemyHealth + Collider.
- [ ] GameObject GameManager con GameManager + ScoreManager + GameStateUI.
- [ ] Probar: menú → jugar → puntos → victoria/derrota → reintentar.
