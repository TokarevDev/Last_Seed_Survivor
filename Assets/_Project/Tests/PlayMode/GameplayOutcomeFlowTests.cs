using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Bootstrap;
using Game.Core.Timing;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Enemy.Worm.Spawning;
using Game.Gameplay.Input;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Signals;
using Game.Infrastructure.Navigation;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.UI.Revive;
using Game.Presentation.UI.Rewards;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Zenject;

namespace Game.Tests.PlayMode
{
    public sealed class GameplayOutcomeFlowTests
    {
        private const float InitialTimeScale = 0.65f;

        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator Victory_DoubleAcceptReturnsToLobbyAndReleasesModalOwnership()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            SignalBus signalBus = sceneContext.Container.Resolve<SignalBus>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            WinPopupView popup = _flow.FindInActiveScene<WinPopupView>();
            Button acceptButton = GetPrivateField<Button>(popup, "_acceptButton");
            timeScale.TimeScale = InitialTimeScale;

            signalBus.Fire<WormDiedSignal>();
            yield return null;

            Assert.That(popup.IsVisible, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            acceptButton.onClick.Invoke();
            acceptButton.onClick.Invoke();
            yield return _flow.WaitForActiveScene(
                GameSceneNames.Lobby,
                "waiting for the victory popup to return to Lobby");

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Defeat_DoubleGiveUpReturnsToLobbyAndReleasesModalOwnership()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            SignalBus signalBus = sceneContext.Container.Resolve<SignalBus>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            RevivalPopupView popup = _flow.FindInActiveScene<RevivalPopupView>();
            Button giveUpButton = GetPrivateField<Button>(popup, "_giveUpButton");
            timeScale.TimeScale = InitialTimeScale;

            signalBus.Fire(new WormPathCompletedSignal(CreatePathCompletion()));
            yield return null;

            Assert.That(popup.IsVisible, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            giveUpButton.onClick.Invoke();
            giveUpButton.onClick.Invoke();
            yield return _flow.WaitForActiveScene(
                GameSceneNames.Lobby,
                "waiting for the defeat popup to return to Lobby");

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ReviveThenReward_DoubleActionsCommitOnceAndRestoreGameplay()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            SignalBus signalBus = sceneContext.Container.Resolve<SignalBus>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            WormReviveApplicationFlow reviveFlow =
                sceneContext.Container.Resolve<WormReviveApplicationFlow>();
            RewardRequestLifecycle rewardLifecycle =
                sceneContext.Container.Resolve<RewardRequestLifecycle>();
            RevivalPopupView revivePopup = _flow.FindInActiveScene<RevivalPopupView>();
            RewardPopupView rewardPopup = _flow.FindInActiveScene<RewardPopupView>();
            Button reviveButton = GetPrivateField<Button>(revivePopup, "_reviveButton");
            timeScale.TimeScale = InitialTimeScale;

            signalBus.Fire(new WormPathCompletedSignal(CreatePathCompletion()));
            yield return null;
            reviveButton.onClick.Invoke();
            reviveButton.onClick.Invoke();

            Assert.That(reviveFlow.RemainingAttempts, Is.Zero);
            yield return _flow.WaitForCondition(
                () => reviveFlow.Phase == WormRevivePhase.Running && !revivePopup.IsVisible,
                "waiting for revive rollback and popup close");

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(timeScale.TimeScale, Is.EqualTo(InitialTimeScale));

            signalBus.Fire(new WormRewardRequestedSignal(
                rewardProfile: null,
                headPathProgressNormalized: 0.8f,
                wormDestructionProgressNormalized: 0.5f));

            RewardButtonView rewardButtonView = GetFirstRewardButton(rewardPopup);
            Button rewardButton = GetPrivateField<Button>(rewardButtonView, "_button");
            yield return _flow.WaitForCondition(
                () => rewardPopup.IsVisible && rewardButton.interactable,
                "waiting for reward popup interaction");

            Assert.That(rewardLifecycle.IsActive, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            rewardButton.onClick.Invoke();
            rewardButton.onClick.Invoke();
            yield return _flow.WaitForCondition(
                () => !rewardPopup.IsVisible && !rewardLifecycle.IsActive,
                "waiting for reward selection completion");

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(timeScale.TimeScale, Is.EqualTo(InitialTimeScale));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator RestartRun_TwiceClearsModalStateAndRebuildsTheWorm()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            GameplayRunRestarter restarter =
                _flow.FindInActiveScene<GameplayRunRestarter>();
            Assert.That(restarter, Is.Not.Null);
            WormSpawnLifecycle spawnLifecycle =
                sceneContext.Container.Resolve<WormSpawnLifecycle>();
            RewardFlowController rewardFlow =
                sceneContext.Container.Resolve<RewardFlowController>();
            RewardRequestLifecycle rewardLifecycle =
                sceneContext.Container.Resolve<RewardRequestLifecycle>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            RewardPopupView rewardPopup = _flow.FindInActiveScene<RewardPopupView>();

            yield return _flow.WaitForCondition(
                () => spawnLifecycle.IsSpawned,
                "waiting for the initial worm spawn");

            Assert.That(rewardFlow.Open(), Is.True);
            Assert.That(rewardPopup.IsVisible, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);

            restarter.RestartRun();
            AssertRestartedState(
                spawnLifecycle,
                rewardLifecycle,
                rewardPopup,
                inputLock);

            restarter.RestartRun();
            AssertRestartedState(
                spawnLifecycle,
                rewardLifecycle,
                rewardPopup,
                inputLock);
            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }

        private static WormPathCompletion CreatePathCompletion()
        {
            return new WormPathCompletion(
                finalDistance: 100f,
                normalizedProgress: 1f,
                reason: WormPathCompletionReason.ReachedRailEnd);
        }

        private static RewardButtonView GetFirstRewardButton(RewardPopupView popup)
        {
            List<RewardButtonView> buttons = GetPrivateField<List<RewardButtonView>>(
                popup,
                "_buttons");
            Assert.That(buttons, Is.Not.Null.And.Not.Empty);
            Assert.That(buttons[0], Is.Not.Null);
            return buttons[0];
        }

        private static void AssertRestartedState(
            WormSpawnLifecycle spawnLifecycle,
            RewardRequestLifecycle rewardLifecycle,
            RewardPopupView rewardPopup,
            IGameplayInputLock inputLock)
        {
            Assert.That(spawnLifecycle.IsSpawned, Is.True);
            Assert.That(rewardLifecycle.IsActive, Is.False);
            Assert.That(rewardPopup.IsVisible, Is.False);
            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        private static TValue GetPrivateField<TValue>(object target, string fieldName)
        {
            Assert.That(target, Is.Not.Null);
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field '{fieldName}'.");
            return (TValue)field.GetValue(target);
        }
    }
}
