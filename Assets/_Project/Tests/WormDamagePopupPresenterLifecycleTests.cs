using System.Collections.Generic;
using System.Reflection;
using Game.Presentation.Worm;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class WormDamagePopupPresenterLifecycleTests
    {
        private GameObject _presenterObject;

        [TearDown]
        public void TearDown()
        {
            if (_presenterObject != null)
                Object.DestroyImmediate(_presenterObject);
        }

        [Test]
        public void OnDisable_ClearsTrackedPopupsAndCounters()
        {
            _presenterObject = new GameObject("WormDamagePopupPresenter");
            WormDamagePopupPresenter presenter =
                _presenterObject.AddComponent<WormDamagePopupPresenter>();
            GetList(presenter, "_activePopups").Add(null);
            GetList(presenter, "_activeNonCriticalPopups").Add(null);
            SetInt(presenter, "_activeCount", 3);
            SetInt(presenter, "_activeNormalCount", 2);
            SetInt(presenter, "_activeDamageOverTimeCount", 1);

            InvokeOnDisable(presenter);

            Assert.That(GetList(presenter, "_activePopups"), Is.Empty);
            Assert.That(GetList(presenter, "_activeNonCriticalPopups"), Is.Empty);
            Assert.That(GetInt(presenter, "_activeCount"), Is.Zero);
            Assert.That(GetInt(presenter, "_activeNormalCount"), Is.Zero);
            Assert.That(GetInt(presenter, "_activeDamageOverTimeCount"), Is.Zero);
        }

        private static List<WormDamagePopupView> GetList(
            WormDamagePopupPresenter presenter,
            string fieldName)
        {
            return (List<WormDamagePopupView>)GetField(fieldName).GetValue(presenter);
        }

        private static int GetInt(WormDamagePopupPresenter presenter, string fieldName)
        {
            return (int)GetField(fieldName).GetValue(presenter);
        }

        private static void SetInt(
            WormDamagePopupPresenter presenter,
            string fieldName,
            int value)
        {
            GetField(fieldName).SetValue(presenter, value);
        }

        private static FieldInfo GetField(string fieldName)
        {
            FieldInfo field = typeof(WormDamagePopupPresenter).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing test field '{fieldName}'.");
            return field;
        }

        private static void InvokeOnDisable(WormDamagePopupPresenter presenter)
        {
            MethodInfo method = typeof(WormDamagePopupPresenter).GetMethod(
                "OnDisable",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(presenter, null);
        }
    }
}
