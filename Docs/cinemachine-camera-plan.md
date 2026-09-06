# Cinemachine camera work for SimpleSkyRoads

> **Status: implemented** (September 2026). The rig lives under a `Cameras` root in `MainScene`,
> holding `CameraDirector` and four shots: `CM Menu Camera` (top-down orbit with handheld noise),
> `CM Play Camera` (damped world-space follow), `CM Boost Camera` (wider, pulled back, `6D Shake`)
> and `CM Crash Camera` (orbit around the wreck on `OnSessionEnded`, back to the play shot on
> retry). Blend times are in `Assets/Settings/CameraBlends.asset`. Deviations from the plan below:
> the orbit spinner is the generic `AutoOrbit.cs` (used by both orbits) instead of
> `MenuCameraOrbit.cs`; the cameras are children of `Cameras` rather than scene roots; and the
> game-over orbit that section 4 leaves out *was* added, on request. **How the finished rig
> works, component by component, is in `cinemachine-camera-guide.md`.**

## Context

`Assets/Scripts/CameraFollow.cs` is a 32-line rigid follow. Its own header comment states the
problem:

> Parks the camera rigidly behind the ship - from Awake and on every frame after it.
> No damping and no per-state distances: the framing is identical on the main menu,
> during play, while boosting and on game over.

Two things are wanted:

1. A camera that **orbits the ship on the main menu**.
2. A **smooth (damped) follow** during play, replacing the hard snap.

Cinemachine gives both, plus the blend between them for free. This is also a natural Session-8
teaching beat: the current script is the "before" picture and Cinemachine is the "after".

### What the exploration established

- **Cinemachine is not installed.** No `com.unity.cinemachine` in `Packages/manifest.json` or
  `packages-lock.json`. Step one is adding it.
- **`CameraFollow.cs` is the only script that touches a camera.** No `Camera.main`, no
  `ScreenToWorldPoint`, no parallax, no World-Space UI anywhere in `Assets/Scripts`. Cinemachine
  can take ownership of the transform cleanly.
- **`Main Camera` is a scene root** (`m_Father: 0`, no children), perspective, FOV 60, clear
  flags Skybox. Components: `Transform`, `Camera`, `AudioListener`, `CameraFollow`,
  and PostProcessing v2 `PostProcessLayer`.
- **The follow target is the `Player` transform** inside the `PlayerContainer` prefab instance,
  at world `(0, 0.61, 0)`. With `_offset = (0, 2, -6)` that puts the camera at `(0, 2.61, -6)`
  with an 18.435° pitch — exactly the scene's authored values.
- **`_offset` is a world-space vector.** The camera never rolls or yaws with the ship. Preserving
  that is a hard requirement (see Binding Mode below).
- **The ship never moves forward.** It is a treadmill: `Player` sits at fixed Z, only X changes
  (clamped ±5.63), plus a cosmetic Z-roll lean from `ShipControls.HorizontalLean`. The road
  scrolls via *texture offset*, so the world never actually translates in Z.
- **The menu is a state, not a scene.** One scene, `Assets/Scenes/MainScene.unity`. `GUIManager`
  cross-fades `Menu` / `HUD` / `GameOver` CanvasGroups. `GameManager.OnSessionStarted` (static
  event) is the unambiguous "gameplay began" signal, fired from `StartGame()` on both first start
  and Retry.
- **There is no "entered main menu" event** — the menu is simply the initial state
  (`_gameInSession` defaults false). And `_startedOnce` is never reset, so the title menu is
  one-shot: after the first death you go game-over → play, never back to the title.
- **No `.asmdef` files.** Everything compiles into `Assembly-CSharp`, which references all package
  assemblies automatically. No assembly wiring needed for Cinemachine.

---

## Components to add

Unity 6 ships **Cinemachine 3.x**, whose component names differ from every CM2 tutorial online.
Since this repo is teaching material, use the CM3 names consistently.

| Concern | CM3 component | (CM2 name, for reference) |
| --- | --- | --- |
| Camera ownership + blending | `CinemachineBrain` | same |
| A shot | `CinemachineCamera` | `CinemachineVirtualCamera` |
| Menu orbit | `CinemachineOrbitalFollow` | Orbital Transposer / FreeLook |
| Gameplay follow | `CinemachineFollow` | Transposer |
| Aiming | `CinemachineRotationComposer` / `CinemachineHardLookAt` | Composer / Hard Look At |
| Boost shake (optional) | `CinemachineBasicMultiChannelPerlin` | same |

### 0. Package

Add `com.unity.cinemachine` (3.x) via Package Manager. Namespace is `Unity.Cinemachine`.

### 1. On `Main Camera` — `CinemachineBrain`

