using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class CriticalDamageResolverTests
    {
        [Test]
        public void Roll_WithZeroChance_DoesNotConsumeRandomValue()
        {
            TestRandomSource randomSource = new(fallback: 0f);

            CriticalDamageRoll roll = CriticalDamageResolver.Roll(
                rawDamage: 10d,
                criticalChance: 0f,
                criticalDamageMultiplier: 3f,
                randomSource);

            Assert.That(roll.Damage, Is.EqualTo(10));
            Assert.That(roll.IsCritical, Is.False);
            Assert.That(roll.DamageKind, Is.EqualTo(DamageKind.Normal));
            Assert.That(randomSource.Calls, Is.Zero);
        }

        [TestCase(0.24f, true, 30)]
        [TestCase(0.25f, false, 10)]
        public void Roll_UsesStrictChanceBoundary(
            float randomValue,
            bool expectedCritical,
            int expectedDamage)
        {
            TestRandomSource randomSource = new(fallback: randomValue);

            CriticalDamageRoll roll = CriticalDamageResolver.Roll(
                rawDamage: 10d,
                criticalChance: 0.25f,
                criticalDamageMultiplier: 3f,
                randomSource);

            Assert.That(roll.Damage, Is.EqualTo(expectedDamage));
            Assert.That(roll.IsCritical, Is.EqualTo(expectedCritical));
            Assert.That(randomSource.Calls, Is.EqualTo(1));
        }
    }
}
