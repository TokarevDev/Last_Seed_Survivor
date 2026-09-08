namespace Game.Core.Timing
{
    using System;

    public enum WeaponFireCycleStep
    {
        None,
        CycleReady,
        PreparationReady,
        BurstActionReady
    }

    public sealed class WeaponFireCycle
    {
        private readonly CooldownBurstCycle _cycle = new();
        private readonly PreparedActionTimer _preparation = new();

        public bool IsBurstActive => _cycle.IsBurstActive;
        public bool IsPreparationActive => _preparation.IsActive;
        public float PreparationElapsed => _preparation.Elapsed;
        public float CooldownRemaining => _cycle.CooldownRemaining;

        public WeaponFireCycleStep Advance(
            float deltaTime,
            float preparationDuration = 0f)
        {
            if (_cycle.IsBurstActive)
            {
                return _cycle.Advance(deltaTime) ==
                    CooldownBurstCycleStep.BurstActionReady
                        ? WeaponFireCycleStep.BurstActionReady
                        : WeaponFireCycleStep.None;
            }

            if (_preparation.IsActive)
            {
                _preparation.Advance(deltaTime);
                return _preparation.HasReached(preparationDuration)
                    ? WeaponFireCycleStep.PreparationReady
                    : WeaponFireCycleStep.None;
            }

            return _cycle.Advance(deltaTime) == CooldownBurstCycleStep.CycleReady
                ? WeaponFireCycleStep.CycleReady
                : WeaponFireCycleStep.None;
        }

        public void BeginPreparation()
        {
            _preparation.Begin();
        }

        public bool CompletePreparation(float elapsed, float maximumDelay)
        {
            return _preparation.TryComplete(elapsed, maximumDelay);
        }

        public void BeginBurst(int actionCount)
        {
            _cycle.BeginBurst(actionCount);
        }

        public bool CommitBurstAction(float nextActionDelay, float cycleCooldown)
        {
            float remainingCooldown = Math.Max(
                0f,
                cycleCooldown - _preparation.LastCompletionDelay);
            return _cycle.CommitBurstAction(nextActionDelay, remainingCooldown);
        }

        public void StartCooldown(float duration)
        {
            _cycle.StartCooldown(duration);
        }

        public void LimitCooldown(float maximumRemaining)
        {
            if (!_preparation.IsActive)
                _cycle.LimitCooldown(maximumRemaining);
        }

        public void CancelTransient()
        {
            _preparation.Reset();
            _cycle.CancelBurst();
        }

        public void Reset()
        {
            _preparation.Reset();
            _cycle.Reset();
        }
    }
}
