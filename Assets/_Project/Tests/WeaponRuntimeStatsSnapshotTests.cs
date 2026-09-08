using Game.Gameplay.Signals;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WeaponRuntimeStatsSnapshotTests
    {
        [Test]
        public void Signal_PreservesCommittedSnapshotValues()
        {
            WeaponRuntimeStatsSnapshot snapshot = new(
                WeaponRuntimeStatsSource.MainProjectile,
                true,
                42,
                0.75f,
                1.5f,
                3,
                2,
                1,
                0.25f,
                2.5f);

            WeaponRuntimeStatsChangedSignal signal = new(in snapshot, 12f);

            Assert.That(signal.Source, Is.EqualTo(WeaponRuntimeStatsSource.MainProjectile));
            Assert.That(signal.Snapshot.ProjectileDamage, Is.EqualTo(42));
            Assert.That(signal.Snapshot.ShotCooldown, Is.EqualTo(0.75f));
            Assert.That(signal.Snapshot.SalvoShots, Is.EqualTo(3));
            Assert.That(signal.OccurredAt, Is.EqualTo(12f));
        }
    }
}
