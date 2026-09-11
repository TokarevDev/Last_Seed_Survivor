using System;
using System.Reflection;
using Game.Core.World;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class PoolRegistryTests
    {
        private GameObject _registryObject;
        private GameObject _poolPrefabObject;
        private GameObject _projectilePrefabObject;

        [SetUp]
        public void SetUp()
        {
            _registryObject = new GameObject("PoolRegistry");
            _poolPrefabObject = new GameObject("ProjectilePoolPrefab");
            _projectilePrefabObject = new GameObject("ProjectilePrefab");
        }

        [TearDown]
        public void TearDown()
        {
            PoolRegistryAwakeProbe.Callback = null;
            UnityEngine.Object.DestroyImmediate(_registryObject);
            UnityEngine.Object.DestroyImmediate(_poolPrefabObject);
            UnityEngine.Object.DestroyImmediate(_projectilePrefabObject);
        }

        [Test]
        public void GetPool_WhenRegistrationBecomesReentrant_DestroysUnregisteredPool()
        {
            PoolRegistry registry = _registryObject.AddComponent<PoolRegistry>();
            ProjectilePool poolPrefab = _poolPrefabObject.AddComponent<ProjectilePool>();
            Projectile projectilePrefab = _projectilePrefabObject.AddComponent<Projectile>();
            _poolPrefabObject.AddComponent<PoolRegistryAwakeProbe>();
            SetPrivateField(poolPrefab, "_prewarmCount", 0);
            SetPrivateField(registry, "_poolPrefab", poolPrefab);
            registry.Init(new StubScreenBounds(), new TestRandomSource());
            PoolRegistryAwakeProbe.Callback = () => registry.GetPool(projectilePrefab);

            Assert.Throws<ArgumentException>(() => registry.GetPool(projectilePrefab));

            Assert.That(registry.transform.childCount, Is.EqualTo(1));
            Assert.That(registry.GetPool(projectilePrefab),
                Is.SameAs(registry.transform.GetChild(0).GetComponent<ProjectilePool>()));
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private sealed class StubScreenBounds : IScreenBounds
        {
            public float Left => -1f;
            public float Right => 1f;
            public float Top => 1f;
            public float Bottom => -1f;
        }
    }

    [ExecuteAlways]
    public sealed class PoolRegistryAwakeProbe : MonoBehaviour
    {
        public static Action Callback { get; set; }

        private void Awake()
        {
            Action callback = Callback;
            Callback = null;
            callback?.Invoke();
        }
    }
}
