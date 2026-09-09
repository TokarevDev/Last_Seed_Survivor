# Last Seed Survivor — Architecture Audit and Next Roadmap

Audit date: 2026-09-04  
Baseline commit: `fed087b`

## Goal

The game must remain a runnable, testable C# program whose Unity layer supplies input,
time, physics, scene references, rendering, audio, and platform integrations. Unity
components are adapters and views; they do not own rules, application workflows, or
dependency selection.

This roadmap continues the completed foundation without a wholesale rewrite. Each
iteration must leave `master` playable and is committed only after its full gate passes.

## Audit scope and method

The audit covered all production C# files under `Assets/_Project`, assembly dependency
direction, installers, runtime searches, Unity lifecycle methods, events/signals,
pooling, object creation, frame loops, static state, and the automated test inventory.

Generated `Infrastructure/Input/Input Actions.cs` is excluded from size-based refactoring.
Editor-only tools are evaluated separately from runtime code and may legitimately be
larger when they do not leak responsibilities into a runtime type.

Current automated baseline:

- 51 EditMode tests.
- 1 PlayMode direct-Game-scene startup test.
- Zenject validation for all 3 enabled build scenes.

## Audit findings

### P0 — architectural boundaries are documented but not enforced

- `LastSeed.Gameplay` references Infrastructure, Zenject, DOTween, and TextMeshPro and
  contains many MonoBehaviours and presentation classes. A clean C# gameplay core cannot
  currently compile without Unity-facing dependencies.
- All asmdefs have `noEngineReferences: false`; there is no assembly boundary proving
  that domain code is engine-independent.
- Desired dependency direction remains `Core <- Gameplay.Domain <- Application`, with
  Unity/Zenject implementations in adapter assemblies and Bootstrap as the only
  composition layer.

### P0 — hidden composition roots remain

- `RewardInstaller` is a MonoBehaviour that constructs `RewardRollService`,
  `RewardApplyService`, and `RewardFlowController` in `Awake`.
- `AcaciaThornWeapon` constructs and owns its concrete projectile pool.
- Some Unity behaviours accept part of their graph through Zenject and another part
  through ad-hoc `Init`, obscuring required dependencies and valid lifecycle states.

### P0 — weapon implementations duplicate the same application workflow

- `ProjectileWeapon` and `AcaciaThornWeapon` both own cooldown/salvo state, progression
  mutations, runtime-stat publication, damage calculation, firing, and pool dispatch.
- `WeaponRuntimeState` is 411 lines and combines progression storage, limits, mutations,
  derived values, modifier ownership, reset, and numeric clamping.
- Adding a third weapon would require copying orchestration rather than implementing a
  narrow firing/spawn strategy.

### P1 — `RailPath` is a runtime/editor God Object

`RailPath` (590 lines) owns serialized authoring data, legacy waypoint migration,
world-space conversion, corner smoothing, distance-table construction, sampling,
nearest-distance lookup, cache invalidation, editor mutation API, and gizmo drawing.
The sampling algorithm is coupled to Transform and cannot be tested independently.

### P1 — worm orchestration is improved but unfinished

- `WormController` is down to 503 lines but still owns serialized configuration,
  lifecycle reset, Unity update dispatch, rail progress, path completion, section
  rollback coordination, revive entry, and presentation layout creation.
- `WormSegment` still combines pooled entity lifecycle, cocoon state, damage binding,
  renderer/view validation, and per-frame sorting updates.
- `WormSegmentChainPresenter` and `WormSegmentVisualRig` are cohesive presentation code,
  but should be split where calculation can be made engine-independent and tested.

### P1 — reward application flow and UI state are mixed

- `RewardFlowController` owns queueing, session attempts, ad callbacks, rolling,
  application, popup state construction, and direct View event subscriptions.
- `RewardPopupView`, `RewardPopupAnimator`, `RewardButtonView`, and
  `RewardPopupActionControls` contain overlapping binding, animation, interaction, and
  formatting responsibilities.
- Reward selection uses `UnityEngine.Random`, preventing deterministic application tests.

### P1 — events are only partially reactive

- Cross-system notifications correctly use typed SignalBus payloads in many paths.
- Domain events such as `WormSectionHealth.Changed/Destroyed` carry no transition data;
  subscribers must re-read mutable state.
- `WormSection` republishes model events using the mutable section object.
- `RewardPopupView` exposes four separate imperative events rather than one typed user
  intent stream. Local View events are acceptable, but their payloads and ownership must
  be explicit and converted to application commands at one boundary.

### P1 — pooled lifecycle is not yet universal

