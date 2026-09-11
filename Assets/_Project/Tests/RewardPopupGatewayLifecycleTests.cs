using System;
using System.Reflection;
using Game.Gameplay.Rewards.Runtime;
using Game.Infrastructure.Advertising;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.UI.Rewards;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class RewardPopupGatewayLifecycleTests
    {
        private GameObject _popupObject;
        private GameObject _rootObject;

        [TearDown]
        public void TearDown()
        {
            if (_popupObject != null)
                UnityEngine.Object.DestroyImmediate(_popupObject);

            if (_rootObject != null)
                UnityEngine.Object.DestroyImmediate(_rootObject);
        }

        [Test]
        public void Dispose_RemovesViewSubscriptions_AndIsIdempotent()
        {
            _popupObject = new GameObject("RewardPopup");
            _popupObject.SetActive(false);
            LogAssert.Expect(LogType.Error, "RewardPopupView requires an animation config.");
            LogAssert.Expect(LogType.Error, "RewardPopupView requires an action presentation config.");
            RewardPopupView popup = _popupObject.AddComponent<RewardPopupView>();

            _rootObject = new GameObject("PopupRoot");
            _rootObject.SetActive(false);
            PopupRoot popupRoot = _rootObject.AddComponent<PopupRoot>();

            RewardAttemptState attempts = new(new RewardFlowSettings(0, 0, 0));
            RewardPopupGateway gateway = new(
                popup,
                popupRoot,
                new RewardRequestLifecycle(),
                new RewardPopupStateFactory(
                    attempts,
                    new RewardedAdOperation(new DisabledRewardedAdService())));

            Assert.That(GetSubscriberCount(popup, "Selected"), Is.EqualTo(1));
            Assert.That(GetSubscriberCount(popup, "RerollRequested"), Is.EqualTo(1));
            Assert.That(GetSubscriberCount(popup, "AdRerollRequested"), Is.EqualTo(1));
            Assert.That(GetSubscriberCount(popup, "TakeAllRequested"), Is.EqualTo(1));

            gateway.Dispose();
            gateway.Dispose();

            Assert.That(GetSubscriberCount(popup, "Selected"), Is.Zero);
            Assert.That(GetSubscriberCount(popup, "RerollRequested"), Is.Zero);
            Assert.That(GetSubscriberCount(popup, "AdRerollRequested"), Is.Zero);
            Assert.That(GetSubscriberCount(popup, "TakeAllRequested"), Is.Zero);
        }

        private static int GetSubscriberCount(RewardPopupView popup, string eventName)
        {
            FieldInfo eventField = typeof(RewardPopupView).GetField(
                eventName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Delegate subscribers = eventField?.GetValue(popup) as Delegate;
            return subscribers?.GetInvocationList().Length ?? 0;
        }
    }
}
