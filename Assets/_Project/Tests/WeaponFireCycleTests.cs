using Game.Core.Timing;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WeaponFireCycleTests
    {
        [Test]
        public void PreparedBurst_AdvancesThroughNamedStagesAndCompensatesCooldown()
        {
            WeaponFireCycle cycle = new();

            Assert.That(cycle.Advance(0f), Is.EqualTo(WeaponFireCycleStep.CycleReady));
            cycle.BeginPreparation();
            Assert.That(cycle.Advance(0.25f, 0.5f), Is.EqualTo(WeaponFireCycleStep.None));
            Assert.That(cycle.Advance(0.25f, 0.5f), Is.EqualTo(WeaponFireCycleStep.PreparationReady));
            Assert.That(cycle.CompletePreparation(0.5f, 2f), Is.True);

            cycle.BeginBurst(1);
            Assert.That(cycle.Advance(0f), Is.EqualTo(WeaponFireCycleStep.BurstActionReady));
            Assert.That(cycle.CommitBurstAction(0.2f, 2f), Is.True);
            Assert.That(cycle.CooldownRemaining, Is.EqualTo(1.5f));
        }

        [Test]
        public void ImmediateBurst_UsesFullCooldownWithoutPreparation()
        {
            WeaponFireCycle cycle = new();
            cycle.BeginBurst(1);

            Assert.That(cycle.Advance(0f), Is.EqualTo(WeaponFireCycleStep.BurstActionReady));
            cycle.CommitBurstAction(0.2f, 2f);

            Assert.That(cycle.CooldownRemaining, Is.EqualTo(2f));
        }

        [Test]
        public void CancelTransient_ClearsPreparationAndBurstWithoutResettingCooldown()
        {
            WeaponFireCycle cycle = new();
            cycle.StartCooldown(2f);
            cycle.BeginPreparation();
            cycle.BeginBurst(2);

            cycle.CancelTransient();

            Assert.That(cycle.IsPreparationActive, Is.False);
            Assert.That(cycle.IsBurstActive, Is.False);
            Assert.That(cycle.CooldownRemaining, Is.EqualTo(2f));
        }
    }
}