- Projectiles, worm segments, and damage popups use the shared pool foundation.
- `WormSectionHpPresenter` still instantiates and destroys HP views per lifecycle.
- `AcaciaThornProjectilePool`, `ProjectilePool`, and `WormSegmentPool` share the generic
  base algorithm but still expose different high-level ownership APIs.
- High-level weapon code still knows concrete pool types.

### P2 — UI requires an application-state boundary

- `PopupRoot` combines registry construction, navigation/stack rules, input locking,
  transitions, and reactive requests.
- Revival and victory popup controllers mix application flow with view/platform calls.
- Large animation classes are not automatically problematic, but animation timelines,
  cached visual references, content binding, and user intent must not live in one type.

### P2 — lifecycle and performance hardening

- Runtime `FindObjectOfType`/`GameObject.Find` usage was not found.
- Remaining `GetComponent*` calls are mostly initialization, validation, collision
  boundary, or editor-safe discovery; each must be cached and documented, not removed
  mechanically.
- DOTween owners need a uniform kill/restore contract on disable/destroy and pool return.
- Shared mutable formatter buffers in `RewardTextFormatter` are non-reentrant and should
  become instance-owned or explicitly single-threaded presentation services.
- Static shared material ownership in legendary lightning needs explicit teardown or a
  documented application-lifetime owner.

### P2 — automated coverage is too narrow for current flows

- Pure worm movement and reward policies have useful unit coverage.
- The only PlayMode test primarily validates startup and DI resolution.
- There are no automated flow tests for reward queue/reroll/take-all, revive completion,
  repeated worm spawn/clear, weapon salvo transitions, pooled collision lifecycle,
  popup stack/input locks, or Bootstrap -> Lobby -> Game -> Lobby.

## Target domain map and ownership

```text
Core
  primitives, result types, clock/random contracts, lifecycle contracts

Gameplay.Domain
  worm health/movement state, weapon progression/firing state, rewards policies
  no Unity, Zenject, UI, advertising, scene, Transform, Time, or SignalBus references

Gameplay.Application
  use cases and ordered gameplay stages
  depends on Domain contracts; publishes typed application events

UnityAdapters
  MonoBehaviours, physics/collision, Transform/rail views, ScriptableObject adapters,
  pooled prefab factories, Unity clock/random adapters

Presentation
  ViewModels/presenters, Views, animation drivers, UI intent adapters

Infrastructure
  input, navigation, advertising, persistence, platform SDK implementations

Bootstrap
  ProjectContext/SceneContext installers and bindings only
```

## Required gameplay lifecycle order

One physical Unity update boundary invokes named stages:

```text
CaptureInput
-> AdvanceSession
-> AdvancePlayer
-> AdvanceWeapons
-> AdvanceWorm
-> ResolveGameplayEvents
-> PresentWorld
-> PresentUI
```

Only stages with a real ordering dependency belong in the coordinator. Independent
Unity animation components may retain `Update`/`LateUpdate` when they are pure views.

## Execution roadmap

### Stage 1 — enforce a clean C# seam

1. Add clock and random contracts (`IGameClock`, `IRandomSource`) in Core.
2. Add Unity adapters bound in Bootstrap; preserve current RNG sequence during migration.
3. Create a `LastSeed.Gameplay.Domain` asmdef with `noEngineReferences: true`.
4. Move already-pure models incrementally: movement snapshots/results, section health,
   rollback state, reward selection primitives, and pool membership logic.
5. Keep ScriptableObjects as configuration adapters that build immutable domain settings.

Done when the domain assembly compiles without Unity/Zenject and its tests reference only
that assembly plus NUnit.

### Stage 2 — reward composition and deterministic application flow

1. Replace `RewardInstaller.Awake` construction with `RewardsInstaller` bindings.
2. Split `RewardFlowController` into:
   - `RewardSessionState` — attempt counters/current request;
   - `RewardRequestQueue` — ordered requests;
   - `RewardChoiceRollUseCase` — roll/guarantee orchestration;
   - `RewardAdUseCase` — rewarded action state and result handling;
   - `RewardSelectionUseCase` — apply one/all choices;
   - `RewardFlowPresenter` — maps application state to the ViewModel.
3. Introduce `IRewardPopupView` and immutable `RewardPopupViewModel`.
4. Inject `IRandomSource` into rarity, assist, ad, and weighted selection.
5. Keep advertising optional via explicit no-op/disabled implementation.

Playtest: normal reward, free reroll, failed/successful ad reroll, take-all, queued cocoon
requests, scene restart while ad callback is pending.

### Stage 3 — common weapon runtime and typed spawn boundary

1. Completed: `WeaponProgressionState`, `WeaponProgressionLimits`, and the shared derived
   stat calculator are extracted into Core and used by both weapons.
2. Completed: both weapons use the common `WeaponFireCycle` state machine for
   cooldown, optional preparation, salvo transitions, cancellation, and reset.
