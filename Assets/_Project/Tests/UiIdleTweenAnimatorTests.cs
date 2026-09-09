using System.Reflection;
using Game.Presentation.UI.Common;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class UiIdleTweenAnimatorTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject _owner;
        private UiIdleTweenAnimator _animator;

        [SetUp]
        public void SetUp()
        {
            _owner = new GameObject("IdleTween", typeof(RectTransform));
            _animator = _owner.AddComponent<UiIdleTweenAnimator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_owner);
        }

        [Test]
        public void DisableThenEnable_ReusesIdleSequence()
        {
            _animator.Play();
            object initialSequence = GetIdleSequence();

            _owner.SetActive(false);
            _owner.SetActive(true);

            Assert.That(initialSequence, Is.Not.Null);
            Assert.That(GetIdleSequence(), Is.SameAs(initialSequence));
        }

        private object GetIdleSequence()
        {
            FieldInfo field = typeof(UiIdleTweenAnimator).GetField(
                "_idleSequence",
                PrivateInstance);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(_animator);
        }
    }
}
