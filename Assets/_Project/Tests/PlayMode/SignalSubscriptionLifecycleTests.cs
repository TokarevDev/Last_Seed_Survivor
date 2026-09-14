using System.Reflection;
using Game.Gameplay.Combat;
using Game.Gameplay.Signals;
using Game.Presentation.UI.Combat;
using Game.Presentation.UI.Rewards;
using Game.Presentation.Worm;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Game.Tests.PlayMode
{
    public sealed class SignalSubscriptionLifecycleTests
    {
        [Test]
        public void Initialize_WhenSecondSubscriptionFails_RollsBackReviveSubscription()
        {
            DiContainer container = new();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<WormReviveGrantedSignal>();
            SignalBus signalBus = container.Resolve<SignalBus>();
            RewardSessionController controller = new(null, signalBus);

            Assert.Throws<ZenjectException>(controller.Initialize);

            signalBus.Fire<WormReviveGrantedSignal>();

            Assert.That(GetHasRevivedThisRun(controller), Is.False);
        }

        [Test]
        public void WormPressureConstruct_WhenSignalSubscriptionFails_RollsBackSessionSubscription()
        {
            DiContainer container = new();
            SignalBusInstaller.Install(container);
            SignalBus signalBus = container.Resolve<SignalBus>();
            CombatSessionState sessionState = new();
            GameObject owner = new("WormPressureDirector");
            WormPressureDirector director = owner.AddComponent<WormPressureDirector>();

            try
            {
                Assert.Throws<ZenjectException>(() =>
                    director.Construct(signalBus, sessionState));

                Assert.That(GetSessionSubscriberCount(sessionState), Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void WormSpawnerConstruct_WhenSecondSubscriptionFails_RollsBackFirstSignal()
        {
            SignalBus signalBus = CreateSignalBusWith<WormReviveGrantedSignal>();
            GameObject owner = new("WormSpawner");
            WormSpawner spawner = owner.AddComponent<WormSpawner>();

            try
            {
                Assert.Throws<ZenjectException>(() =>
                    spawner.Construct(signalBus, null, null, null));

                Assert.DoesNotThrow(() => signalBus.Fire<WormReviveGrantedSignal>());
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void VictoryControllerConstruct_WhenSecondSubscriptionFails_RollsBackFirstSignal()
        {
            SignalBus signalBus = CreateSignalBusWith<WormDiedSignal>();
            GameObject owner = new("WormVictoryPopupController");
            WormVictoryPopupController controller =
                owner.AddComponent<WormVictoryPopupController>();

            try
            {
                Assert.Throws<ZenjectException>(() =>
                    controller.Construct(signalBus, null));

                Assert.DoesNotThrow(() => signalBus.Fire<WormDiedSignal>());
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        private static bool GetHasRevivedThisRun(RewardSessionController controller)
        {
            FieldInfo field = typeof(RewardSessionController).GetField(
                "_hasRevivedThisRun",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (bool)field.GetValue(controller);
        }

        private static int GetSessionSubscriberCount(CombatSessionState sessionState)
        {
            FieldInfo field = typeof(CombatSessionState).GetField(
                "ShootingEnabledChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return ((System.Delegate)field.GetValue(sessionState))
                ?.GetInvocationList().Length ?? 0;
        }

        private static SignalBus CreateSignalBusWith<TSignal>()
        {
            DiContainer container = new();
            SignalBusInstaller.Install(container);
            container.DeclareSignal<TSignal>();
            return container.Resolve<SignalBus>();
        }
    }
}
