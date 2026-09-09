using System.Reflection;
using Game.Presentation.UI.Rewards;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    public sealed class RewardButtonViewLifecycleTests
    {
        [Test]
        public void Disable_PreservesListenersOwnedByOtherSystems()
        {
            GameObject owner = new("RewardButton", typeof(RectTransform));
            owner.SetActive(false);

            try
            {
                Button button = owner.AddComponent<Button>();
                RewardButtonView view = owner.AddComponent<RewardButtonView>();
                SetButton(view, button);
                int externalClickCount = 0;
                button.onClick.AddListener(() => externalClickCount++);

                InvokeOnDisable(view);
                button.onClick.Invoke();

                Assert.That(externalClickCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        private static void SetButton(RewardButtonView view, Button button)
        {
            FieldInfo field = typeof(RewardButtonView).GetField(
                "_button",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(view, button);
        }

        private static void InvokeOnDisable(RewardButtonView view)
        {
            MethodInfo method = typeof(RewardButtonView).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(view, null);
        }
    }
}
