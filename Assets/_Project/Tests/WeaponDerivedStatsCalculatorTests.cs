using Game.Core.Combat;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WeaponDerivedStatsCalculatorTests
    {
        [Test]
        public void CalculateDamage_AppliesMultiplierAndSharedClamp()
        {
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateDamage(12, 2.5f),
                Is.EqualTo(30));
        }

        [Test]
        public void CalculateCooldown_AppliesBonusWithoutCrossingMinimum()
        {
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateCooldown(3f, 0.75f, 1f),
                Is.EqualTo(1.5f));
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateCooldown(3f, 0.75f, 10f),
                Is.EqualTo(0.75f));
        }

        [Test]
        public void CalculateProjectileSpeedMultiplier_NormalizesInvalidBonus()
        {
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(1.5f),
                Is.EqualTo(2.5f));
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(-2f),
                Is.EqualTo(WeaponDerivedStatsCalculator.MinimumProjectileSpeedMultiplier));
        }

        [Test]
        public void CalculateSalvoInterval_ScalesByProjectileSpeedAndKeepsMinimum()
        {
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateSalvoInterval(0.2f, 2f),
                Is.EqualTo(0.1f));
            Assert.That(
                WeaponDerivedStatsCalculator.CalculateSalvoInterval(0.001f, 2f),
                Is.EqualTo(WeaponDerivedStatsCalculator.MinimumSalvoInterval));
        }
    }
}
