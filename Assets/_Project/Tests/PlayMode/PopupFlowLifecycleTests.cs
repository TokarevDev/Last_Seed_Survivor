using System.Collections;
using Game.Core.Timing;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Input;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Signals;
using Game.Infrastructure.Navigation;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.UI.Rewards;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace Game.Tests.PlayMode
{
    public sealed class PopupFlowLifecycleTests
    {
        private const float InitialTimeScale = 0.65f;

        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator PopupClose_RestoresOwnedInputAndTimeScale()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            PopupRoot popupRoot = _flow.FindInActiveScene<PopupRoot>();
            RevivalPopupView popup = _flow.FindInActiveScene<RevivalPopupView>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            timeScale.TimeScale = InitialTimeScale;

            popupRoot.Show(popup);

            Assert.That(popup.IsVisible, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            popup.RequestClose();
            yield return null;

            Assert.That(popup.IsVisible, Is.False);
            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(timeScale.TimeScale, Is.EqualTo(InitialTimeScale));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator RewardFlow_SceneTeardown_ClearsRequestAndRestoresModalOwnership()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            RewardFlowController rewardFlow = sceneContext.Container.Resolve<RewardFlowController>();
            RewardRequestLifecycle requestLifecycle =
                sceneContext.Container.Resolve<RewardRequestLifecycle>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            timeScale.TimeScale = InitialTimeScale;

            Assert.That(rewardFlow.Open(), Is.True);
            Assert.That(requestLifecycle.IsActive, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            yield return _flow.LoadScene(GameSceneNames.Lobby);

            Assert.That(requestLifecycle.IsActive, Is.False);
            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(InitialTimeScale));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ReviveFlow_SceneTeardown_RestoresModalOwnership()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            SignalBus signalBus = sceneContext.Container.Resolve<SignalBus>();
            RevivalPopupView popup = _flow.FindInActiveScene<RevivalPopupView>();
            IGameplayInputLock inputLock = sceneContext.Container.Resolve<IGameplayInputLock>();
            ITimeScaleController timeScale = sceneContext.Container.Resolve<ITimeScaleController>();
            timeScale.TimeScale = InitialTimeScale;
            WormPathCompletion completion = new(
                finalDistance: 100f,
                normalizedProgress: 1f,
                reason: WormPathCompletionReason.ReachedRailEnd);

            signalBus.Fire(new WormPathCompletedSignal(completion));
            yield return null;

            Assert.That(popup.IsVisible, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            yield return _flow.LoadScene(GameSceneNames.Lobby);

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(InitialTimeScale));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }
    }
}
