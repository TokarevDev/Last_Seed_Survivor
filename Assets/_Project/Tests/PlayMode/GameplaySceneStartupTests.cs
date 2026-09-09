using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

using Game.Bootstrap.GameplayLoop;
using Game.Bootstrap.Scenes.Game;
using Game.Core.Collections;
using Game.Core.Pooling;
using Game.Core.Random;
using Game.Core.Timing;
using Game.Gameplay.Combat;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Gameplay.Enemy.Worm.Spawning;
using Game.Gameplay.Player;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Gameplay.Signals;
using Game.Infrastructure.Advertising;
using Game.Infrastructure.Input;
using Game.Infrastructure.Navigation;
using Game.Presentation.Signals;
using Game.Presentation.UI.Rewards;
using Game.Presentation.Worm;

namespace Game.Tests.PlayMode
{
    public sealed class GameplaySceneStartupTests
    {
        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator GameplayScene_WhenLoadedDirectly_ResolvesRequiredRuntimeDependencies()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            Scene gameplayScene = SceneManager.GetActiveScene();
            SceneContext sceneContext = _flow.GetActiveSceneContext();

            Assert.That(gameplayScene.name, Is.EqualTo(GameSceneNames.Gameplay));
            Assert.That(ProjectContext.HasInstance, Is.True);
            Assert.That(sceneContext, Is.Not.Null);
            Assert.That(_flow.FindInActiveScene<PlayerInputSnapshotProvider>(), Is.Not.Null);
            Assert.That(_flow.FindInActiveScene<GameplayUpdateDriver>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRandomSource>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<ITimeScaleController>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IGameTimeProvider>(), Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<WeaponRuntimeStatsSignalPublisher>(),
                Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<IWeaponRuntimeStatsPublisher>(),
                Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<IWeaponAttackCyclePublisher>(),
                Is.Not.Null);
            Assert.That(
                sceneContext.Container.Resolve<WeaponAttackCycleSignalPublisher>(),
                Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRewardedAdService>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardFlowController>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardSessionController>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardAttemptState>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardRequestQueue>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardRequestLifecycle>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardRequestCoordinator>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardPopupGateway>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRewardChoiceApplier>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRewardRuntimeContextProvider>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRewardChoiceRoller>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardChoiceRollService>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<IRewardChoiceRollService>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardBatchApplyService>(), Is.Not.Null);
            Assert.That(sceneContext.Container.Resolve<RewardGrantedActionService>(), Is.Not.Null);

            AssertPlayerServices(sceneContext.Container);
            AssertWormServices(sceneContext.Container);
            AssertCombatSessionSignals(sceneContext.Container);
            AssertWormBurstSignal(sceneContext.Container);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertPlayerServices(DiContainer sceneContainer)
        {
            Assert.That(sceneContainer.Resolve<PlayerMovementController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<PlayerWeaponController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<AcaciaThornRuntimeInitializer>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<ProjectileWeapon>(), Is.Not.Null);
        }

        private static void AssertWormServices(DiContainer sceneContainer)
        {
            Assert.That(sceneContainer.Resolve<WormAdaptiveHpController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormCombatBurstController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormRailTargetResolver>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSegmentChainPresenter>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSegmentTransformPresenter>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSegmentVisualChainPresenter>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormReviveMotionCalculator>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormReviveAnimationController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormReviveVisualScaler>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<OrderedReferenceSet<WormSegment>>(), Is.Not.Null);
            Assert.That(
                sceneContainer.Resolve<WormSectionRollbackState<WormSegment>>(),
                Is.Not.Null);
            Assert.That(
                sceneContainer.Resolve<WormSectionRollbackMotionController<WormSegment>>(),
                Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSpawnSettings>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSegmentPool>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormFactory>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormCombatBurstSignalPublisher>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormPathCompletedSignalPublisher>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<IWormCombatEventPublisher>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormCocoonShakeClock>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormSpawnLifecycle>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormForwardMotionController>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormFrameSimulation>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormFaceBurstPresenter>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<IWormFaceBurstPresentation>(), Is.Not.Null);
            Assert.That(sceneContainer.Resolve<WormPressureDirector>(), Is.Not.Null);
        }

        private static void AssertWormBurstSignal(DiContainer sceneContainer)
        {
            SignalBus signalBus = sceneContainer.Resolve<SignalBus>();
            WormCombatBurstController burstController =
                sceneContainer.Resolve<WormCombatBurstController>();
            WormCombatBurstSignalPublisher publisher =
                sceneContainer.Resolve<WormCombatBurstSignalPublisher>();
            bool receivedActiveState = false;
            Action<WormCombatBurstStateChangedSignal> handler = signal =>
                receivedActiveState = signal.IsActive;
            WormCombatBurstSettings settings = new(
                enabled: true,
                burstSpeed: 3f,
                interval: 0.1f,
                duration: 1f,
                slowdownDuration: 0.2f);

            signalBus.Subscribe(handler);
            burstController.Reset(baseSpeed: 1f);
            publisher.PublishIfChanged(burstController.IsActive);
            burstController.ResolveForwardSpeed(0.1f, 1f, 1f, false, true, settings);
            publisher.PublishIfChanged(burstController.IsActive);
            burstController.ResolveForwardSpeed(0.1f, 1f, 1f, false, true, settings);
            publisher.PublishIfChanged(burstController.IsActive);

            Assert.That(receivedActiveState, Is.True);

            signalBus.Unsubscribe(handler);
            burstController.Reset(baseSpeed: 1f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }

        private static void AssertCombatSessionSignals(DiContainer sceneContainer)
        {
            ICombatSessionState combatSessionState = sceneContainer.Resolve<ICombatSessionState>();
            bool receivedShootingEnabledSignal = false;
            Action<bool> signalHandler = isEnabled =>
                receivedShootingEnabledSignal = isEnabled;

            combatSessionState.ShootingEnabledChanged += signalHandler;
            combatSessionState.SetShootingEnabled(true);

            Assert.That(combatSessionState.IsShootingEnabled, Is.True);
            Assert.That(receivedShootingEnabledSignal, Is.True);

            combatSessionState.ShootingEnabledChanged -= signalHandler;
            combatSessionState.Reset();
        }
    }
}
