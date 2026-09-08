using System;

namespace Game.Core.Timing
{
    public enum CooldownBurstCycleStep
    {
        None,
        CycleReady,
        BurstActionReady
    }

    public sealed class CooldownBurstCycle
    {
        private readonly CountdownTimer _cooldown = new();
        private readonly TimedBurst _burst = new();

        public bool IsBurstActive => _burst.IsActive;
        public float CooldownRemaining => _cooldown.Remaining;

        public CooldownBurstCycleStep Advance(float deltaTime)
        {
            if (_burst.IsActive)
            {
                _burst.Advance(deltaTime);
                return _burst.IsShotReady
                    ? CooldownBurstCycleStep.BurstActionReady
                    : CooldownBurstCycleStep.None;
            }

            _cooldown.Advance(deltaTime);
            return _cooldown.IsElapsed
                ? CooldownBurstCycleStep.CycleReady
                : CooldownBurstCycleStep.None;
        }

        public void BeginBurst(int actionCount)
        {
            _burst.Begin(actionCount);
        }

        public bool CommitBurstAction(
            float nextActionDelay,
            float completedCycleCooldown)
        {
            _burst.CommitShot(nextActionDelay);

            if (_burst.IsActive)
                return false;

            _cooldown.Start(completedCycleCooldown);
            return true;
        }

        public void StartCooldown(float duration)
        {
            _cooldown.Start(duration);
        }

        public void LimitCooldown(float maximumRemaining)
        {
            if (!_burst.IsActive)
                _cooldown.LimitTo(maximumRemaining);
        }

        public void CancelBurst()
        {
            _burst.Reset();
        }

        public void Reset()
        {
            _burst.Reset();
            _cooldown.Reset();
        }
    }
}
