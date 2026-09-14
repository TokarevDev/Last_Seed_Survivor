using System;
using System.Reflection;
using Game.Bootstrap;
using Game.Core.Timing;
using Game.Gameplay.Input;
using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.PlayMode
{
    public sealed class GameplayRunRestarterLifecycleTests
    {
        [Test]
        public void RestartRun_WhenPopupCleanupThrows_ReleasesReentrancyGuard()
        {
            GameObject restarterObject = new("GameplayRunRestarter");
            GameObject popupRootObject = new("PopupRoot");
            GameObject popupObject = new("Popup");
            popupObject.SetActive(false);
            GameplayRunRestarter restarter =
                restarterObject.AddComponent<GameplayRunRestarter>();
            PopupRoot popupRoot = popupRootObject.AddComponent<PopupRoot>();
            PopupView popup = popupObject.AddComponent<PopupView>();
            SetPrivateField(
                popupRoot,
                "_modalLock",
                new PopupModalLock(
                    new GameplayInputLock(),
                    new FakeTimeScaleController(),
                    pauseTime: false));
            Action<PopupView> observer = _ =>
                throw new InvalidOperationException("Popup cleanup failed.");
            SetPrivateField(restarter, "_popupRoot", popupRoot);

            try
            {
                popupRoot.Show(popup);
                popup.Hidden += observer;

                Assert.Throws<InvalidOperationException>(restarter.RestartRun);
                Assert.That(GetIsRestarting(restarter), Is.False);
            }
            finally
            {
                popup.Hidden -= observer;
                UnityEngine.Object.DestroyImmediate(popupObject);
                UnityEngine.Object.DestroyImmediate(popupRootObject);
                UnityEngine.Object.DestroyImmediate(restarterObject);
            }
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static bool GetIsRestarting(GameplayRunRestarter restarter)
        {
            FieldInfo field = typeof(GameplayRunRestarter).GetField(
                "_isRestarting",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (bool)field.GetValue(restarter);
        }

        private sealed class FakeTimeScaleController : ITimeScaleController
        {
            public float TimeScale { get; set; } = 1f;
        }
    }
}
