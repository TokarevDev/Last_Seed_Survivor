using NUnit.Framework;

using Game.Gameplay.Combat.Weapons.Runtime;

namespace Game.Tests
{
    public sealed class WeaponShotPatternStateTests
    {
        [Test]
        public void SetParallelLimit_RestrictsParallelProgression()
        {
            WeaponShotPatternState state = new();
            state.SetParallelLimit(maxParallelProjectiles: 2);

            Assert.That(state.AddParallelProjectiles(3, 0.7f), Is.EqualTo(1));
            Assert.That(state.CanAddParallelProjectiles, Is.False);
        }

        [Test]
        public void Clone_CopiesValuesWithoutSharingFutureProgression()
        {
            WeaponShotPatternState source = new();
            source.AddParallelProjectiles(1, 0.8f);
            WeaponShotPatternState clone = source.Clone();

            source.AddParallelProjectiles(1, 1f);

            Assert.That(clone.ParallelProjectileCount, Is.EqualTo(2));
            Assert.That(clone.ParallelSpacing, Is.EqualTo(0.8f));
            Assert.That(source.ParallelProjectileCount, Is.EqualTo(3));
        }

        [Test]
        public void Reset_ClearsPatternProgression()
        {
            WeaponShotPatternState state = new();
            state.AddParallelProjectiles(2, 1f);

            state.Reset();

            Assert.That(state.ParallelProjectileCount, Is.EqualTo(1));
            Assert.That(state.ParallelSpacing, Is.EqualTo(0.5f));
        }
    }
}
