using System;
using Game.Gameplay.Enemy.Worm.Combat;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WormDestructionProgressStateTests
    {
        [Test]
        public void Reset_CreatesZeroProgressSnapshot()
        {
            WormDestructionProgressState state = new();

            state.Reset(10);

            WormDestructionProgressSnapshot snapshot = state.CurrentProgress;
            Assert.That(snapshot.DestroyedSegments, Is.Zero);
            Assert.That(snapshot.TotalSegments, Is.EqualTo(10));
            Assert.That(snapshot.NormalizedProgress, Is.Zero);
            Assert.That(snapshot.RemainingNormalized, Is.EqualTo(1f));
        }

        [Test]
        public void RecordDestroyed_ClampsAtTotalWithoutOverflowingProgress()
        {
            WormDestructionProgressState state = new();
            state.Reset(10);

            state.RecordDestroyed(4);
            state.RecordDestroyed(int.MaxValue);

            WormDestructionProgressSnapshot snapshot = state.CurrentProgress;
            Assert.That(snapshot.DestroyedSegments, Is.EqualTo(10));
            Assert.That(snapshot.NormalizedProgress, Is.EqualTo(1f));
            Assert.That(snapshot.RemainingNormalized, Is.Zero);
        }

        [Test]
        public void Clear_RemovesProgressFromPreviousSession()
        {
            WormDestructionProgressState state = new();
            state.Reset(10);
            state.RecordDestroyed(7);

            state.Clear();

            WormDestructionProgressSnapshot snapshot = state.CurrentProgress;
            Assert.That(snapshot.DestroyedSegments, Is.Zero);
            Assert.That(snapshot.TotalSegments, Is.Zero);
            Assert.That(snapshot.NormalizedProgress, Is.Zero);
            Assert.That(snapshot.RemainingNormalized, Is.EqualTo(1f));
        }

        [TestCase(-1)]
        public void Reset_WhenTotalIsNegative_Throws(int invalidTotal)
        {
            WormDestructionProgressState state = new();

            Assert.Throws<ArgumentOutOfRangeException>(() => state.Reset(invalidTotal));
        }

        [TestCase(-1)]
        public void RecordDestroyed_WhenCountIsNegative_Throws(int invalidCount)
        {
            WormDestructionProgressState state = new();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.RecordDestroyed(invalidCount));
        }
    }
}
