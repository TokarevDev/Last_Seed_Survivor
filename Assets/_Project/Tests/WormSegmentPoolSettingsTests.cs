using System;
using System.Reflection;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Gameplay.Enemy.Worm.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class WormSegmentPoolSettingsTests
    {
        private GameObject _parentObject;
        private GameObject _headObject;
        private GameObject _bodyObject;
        private GameObject _tailObject;

        [SetUp]
        public void SetUp()
        {
            _parentObject = new GameObject("PoolRoot");
            _headObject = CreateSegmentObject("Head", WormSegmentType.Head);
            _bodyObject = CreateSegmentObject("Body", WormSegmentType.Body);
            _tailObject = CreateSegmentObject("Tail", WormSegmentType.Tail);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_parentObject);
            UnityEngine.Object.DestroyImmediate(_headObject);
            UnityEngine.Object.DestroyImmediate(_bodyObject);
            UnityEngine.Object.DestroyImmediate(_tailObject);
        }

        [Test]
        public void Constructor_AcceptsMatchingPrefabTypes()
        {
            WormSegment head = _headObject.GetComponent<WormSegment>();
            WormSegment body = _bodyObject.GetComponent<WormSegment>();
            WormSegment tail = _tailObject.GetComponent<WormSegment>();

            WormSegmentPoolSettings settings = new(
                _parentObject.transform,
                head,
                body,
                tail);

            Assert.That(settings.HeadPrefab, Is.SameAs(head));
            Assert.That(settings.BodyPrefab, Is.SameAs(body));
            Assert.That(settings.TailPrefab, Is.SameAs(tail));
        }

        [Test]
        public void Constructor_RejectsPrefabOwnedByDifferentPool()
        {
            WormSegment body = _bodyObject.GetComponent<WormSegment>();
            WormSegment tail = _tailObject.GetComponent<WormSegment>();

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                new WormSegmentPoolSettings(
                    _parentObject.transform,
                    body,
                    body,
                    tail));

            Assert.That(exception.ParamName, Is.EqualTo("headPrefab"));
            Assert.That(exception.Message, Does.Contain("must have type Head"));
        }

        [Test]
        public void Get_UnknownType_DoesNotRentFromBodyPool()
        {
            WormSegmentPoolSettings settings = new(
                _parentObject.transform,
                _headObject.GetComponent<WormSegment>(),
                _bodyObject.GetComponent<WormSegment>(),
                _tailObject.GetComponent<WormSegment>());
            WormSegmentPool pool = new(settings, new FakeCocoonShakeClock());
            LogAssert.Expect(LogType.Error, "Prefab for None is not assigned");

            WormSegment segment = pool.Get(WormSegmentType.None);

            Assert.That(segment, Is.Null);
        }

        private static GameObject CreateSegmentObject(
            string name,
            WormSegmentType type)
        {
            GameObject segmentObject = new(name);
            WormSegment segment = segmentObject.AddComponent<WormSegment>();
            PropertyInfo typeProperty = typeof(WormSegment).GetProperty(
                nameof(WormSegment.Type),
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = typeProperty?.GetSetMethod(true);

            Assert.That(setter, Is.Not.Null);
            setter.Invoke(segment, new object[] { type });
            return segmentObject;
        }

        private sealed class FakeCocoonShakeClock : IWormCocoonShakeClock
        {
            public float RotationOffset => 0f;

            public void Register(float interval, float angle)
            {
            }

            public void Unregister()
            {
            }
        }
    }
}
