using System;
using System.Reflection;
using Game.Core.Timing;
using Game.Gameplay.Input;
using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class PopupRootLifecycleTests
    {
        [Test]
        public void HideActive_WhenHiddenObserverThrows_ReleasesModalOwnership()
        {
            GameObject rootObject = new("PopupRoot");
            GameObject popupObject = new("Popup");
            popupObject.SetActive(false);
            PopupRoot root = rootObject.AddComponent<PopupRoot>();
            PopupView popup = popupObject.AddComponent<PopupView>();
            GameplayInputLock inputLock = new();
            FakeTimeScaleController timeScale = new(0.65f);
            SetModalLock(root, new PopupModalLock(inputLock, timeScale, true));
            Action<PopupView> observer = _ =>
                throw new InvalidOperationException("Observer failed.");

            try
            {
                root.Show(popup);
                popup.Hidden += observer;

                Assert.Throws<InvalidOperationException>(() => root.HideActive());
                Assert.That(inputLock.IsLocked, Is.False);
                Assert.That(timeScale.TimeScale, Is.EqualTo(0.65f));
                Assert.DoesNotThrow(() => root.HideActive());
            }
            finally
            {
                popup.Hidden -= observer;
                root.ReleaseGameplayLock();
                UnityEngine.Object.DestroyImmediate(popupObject);
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void Show_WhenViewActivationThrows_RollsBackNavigationAndModalOwnership()
        {
            GameObject rootObject = new("PopupRoot");
            GameObject popupObject = new("ThrowingPopup");
            popupObject.SetActive(false);
            PopupRoot root = rootObject.AddComponent<PopupRoot>();
            ThrowingOnShownPopupView popup =
                popupObject.AddComponent<ThrowingOnShownPopupView>();
            GameplayInputLock inputLock = new();
            FakeTimeScaleController timeScale = new(0.75f);
            SetModalLock(root, new PopupModalLock(inputLock, timeScale, true));

            try
            {
                Assert.Throws<InvalidOperationException>(() => root.Show(popup));
                Assert.That(inputLock.IsLocked, Is.False);
                Assert.That(timeScale.TimeScale, Is.EqualTo(0.75f));
                Assert.That(popup.IsVisible, Is.False);
                Assert.DoesNotThrow(() => root.HideActive());
            }
            finally
            {
                root.ReleaseGameplayLock();
                UnityEngine.Object.DestroyImmediate(popupObject);
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
        }

        private static void SetModalLock(PopupRoot root, PopupModalLock modalLock)
        {
            typeof(PopupRoot)
                .GetField("_modalLock", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(root, modalLock);
        }

        private sealed class FakeTimeScaleController : ITimeScaleController
        {
            public FakeTimeScaleController(float timeScale)
            {
                TimeScale = timeScale;
            }

            public float TimeScale { get; set; }
        }
    }

    public sealed class ThrowingOnShownPopupView : PopupView
    {
        protected override void OnShown()
        {
            throw new InvalidOperationException("Popup failed to show.");
        }
    }
}
