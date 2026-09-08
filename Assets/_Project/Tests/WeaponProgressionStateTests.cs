using Game.Core.Combat;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WeaponProgressionStateTests
    {
        private static readonly WeaponProgressionLimits HardLimits = new(
            damageMultiplier: 10f,
            fireRateBonus: 3f,
            salvoExtraShots: 5,
            projectileSpeedBonus: 2f,
            criticalChance: 1f,
            criticalDamageMultiplier: 10f);

        [Test]
        public void SetLimits_ClampsRequestedValuesToHardLimits()
        {
            WeaponProgressionState state = CreateState();

            state.SetLimits(new WeaponProgressionLimits(
                damageMultiplier: 20f,
                fireRateBonus: 6f,
                salvoExtraShots: 8,
                projectileSpeedBonus: 4f,
                criticalChance: 2f,
                criticalDamageMultiplier: 20f));

            Assert.That(state.ApplyDamageMultiplier(20f), Is.EqualTo(9f));
            Assert.That(state.AddFireRateBonus(6f), Is.EqualTo(3f));
            Assert.That(state.AddSalvoShots(8), Is.EqualTo(5));
            Assert.That(state.AddProjectileSpeedBonus(4f), Is.EqualTo(2f));
            Assert.That(state.AddCriticalChance(2f), Is.EqualTo(1f));
            Assert.That(state.AddCriticalDamageBonus(20f), Is.EqualTo(8f));
        }

        [Test]
        public void Reset_RestoresProgressionAndConfiguredCriticalBaseline()
        {
            WeaponProgressionState state = CreateState();
            state.ApplyDamageMultiplier(2f);
            state.AddFireRateBonus(1f);
            state.AddSalvoShots(2);
            state.AddProjectileSpeedBonus(1f);
            state.AddCriticalChance(0.5f);
            state.AddCriticalDamageBonus(1f);

            state.Reset(criticalDamageMultiplier: 3f);

            Assert.That(state.DamageMultiplier, Is.EqualTo(1f));
            Assert.That(state.FireRateBonus, Is.Zero);
            Assert.That(state.SalvoExtraShots, Is.Zero);
            Assert.That(state.ProjectileSpeedBonus, Is.Zero);
            Assert.That(state.CriticalChance, Is.Zero);
            Assert.That(state.CriticalDamageMultiplier, Is.EqualTo(3f));
        }

        [Test]
        public void Clone_CopiesValuesWithoutSharingMutableProgression()
        {
            WeaponProgressionState source = CreateState();
            source.AddFireRateBonus(1f);
            source.AddSalvoShots(1);

            WeaponProgressionState clone = source.Clone();
            clone.AddFireRateBonus(1f);
            clone.AddSalvoShots(1);

            Assert.That(source.FireRateBonus, Is.EqualTo(1f));
            Assert.That(source.SalvoExtraShots, Is.EqualTo(1));
            Assert.That(clone.FireRateBonus, Is.EqualTo(2f));
            Assert.That(clone.SalvoExtraShots, Is.EqualTo(2));
        }

        private static WeaponProgressionState CreateState()
        {
            WeaponProgressionLimits initialLimits = new(
                damageMultiplier: 5f,
                fireRateBonus: 2f,
                salvoExtraShots: 3,
                projectileSpeedBonus: 1.5f,
                criticalChance: 0.75f,
                criticalDamageMultiplier: 6f);

            return new WeaponProgressionState(initialLimits, HardLimits);
        }
    }
}