- **Update Method: `LateUpdate`** — matches what `CameraFollow` did, and keeps PPv2 happy.
- **Default Blend: Ease In Out, ~1.5–2 s.** This single setting is what makes the menu→gameplay
  transition a swoop instead of a cut. No code required.
- Leave `PostProcessLayer` on this camera. It stays valid — its `volumeTrigger` points at the
  camera's own transform, and the global `PostProcessVolume` on the `PostProcessing` root is
  unaffected.

### 2. `CM Menu Camera` — the orbit

New empty GameObject, scene root.

- `CinemachineCamera`, **Priority 20** (wins at scene load, so the menu opens on the orbit).
  - Tracking Target = the `Player` transform.
- `CinemachineOrbitalFollow` (Position Control)
  - Orbit Style: **Sphere** (simplest; ThreeRing is overkill here).
  - Radius ≈ **6.3** — matches the current `(0, 2, -6)` distance, so the orbit reads as "the same
    shot, moving" rather than a different framing.
  - Vertical Axis parked around **15–20°** so it looks slightly down the ship, like play does.
  - Horizontal Axis: driven by script (below). Do **not** add
    `CinemachineInputAxisController` — that is for player-driven orbits; we want an automatic idle spin.
- `CinemachineRotationComposer` (Rotation Control) to keep the ship framed, or
  `CinemachineHardLookAt` if you want it dead-centre and rigid.
- New script **`MenuCameraOrbit.cs`**: each frame, advance
  `CinemachineOrbitalFollow.HorizontalAxis.Value` by a serialized `_degreesPerSecond`, wrapping at
  360. ~15 lines. Disable itself once the session starts.

Because everything in the world is frozen during the menu (every mover gates on
`GameManager.Instance.GameInSession`), the orbit shows a still ship over a still road — which is
exactly the desired attract-mode look.

> **Zero-new-code alternative:** the project already has `Assets/Scripts/Rotate.cs`
> (`_transform.Rotate(_rotationSpeed * Time.deltaTime, _rotationSpace)`). Parent a
> `CinemachineCamera` that has **no** Position Control ("Do Nothing") under an empty pivot placed
> at the ship, put `Rotate.cs` on the pivot, and give the vcam only `CinemachineHardLookAt`. This
> reuses existing course code and needs no new script. It is less idiomatic Cinemachine, so prefer
> `CinemachineOrbitalFollow` if the point is to *teach* Cinemachine.

### 3. `CM Play Camera` — the smooth follow

New empty GameObject, scene root.

- `CinemachineCamera`, **Priority 10**.
  - Tracking Target = the `Player` transform.
- `CinemachineFollow` (Position Control)
  - **Binding Mode: `World Space`** — this is the one setting that must not be got wrong. The old
    code added a *world* offset, and the ship visibly rolls on Z when strafing. Any
    `Lock To Target` mode would make the camera inherit that roll and the whole screen would tilt
    when the player steers.
  - Follow Offset: **`(0, 2, -6)`** to reproduce the current framing exactly.
  - Position Damping: start at **X ≈ 0.8–1.2, Y ≈ 0.4, Z ≈ 0.2**. X carries all the feel here,
    because lateral strafing is the only real motion the camera ever sees. Z damping is nearly
    inert (the ship never moves in Z) — keep it small so it does not fight the boost camera.
- `CinemachineRotationComposer` (Rotation Control) with a little damping, so the aim eases too.
  For strict parity with `transform.LookAt` first, use `CinemachineHardLookAt`, confirm it matches,
  then switch to the composer.

### 4. Switching between them — `CameraDirector.cs`

New script. Subscribes to the static `GameManager` events:

- `OnSessionStarted` → raise `CM Play Camera` priority above the menu camera (or disable the menu
  camera). The Brain blends automatically.
- Because `_startedOnce` is never reset, the title menu never returns; **do not** switch back to
  the orbit on `OnSessionEnded` unless you also want the game-over screen to orbit. That is a nice
  touch and a one-line change, but it is a design decision, not a requirement — left out by default.

`GameManager`'s events are `static` and the house style never unsubscribes. New code should still
unsubscribe in `OnDestroy` — a static event holding a destroyed MonoBehaviour survives domain
reloads in the Editor and will throw. This is new code, not a "fix" to the existing rough style.

### 5. The teleport gotcha — must handle

`ShipControls.ResetLocation()` **teleports** the ship to `_startingLocation` on
`OnSessionStarted`. With damping, a Cinemachine camera will smear across that jump. Call
`CinemachineCamera.OnTargetObjectWarped(target, positionDelta)` (or `ForceCameraPosition`) at that
moment. `CameraFollow` had no damping, so this bug does not exist today and will appear the moment
damping is added.

Related: because the road scrolls by texture offset, the follow target is **stationary** in world
space during play. Any velocity-based Cinemachine feature (look-ahead,
`CinemachinePositionComposer` lookahead, `LazyFollow`) will see zero velocity and do nothing.
Do not reach for those here.