3. Completed: shot-pattern strategy, shared damage calculation, type-specific request
   factories, and the narrow `IProjectileSpawnSink<TRequest>` boundary are separated.
4. Completed: both weapon MonoBehaviours are thin adapters over shared fire-cycle,
   progression, derived-stat, request-factory, spawn-sink, and mutation-commit boundaries.
5. Completed: both projectile pools are owned by Bootstrap composition; weapon views
   consume only typed spawn sinks and never construct or reference concrete pools.
6. Completed: each committed weapon mutation publishes one immutable runtime-stat
   snapshot containing source, activation state, derived values, and progression values.

Playtest: initial fire, delayed animation release, salvo, critical hit, reset, unlock,
both weapon types active, extreme configured limits, and pool exhaustion/rollback.

### Stage 4 — rail domain and adapter split

1. Completed: immutable `RailPathDefinition` owns validated authoring settings and a
   defensive copy of local control points; `RailPath` adapts it to world-space caches.
2. Completed: smoothing, distance-table construction, sampling, and nearest-point
   queries live in focused stateless calculation classes.
3. Completed: the serialized `RailPath` MonoBehaviour keeps its Unity script identity
   and acts as the Transform/cache-invalidating adapter; immutable `BakedRailPath`
   owns runtime sampling, nearest-distance, and control-point-progress queries exposed
   through `IWormRailPath`.
4. Completed: legacy waypoint/child import and all point-editing operations live in
   `RailPathSerializedData` and `RailPathEditor` inside the Editor-only assembly.
5. Completed: selected-path gizmos are rendered by the Editor-only
   `RailPathGizmoDrawer`; the runtime component contains no editor drawing code.
6. Completed: control-point distances are calculated once while baking and stored in
   `BakedRailPath`; progress queries use binary search without repeated nearest-sample
   scans.

Playtest: linear/smoothed paths, short/duplicate points, transformed path object, catch-up,
burst-disable point, revive target, and editor point migration.

### Stage 5 — finish worm application boundary

1. Completed: the existing `WormMovementConfig` ScriptableObject retains its serialized
   asset identity and creates immutable `WormMovementRuntimeConfig`,
   `WormPresentationConfig`, and `WormReviveConfig` snapshots once at composition;
   `WormController` does not read mutable balance assets during its runtime loop.
2. Completed: `WormLifecycleController` owns symmetric init/clear cancellation,
   collection replacement, cache reset, rollback reset, and movement-state reset;
   `WormMovementCoordinator` owns the explicit forward/section-rollback/revive state
   priority while `WormFrameSimulation` preserves movement -> render -> completion order.
3. Completed: `WormController.Tick` returns an optional immutable
   `WormPathCompletion` containing final distance, normalized progress, and reason;
   the presentation adapter publishes it once as `WormPathCompletedSignal`.
4. Completed: `WormSegment` is the pooled entity adapter and delegates damage binding,
   cocoon presentation, pooled visibility/liveness, and visual-chain operations to
   focused collaborators; required hierarchy discovery is cached once during `Awake`.
5. Completed: chain ranges, spacing, distances, positions, and look angles are calculated
   by `WormSegmentPoseCalculator`; presenters retain only cached state and Transform/view
   application.
6. Completed: `WormSectionHpViewPool` wraps `ObjectPool<WormSectionHpView>`, while
   `WormSectionHpPresenter` owns a symmetric section-to-view dictionary and rolls back
   subscriptions and rented views on partial binding failure.

Playtest: spawn rollback failure, repeated reset, multiple simultaneous destroyed gaps,
revive during pause, path completion, death, and all cocoon variants.

### Stage 6 — payload-first domain events

1. Completed: immutable `WormSectionHealthChanged`, `WormSectionDestroyed`,
   `RewardUserIntent`, and weapon runtime/progression snapshots carry typed state across
   their boundaries; reward UI actions are translated into one intent stream.
2. Completed: `HealthChange` captures previous/current/max HP and applied damage before
   section reset, and `WormSectionDestroyed` retains section identity plus that final
   snapshot after pooled segments are released or the section is reused.
3. Completed: section health/destruction events stay within aggregate and presentation
   ownership; `WormCombatSignalPublisher` translates only high-level combat requests
   and progress into scene SignalBus payloads.
4. Completed: completion, combat-burst, damage, reward, and destruction-progress flows
   each have one adapter publisher; obsolete per-consumer publishers/subscriptions are
   absent after migration.
5. Completed: aggregate observer removal is covered across reset/damage, reward intent
   gateway disposal verifies all view subscriptions are removed idempotently, and
   PlayMode scene-container tests exercise teardown without retained signal handlers.

