using System;
using NUnit.Framework;
using UnityEngine;

using Game.Gameplay.Enemy.Worm.Presentation;

namespace Game.Tests
{
    public sealed class WormSegmentCocoonPresenterTests
    {
        private GameObject _ownerObject;
        private GameObject _visualObject;
        private SpriteRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            _ownerObject = new GameObject("Owner");
            _visualObject = new GameObject("Cocoon");
            _visualObject.transform.SetParent(_ownerObject.transform, false);
            _renderer = _visualObject.AddComponent<SpriteRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_ownerObject);
        }

        [Test]
        public void ShowAndHide_OwnShakeRegistrationAndVisualLifecycle()
        {
            FakeShakeClock shakeClock = new();
            WormSegmentCocoonPresenter presenter = CreatePresenter();
            presenter.BindShakeClock(shakeClock, ownerIsActive: true);

            presenter.Show(null, ownerIsActive: true);
            presenter.OnOwnerEnabled();

            Assert.That(presenter.IsVisible, Is.True);
            Assert.That(_visualObject.activeSelf, Is.True);
            Assert.That(shakeClock.RegisterCount, Is.EqualTo(1));

            presenter.Hide();
            presenter.OnOwnerDisabled();

            Assert.That(presenter.IsVisible, Is.False);
            Assert.That(_visualObject.activeSelf, Is.False);
            Assert.That(shakeClock.UnregisterCount, Is.EqualTo(1));
        }

        [Test]
        public void UpdateOrientationAndSorting_ApplyCurrentSharedShakeOffset()
        {
            FakeShakeClock shakeClock = new() { RotationOffset = 5f };
            WormSegmentCocoonPresenter presenter = CreatePresenter();
            presenter.BindShakeClock(shakeClock, ownerIsActive: true);
            presenter.Show(null, ownerIsActive: true);
            _ownerObject.transform.rotation = Quaternion.Euler(0f, 0f, 30f);

            presenter.UpdateOrientation();
            presenter.SetSortingOrder(7);

            Assert.That(
                Mathf.DeltaAngle(_visualObject.transform.localEulerAngles.z, -25f),
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(_renderer.sortingOrder, Is.EqualTo(107));
        }

        [Test]
        public void ShowWhileDisabled_DefersShakeRegistrationUntilEnable()
        {
            FakeShakeClock shakeClock = new();
            WormSegmentCocoonPresenter presenter = CreatePresenter();
            presenter.BindShakeClock(shakeClock, ownerIsActive: false);

            presenter.Show(null, ownerIsActive: false);

            Assert.That(shakeClock.RegisterCount, Is.Zero);

            presenter.OnOwnerEnabled();

            Assert.That(shakeClock.RegisterCount, Is.EqualTo(1));
        }

        [Test]
        public void RegistrationFailure_DoesNotCommitOwnershipAndCanRetry()
        {
            ThrowOnceShakeClock shakeClock = new();
            WormSegmentCocoonPresenter presenter = CreatePresenter();
            presenter.BindShakeClock(shakeClock, ownerIsActive: false);
            presenter.Show(null, ownerIsActive: false);

            Assert.Throws<InvalidOperationException>(presenter.OnOwnerEnabled);
            Assert.That(shakeClock.RegisterCount, Is.EqualTo(1));

            Assert.DoesNotThrow(presenter.OnOwnerEnabled);
            Assert.That(shakeClock.RegisterCount, Is.EqualTo(2));

            presenter.Hide();

            Assert.That(shakeClock.UnregisterCount, Is.EqualTo(1));
        }

        [Test]
        public void UnregistrationFailure_PreservesOwnershipAndCanRetry()
        {
            ThrowOnceUnregisterShakeClock shakeClock = new();
            WormSegmentCocoonPresenter presenter = CreatePresenter();
            presenter.BindShakeClock(shakeClock, ownerIsActive: false);
            presenter.Show(null, ownerIsActive: true);

            Assert.Throws<InvalidOperationException>(presenter.Hide);
            Assert.That(shakeClock.UnregisterCount, Is.EqualTo(1));
            Assert.That(presenter.IsVisible, Is.True);

            Assert.DoesNotThrow(presenter.Hide);
            Assert.That(shakeClock.UnregisterCount, Is.EqualTo(2));
            Assert.That(presenter.IsVisible, Is.False);
        }

        private WormSegmentCocoonPresenter CreatePresenter()
        {
            return new WormSegmentCocoonPresenter(
                _ownerObject.transform,
                _visualObject,
                shakeInterval: 3f,
                shakeAngle: 10f);
        }

        private sealed class FakeShakeClock : IWormCocoonShakeClock
        {
            public float RotationOffset { get; set; }
            public int RegisterCount { get; private set; }
            public int UnregisterCount { get; private set; }

            public void Register(float interval, float angle)
            {
                RegisterCount++;
            }

            public void Unregister()
            {
                UnregisterCount++;
            }
        }

        private sealed class ThrowOnceShakeClock : IWormCocoonShakeClock
        {
            public float RotationOffset => 0f;
            public int RegisterCount { get; private set; }
            public int UnregisterCount { get; private set; }

            public void Register(float interval, float angle)
            {
                RegisterCount++;

                if (RegisterCount == 1)
                    throw new InvalidOperationException("Shake registration failed.");
            }

            public void Unregister()
            {
                UnregisterCount++;
            }
        }

        private sealed class ThrowOnceUnregisterShakeClock : IWormCocoonShakeClock
        {
            public float RotationOffset => 0f;
            public int UnregisterCount { get; private set; }

            public void Register(float interval, float angle)
            {
            }

            public void Unregister()
            {
                UnregisterCount++;

                if (UnregisterCount == 1)
                    throw new InvalidOperationException("Shake unregistration failed.");
            }
        }
    }
}