### 6. Retire `CameraFollow`

Remove the component from `Main Camera` and delete `Assets/Scripts/CameraFollow.cs` (+ its
`.meta`). If both it and a `CinemachineBrain` write the transform in `LateUpdate`, you get
order-dependent jitter — this is the single most likely "why is it shaking" failure.

### Optional, and a strong demo of the point

The header comment complains there are "no per-state distances". A third
`CM Boost Camera` — same setup as the play camera but wider FOV and a pulled-back offset, raised
in priority while `GameManager.Instance.IsPlayerBoosting` is true — makes boosting *feel* faster
with no new movement code, purely through blending. Add
`CinemachineBasicMultiChannelPerlin` to it for shake.

---

## Files

| File | Change |
| --- | --- |
| `Packages/manifest.json` | add `com.unity.cinemachine` (via Package Manager, not by hand) |
| `Assets/Scenes/MainScene.unity` | Brain on `Main Camera`; two new camera GameObjects — **via the Editor / MCP, never by editing YAML** |
| `Assets/Scripts/MenuCameraOrbit.cs` | new |
| `Assets/Scripts/CameraDirector.cs` | new |
| `Assets/Scripts/ShipControls.cs` | add the `OnTargetObjectWarped` call in `ResetLocation()` |
| `Assets/Scripts/CameraFollow.cs` | delete (with `.meta`) |

Match this project's register, not Asteroids': no namespaces, `_camelCase` privates,
`[SerializeField] private`, sparse lowercase comments, `#region` blocks, explicit
`if (OnX != null)` event invocation. **No XML doc comments** — this project has none in gameplay
scripts, and the root `CLAUDE.md` says not to "fix" an early project to match a later one.

Note `MainScene.unity` already has one uncommitted override (MobileControls `m_IsActive: 0`);
commit or revert it first so the camera diff is readable.

---

## How to test

**Before play mode**

1. Select `CM Menu Camera` and press **Solo** in the inspector. The Game view immediately shows
   that camera's framing without entering play mode, and the Scene view draws the orbit ring and
   the composer's dead-zone guides. Do the same for `CM Play Camera` and confirm it matches the
   old shot — Solo it and check the Game view is pixel-identical to what `CameraFollow` produced.
2. In the Scene view, drag the `Player` left and right with `CM Play Camera` soloed to watch the
   damping respond live. This is the fastest tuning loop; no play mode needed.

**In play mode**

3. Enter play. The menu should orbit the ship. Confirm the road and asteroids stay frozen.
4. Press any key. Expect a smooth blend from orbit into the behind-the-ship shot over the Brain's
   default blend time — this is the money shot; if it cuts instantly, the blend is set to zero or
   one camera is being disabled rather than deprioritised.
5. **Check for the teleport smear** at exactly that moment (step 5 above). If the camera lurches,
   `OnTargetObjectWarped` is missing.
6. Steer hard left and right. The camera should lag slightly and catch up. Verify the **horizon
   stays level** — if the screen tilts when you steer, Binding Mode is wrong (should be
   `World Space`).
7. Die, then press Retry. `OnSessionStarted` fires again — confirm no smear and no double-blend.
8. Confirm the shield force-field still renders. `ForceField.shader` samples `_CameraDepthTexture`
   and does a `GrabPass`; it works today only because forward rendering with a shadow-casting
   directional light happens to generate the depth texture. It should be unaffected, but it is the
   one visual most likely to break subtly.
9. Confirm post-processing still applies — `PostProcessLayer` must still be on the Brain camera.

**Via MCP** — the `unity-skyroads` server is connected this session (unlike `unity-asteroids` /
`unity-flappybirb`, which are refusing connections):

- `editor-application-set-state` to enter/exit play mode
- `screenshot-game-view` to capture the menu orbit and the post-blend gameplay shot
- `console-get-logs` to catch null-target or missing-Brain errors
- `gameobject-component-add` / `gameobject-component-modify` to build the rig without touching
  scene YAML, per the root `CLAUDE.md` rule

**Regression surface** is genuinely small: nothing but `CameraFollow` read the camera, all UI is
Screen-Space-Overlay, and there is no World-Space canvas or `Camera.main` lookup to break.

---

## Slides coupling

The root `CLAUDE.md` notes that a change to how a project demonstrates a concept may need the
matching slide updated. Cinemachine is not currently anywhere in the slide arc (Singletons → S3,
Physics → S4, Coroutines/Audio → S5, Pooling/SOs/Persistence → S6, Mobile → S7). If this lands as
Session 8 material, `Unity101-Slides/SUBJECTS-INDEX.md` and the Session 8 deck need a matching
entry — worth deciding before implementing, since it changes how much of the "before" script
should be preserved for contrast.
