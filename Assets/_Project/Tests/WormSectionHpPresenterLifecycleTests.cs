using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Presentation.Worm;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class WormSectionHpPresenterLifecycleTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly List<GameObject> _segmentObjects = new();
        private GameObject _presenterObject;
        private GameObject _viewPrefabObject;
        private GameObject _viewRootObject;
        private WormSectionHpPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _presenterObject = new GameObject("WormSectionHpPresenter");
            LogAssert.Expect(
                LogType.Error,
                "WormSectionHpPresenter: View prefab is not assigned.");
            LogAssert.Expect(
                LogType.Error,
                "WormSectionHpPresenter: Root is not assigned.");
            _presenter = _presenterObject.AddComponent<WormSectionHpPresenter>();
            _viewRootObject = new GameObject("HpViewRoot");
            _viewPrefabObject = CreateViewPrefab();
            SetPrivateField(
                _presenter,
                "_viewPool",
                new WormSectionHpViewPool(
                    _viewPrefabObject.GetComponent<WormSectionHpView>(),
                    _viewRootObject.transform));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_presenterObject);
            UnityEngine.Object.DestroyImmediate(_viewRootObject);
            UnityEngine.Object.DestroyImmediate(_viewPrefabObject);

            for (int index = 0; index < _segmentObjects.Count; index++)
                UnityEngine.Object.DestroyImmediate(_segmentObjects[index]);

            _segmentObjects.Clear();
        }

        [Test]
        public void Clear_WhenViewsFailToReturn_ReleasesEverySubscriptionAndReportsFailures()
        {
            WormSection firstSection = CreateSection("FirstSegment");
            WormSection secondSection = CreateSection("SecondSegment");
            _presenter.BindSections(new[] { firstSection, secondSection });
            Dictionary<WormSection, WormSectionHpView> views = GetViews();
            List<WormSectionHpView> activeViews = new(views.Values);

            for (int index = 0; index < activeViews.Count; index++)
                UnityEngine.Object.DestroyImmediate(activeViews[index].gameObject);

            Exception exception = Assert.Catch<Exception>(_presenter.Clear);

            Assert.That(exception, Is.TypeOf<AggregateException>());
            Assert.That(((AggregateException)exception).InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(views, Is.Empty);
            Assert.That(GetSubscriberCount(firstSection, "HpChanged"), Is.Zero);
            Assert.That(GetSubscriberCount(firstSection, "Destroyed"), Is.Zero);
            Assert.That(GetSubscriberCount(secondSection, "HpChanged"), Is.Zero);
            Assert.That(GetSubscriberCount(secondSection, "Destroyed"), Is.Zero);
        }

        private GameObject CreateViewPrefab()
        {
            GameObject viewObject = new("HpViewPrefab");
            viewObject.SetActive(false);
            TextMeshPro text = viewObject.AddComponent<TextMeshPro>();
            LogAssert.Expect(LogType.Error, "WormSectionHpView: TMP_Text is not assigned.");
            WormSectionHpView view = viewObject.AddComponent<WormSectionHpView>();
            SetPrivateField(view, "_text", text);
            SetPrivateField(view, "_visualRoot", viewObject.transform);
            return viewObject;
        }

        private WormSection CreateSection(string segmentName)
        {
            GameObject segmentObject = new(segmentName);
            _segmentObjects.Add(segmentObject);
            WormSegment segment = segmentObject.AddComponent<WormSegment>();
            WormSection section = new();
            section.AddSegment(segment);
            section.InitializeHp(100);
            return section;
        }

        private Dictionary<WormSection, WormSectionHpView> GetViews()
        {
            return GetPrivateField<Dictionary<WormSection, WormSectionHpView>>(
                _presenter,
                "_views");
        }

        private static int GetSubscriberCount(WormSection section, string eventName)
        {
            Delegate subscribers = GetPrivateField<Delegate>(section, eventName);
            return subscribers?.GetInvocationList().Length ?? 0;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Missing test field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Missing test field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
