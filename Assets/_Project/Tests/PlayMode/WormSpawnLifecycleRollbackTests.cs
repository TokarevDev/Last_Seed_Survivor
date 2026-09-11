using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Pooling;
using Game.Core.Randomization;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Gameplay.Enemy.Worm.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class WormSpawnLifecycleRollbackTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject _poolRoot;
        private GameObject _wormControllerObject;
        private GameObject _combatControllerObject;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_combatControllerObject);
            UnityEngine.Object.DestroyImmediate(_wormControllerObject);
            UnityEngine.Object.DestroyImmediate(_poolRoot);
        }

        [Test]
        public void Spawn_WhenControllerInitAndCleanupFail_PreservesOriginalFailureAndReleasesSegments()
        {
            WormSegmentPool segmentPool = CreateSegmentPool();
            WormFactory factory = new(segmentPool);
            WormAdaptiveHpController adaptiveHpController = new(
                null,
                new InvalidWeaponPowerProvider(),
                null,
                new WormAdaptiveHpSettings(1, 1, 0f));

            _wormControllerObject = new GameObject("WormController");
            LogAssert.Expect(LogType.Error, "WormController requires a rail path.");
            LogAssert.Expect(LogType.Error, "WormController requires a movement config.");
            WormController wormController =
                _wormControllerObject.AddComponent<WormController>();

            _combatControllerObject = new GameObject("WormCombatController");
            WormCombatController combatController =
                _combatControllerObject.AddComponent<WormCombatController>();
            TrackingHealthPresentation healthPresentation = new();
            TrackingFacePresentation facePresentation = new();
            WormSpawnLifecycle lifecycle = new(
                segmentPool,
                factory,
                new WormSpawnSettings(1, 0, 1),
                adaptiveHpController,
                wormController,
                combatController,
                healthPresentation,
                facePresentation,
                new FixedRandomSource());

            AggregateException exception = Assert.Throws<AggregateException>(
                () => lifecycle.Spawn(null, 12f));

            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(
                exception.InnerExceptions[0].StackTrace,
                Does.Contain($"{nameof(WormController)}.{nameof(WormController.Init)}"));
            Assert.That(
                exception.InnerExceptions[1].StackTrace,
                Does.Contain($"{nameof(WormController)}.{nameof(WormController.ClearWorm)}"));
            Assert.That(GetActiveSegmentCount(segmentPool), Is.Zero);
            Assert.That(GetAdaptiveSectionCount(adaptiveHpController), Is.Zero);
            Assert.That(healthPresentation.ClearCalls, Is.Zero);
            Assert.That(facePresentation.UnbindCalls, Is.Zero);
            Assert.That(lifecycle.IsSpawned, Is.False);
        }

        private WormSegmentPool CreateSegmentPool()
        {
            _poolRoot = new GameObject("WormSegmentPoolRoot");
            WormSegment head = CreateSegmentPrefab("Head", WormSegmentType.Head);
            WormSegment body = CreateSegmentPrefab("Body", WormSegmentType.Body);
            WormSegment tail = CreateSegmentPrefab("Tail", WormSegmentType.Tail);
            WormSegmentPoolSettings settings = new(
                _poolRoot.transform,
                head,
                body,
                tail);
            return new WormSegmentPool(settings, new FakeCocoonShakeClock());
        }

        private WormSegment CreateSegmentPrefab(string name, WormSegmentType type)
        {
            GameObject segmentObject = new(name);
            segmentObject.transform.SetParent(_poolRoot.transform);
            WormSegment segment = segmentObject.AddComponent<WormSegment>();
            SetPrivateField(segment, "<Type>k__BackingField", type);
            SetPrivateField(segment, "<VisualRoot>k__BackingField", segmentObject.transform);
            return segment;
        }

        private static int GetActiveSegmentCount(WormSegmentPool pool)
        {
            return GetPool(pool, "_headPool").ActiveCount
                + GetPool(pool, "_bodyPool").ActiveCount
                + GetPool(pool, "_tailPool").ActiveCount;
        }

        private static ObjectPool<WormSegment> GetPool(
            WormSegmentPool pool,
            string fieldName)
        {
            return GetPrivateField<ObjectPool<WormSegment>>(pool, fieldName);
        }

        private static int GetAdaptiveSectionCount(
            WormAdaptiveHpController controller)
        {
            return GetPrivateField<List<IWormSectionHpTarget>>(controller, "_sections").Count;
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

        private sealed class InvalidWeaponPowerProvider : IWeaponPowerProvider
        {
            public WeaponPowerSnapshot GetCurrentPower()
            {
                return WeaponPowerSnapshot.Invalid;
            }
        }

        private sealed class FixedRandomSource : IRandomSource
        {
            public float NextUnitFloat()
            {
                return 0f;
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                return minInclusive;
            }
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

        private sealed class TrackingHealthPresentation : IWormSectionHealthPresentation
        {
            public int ClearCalls { get; private set; }

            public void BindSections(IReadOnlyList<WormSection> sections)
            {
            }

            public void Clear()
            {
                ClearCalls++;
            }
        }

        private sealed class TrackingFacePresentation : IWormFaceBurstPresentation
        {
            public int UnbindCalls { get; private set; }

            public void Bind(IWormFaceBurstView faceView)
            {
            }

            public void Unbind()
            {
                UnbindCalls++;
            }
        }
    }
}
