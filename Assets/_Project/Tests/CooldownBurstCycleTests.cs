using NUnit.Framework;

using Game.Core.Timing;

namespace Game.Tests
{
    public sealed class CooldownBurstCycleTests
    {
        [Test]
        public void Advance_ReportsCycleReadyAfterCooldown()
        {
            CooldownBurstCycle cycle = new();
            cycle.StartCooldown(1f);

            Assert.That(cycle.Advance(0.5f), Is.EqualTo(CooldownBurstCycleStep.None));
            Assert.That(cycle.Advance(0.5f), Is.EqualTo(CooldownBurstCycleStep.CycleReady));
        }

        [Test]
        public void Burst_PreservesActionTimingAndStartsCooldownAfterFinalCommit()
        {
            CooldownBurstCycle cycle = new();
            cycle.BeginBurst(2);

            Assert.That(cycle.Advance(0f), Is.EqualTo(CooldownBurstCycleStep.BurstActionReady));
            Assert.That(cycle.CommitBurstAction(0.25f, 2f), Is.False);
            Assert.That(cycle.Advance(0.1f), Is.EqualTo(CooldownBurstCycleStep.None));
            Assert.That(cycle.Advance(0.15f), Is.EqualTo(CooldownBurstCycleStep.BurstActionReady));
            Assert.That(cycle.CommitBurstAction(0.25f, 2f), Is.True);
            Assert.That(cycle.CooldownRemaining, Is.EqualTo(2f));
        }

        [Test]
        public void LimitCooldown_DoesNotInterruptActiveBurst()
        {
            CooldownBurstCycle cycle = new();
            cycle.StartCooldown(4f);
            cycle.BeginBurst(1);

            cycle.LimitCooldown(0.5f);

            Assert.That(cycle.CooldownRemaining, Is.EqualTo(4f));
            Assert.That(cycle.IsBurstActive, Is.True);
        }

        [Test]
        public void Reset_ClearsBurstAndMakesCycleImmediatelyReady()
        {
            CooldownBurstCycle cycle = new();
            cycle.StartCooldown(3f);
            cycle.BeginBurst(3);

            cycle.Reset();

            Assert.That(cycle.IsBurstActive, Is.False);
            Assert.That(cycle.CooldownRemaining, Is.Zero);
            Assert.That(cycle.Advance(0f), Is.EqualTo(CooldownBurstCycleStep.CycleReady));
        }
    }
}
