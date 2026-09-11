using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Infrastructure.Advertising;
using Game.Presentation.UI.Revive;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class WormReviveFlowControllerLifecycleTests
    {
        private GameObject _controllerObject;

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
                UnityEngine.Object.DestroyImmediate(_controllerObject);
        }

        [Test]
        public void ResetForNewRun_CancelsPendingRewardedAdAndInvalidatesLateCallback()
        {
            _controllerObject = new GameObject("WormReviveFlowController");
            _controllerObject.SetActive(false);
            WormReviveFlowController controller =
                _controllerObject.AddComponent<WormReviveFlowController>();
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            WormReviveApplicationFlow applicationFlow = new(operation);
            applicationFlow.InitializeSession(1);
            SetApplicationFlow(controller, applicationFlow);
            _controllerObject.SetActive(true);
            bool wasCalled = false;
            Assert.That(applicationFlow.TryBeginFailure(), Is.True);
            bool started = applicationFlow.TryBeginRewardedRevive(
                _ => wasCalled = true,
                null);

            Assert.That(started, Is.True);
            controller.ResetForNewRun();
            adService.Complete(true);

            Assert.That(operation.IsPending, Is.False);
            Assert.That(wasCalled, Is.False);
        }

        private static void SetApplicationFlow(
            WormReviveFlowController controller,
            WormReviveApplicationFlow applicationFlow)
        {
            FieldInfo field = typeof(WormReviveFlowController).GetField(
                "_applicationFlow",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(controller, applicationFlow);
        }

        private sealed class DelayedRewardedAdService : IRewardedAdService
        {
            private readonly List<Action<bool>> _callbacks = new();

            public bool IsReady => true;

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                _callbacks.Add(onCompleted);
            }

            public void Complete(bool rewardGranted)
            {
                _callbacks[0](rewardGranted);
            }
        }
    }
}
