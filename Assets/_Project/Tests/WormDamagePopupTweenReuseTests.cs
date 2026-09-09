using System.Reflection;
using Game.Core.Combat;
using Game.Gameplay.Signals;
using Game.Presentation.Worm;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace Game.Tests
{
    public sealed class WormDamagePopupTweenReuseTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void Show_WithSameModeAndScale_ReusesSequence()
        {
            GameObject owner = new("DamagePopup");
            owner.SetActive(false);
            TMP_Text text = owner.AddComponent<TextMeshPro>();
            WormDamagePopupView view = owner.AddComponent<WormDamagePopupView>();
            SetPrivateField(view, "_text", text);
            owner.SetActive(true);
            DamageViewRequest request = new(
                10,
                Vector3.zero,
                DamageKind.Normal,
                false);

            try
            {
                view.Show(
                    request,
                    WormDamagePopupView.AnimationMode.Normal,
                    1f,
                    null);
                object initialSequence = GetSequence(view);

                view.Show(
                    request,
                    WormDamagePopupView.AnimationMode.Normal,
                    1f,
                    null);

                Assert.That(initialSequence, Is.Not.Null);
                Assert.That(GetSequence(view), Is.SameAs(initialSequence));
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        private static void SetPrivateField(
            WormDamagePopupView view,
            string fieldName,
            object value)
        {
            FieldInfo field = typeof(WormDamagePopupView).GetField(
                fieldName,
                PrivateInstance);
            Assert.That(field, Is.Not.Null);
            field.SetValue(view, value);
        }

        private static object GetSequence(WormDamagePopupView view)
        {
            FieldInfo field = typeof(WormDamagePopupView).GetField(
                "_sequence",
                PrivateInstance);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(view);
        }
    }
}
