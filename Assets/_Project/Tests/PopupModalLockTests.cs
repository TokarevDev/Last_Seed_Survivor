using System;
using NUnit.Framework;

using Game.Core.Timing;
using Game.Gameplay.Input;
using Game.Presentation.UI.Common.Popups;

namespace Game.Tests
{
    public sealed class PopupModalLockTests
    {
        [Test]
        public void AcquireAndRelease_AreIdempotentAndRestoreCapturedTimeScale()
        {
            GameplayInputLock inputLock = new();
            FakeTimeScaleController timeScale = new(0.75f);
            PopupModalLock modalLock = new(inputLock, timeScale, pauseTime: true);

            modalLock.Acquire();
            modalLock.Acquire();

            Assert.That(modalLock.IsAcquired, Is.True);
            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.Zero);

            modalLock.Release();
            modalLock.Release();

            Assert.That(modalLock.IsAcquired, Is.False);
            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(timeScale.TimeScale, Is.EqualTo(0.75f));
        }

        [Test]
        public void Acquire_WhenPauseDisabled_OnlyOwnsGameplayInputLock()
        {
            GameplayInputLock inputLock = new();
            FakeTimeScaleController timeScale = new(0.5f);
            PopupModalLock modalLock = new(inputLock, timeScale, pauseTime: false);

            modalLock.Acquire();

            Assert.That(inputLock.IsLocked, Is.True);
            Assert.That(timeScale.TimeScale, Is.EqualTo(0.5f));

            modalLock.Dispose();

            Assert.That(inputLock.IsLocked, Is.False);
            Assert.That(timeScale.TimeScale, Is.EqualTo(0.5f));
        }

        [Test]
        public void Acquire_WhenTimeScaleCaptureFails_RollsBackInputLock()
        {
            GameplayInputLock inputLock = new();
            ThrowingTimeScaleController timeScale = new();
            PopupModalLock modalLock = new(inputLock, timeScale, pauseTime: true);

            Assert.Throws<InvalidOperationException>(modalLock.Acquire);
            Assert.That(modalLock.IsAcquired, Is.False);
            Assert.That(inputLock.IsLocked, Is.False);
        }

        private sealed class FakeTimeScaleController : ITimeScaleController
        {
            public FakeTimeScaleController(float timeScale)
            {
                TimeScale = timeScale;
            }

            public float TimeScale { get; set; }
        }

        private sealed class ThrowingTimeScaleController : ITimeScaleController
        {
            public float TimeScale
            {
                get => throw new InvalidOperationException("Time scale is unavailable.");
                set => throw new InvalidOperationException("Time scale is unavailable.");
            }
        }
    }
}