Playtest: damage/reward/death ordering, pooled reuse, scene reload, and duplicate-event
protection.

### Stage 7 — popup state and MVVM

1. Completed: `PopupRegistry`, `QueuedActivationState<PopupView>`, and `PopupModalLock`
   own registration, queued navigation, and input/time-scale ownership respectively;
   `PopupRoot` remains the serialized scene view host and SignalBus adapter.
2. In progress: reward state and `RevivalPopupViewModel` provide immutable rendering
   boundaries; add equivalent focused models only where victory, HUD, lobby, and
   navigation currently pass mutable or fragmented state.
3. In progress: revive now exposes one `RevivalPopupIntent` stream and renders immutable
   state; migrate remaining views at their application boundaries without adding proxy
   abstractions around already-typed APIs.
4. Completed: `RewardButtonContentPresenter` owns content/style binding and
   `RewardButtonAnimator` owns cached RectTransform/icon state plus tween creation;
   `RewardButtonView` remains the serialized input/view adapter.
5. Completed: `RewardPopupAnimator`, `RewardPopupAnimatedLayout`, and
   `RewardPopupRefreshAnimationBuilder` own timeline composition, while
   `RewardPopupAudioPlayer` isolates concrete clip playback.
6. Completed: reward buttons remove only their owned click callback, cancel and restore
   tween state on disable, and clear bound data/delegates. `PopupScaleFadeAnimator` gives
   revive and victory popups one explicit `PlayShow`/`PlayHide`/`CancelAndRestore`
   lifecycle instead of duplicated tween ownership in each view.

Playtest: rapid clicks, close during transition, nested popup request, timescale zero,
resolution/aspect changes, navigation, and input lock restoration.

### Stage 8 — pooling and identity registries

1. Define a common pooled lifecycle contract and typed `EntityLease<T>`/owner mapping.
2. Migrate remaining HP views and repeated VFX where profiling shows churn.
3. Map damageable entity/view/lifecycle owner in one registry; consumers must not scan
   concrete subtype pools.
4. Verify transactional rent -> initialize -> register -> activate and reverse rollback.
5. Add stress tests for duplicate return, partial initialization failure, scene teardown,
   and stale identity removal.

### Stage 9 — async/platform lifecycle

1. Add cancellation-bound application operations before introducing more async work.
2. Completed: shared `RewardedAdOperation` invalidates late or superseded callbacks;
   reward and revive flows cancel it at their owning lifecycle boundary.
3. Add UniTask only where it improves scene loading, popup transitions, prewarming, or
   platform calls; every operation receives an owner lifetime token.
4. Add persistence/analytics as optional interfaces with no-op implementations.

### Stage 10 — test, profiling, and release hardening

1. Add deterministic domain tests for reward distributions and weapon fire cycles.
2. Add PlayMode scenarios for reward, revive, spawn/reset, pooling, and popup stack.
3. Add Bootstrap -> Lobby -> Game -> Lobby integration coverage.
4. Add allocation budgets for gameplay update, weapon fire, worm movement, and popup idle.
5. Profile CPU, GC, pool growth, retained Unity objects, tween counts, and scene unload.
6. Add build-time validation for required references, configs, signal declarations, and
   duplicate stable keys.

## Gate required for every implementation stage

1. Inspect working tree and preserve unrelated user changes.
2. Add/adjust focused EditMode tests for extracted pure logic.
3. Compile Unity scripts without C# errors or new warnings.
4. Validate all enabled build scenes through Zenject.
5. Run the complete EditMode suite.
6. Run the direct-Game PlayMode suite plus the stage-specific scenario.
7. Inspect logs for unexpected exceptions, missing references, and leaked subscriptions.
8. Run `git diff --check` and verify only intended files/assets changed.
9. Commit the stage independently and push only after every gate is green.

Visual, platform-SDK, or timing-sensitive behavior additionally requires a manual Editor
playtest and Profiler check; a DI smoke test alone is not sufficient.

## Explicit non-goals

- Do not split classes solely to reach an arbitrary line count.
- Do not refactor generated Input System code.
- Do not create empty installers, repositories, factories, or interfaces without a
  current second implementation/test seam/ownership reason.
- Do not move all MonoBehaviours out of Gameplay mechanically; move rules first and keep
  thin Unity adapters close to their domain until assembly boundaries are stable.
- Do not introduce ECS, Addressables, UniTask, or a universal event bus before profiling
  or a concrete lifecycle need justifies them.

## Immediate next slice

Start with Stage 1 random/clock seams and Stage 2 reward composition together only as far
as needed to remove `RewardInstaller` as a hidden composition root. This creates a small,
testable vertical slice: deterministic reward domain -> application use case -> popup
adapter -> Zenject composition, without touching unrelated UI animation assets.
