using System;
using Game.Infrastructure.Advertising;

namespace Game.Presentation.UI.Revive
{
    public enum WormRevivePhase
    {
        Running,
        AwaitingDecision,
        AwaitingReward,
        Reviving,
        GivingUp
    }

    public enum WormReviveStageCompletion
    {
        Ignored,
        WaitingForOtherStage,
        FlowCompleted
    }

    public sealed class WormReviveApplicationFlow
    {
        private readonly RewardedAdOperation _rewardedAdOperation;

        private int _maximumAttempts;
        private bool _isPopupClosePending;
        private bool _isRollbackPending;

        public WormReviveApplicationFlow(RewardedAdOperation rewardedAdOperation)
        {
            _rewardedAdOperation = rewardedAdOperation ??
                throw new ArgumentNullException(nameof(rewardedAdOperation));
        }

        public WormRevivePhase Phase { get; private set; }
        public int RemainingAttempts { get; private set; }
        public bool IsWaiting => Phase is WormRevivePhase.AwaitingReward
            or WormRevivePhase.Reviving
            or WormRevivePhase.GivingUp;
        public bool CanRevive =>
            Phase == WormRevivePhase.AwaitingDecision && RemainingAttempts > 0;

        public void InitializeSession(int maximumAttempts)
        {
            if (maximumAttempts < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumAttempts));

            _maximumAttempts = maximumAttempts;
            ResetSession();
        }

        public bool TryBeginFailure()
        {
            if (Phase != WormRevivePhase.Running)
                return false;

            Phase = WormRevivePhase.AwaitingDecision;
            return true;
        }

        public bool TryBeginRewardedRevive(
            Action<bool> onResolved,
            Action onOperationFailed)
        {
            if (onResolved == null)
                throw new ArgumentNullException(nameof(onResolved));

            if (!CanRevive)
                return false;

            Phase = WormRevivePhase.AwaitingReward;
            bool started = _rewardedAdOperation.TryBegin(
                this,
                rewardGranted => ResolveReward(rewardGranted, onResolved),
                () => RecoverRewardOperation(onOperationFailed));

            if (!started && Phase == WormRevivePhase.AwaitingReward)
                Phase = WormRevivePhase.AwaitingDecision;

            return started;
        }

        public bool AbortFailure()
        {
            if (Phase != WormRevivePhase.AwaitingDecision)
                return false;

            Phase = WormRevivePhase.Running;
            return true;
        }

        public bool TryCommitDevelopmentRevive()
        {
            if (!CanRevive)
                return false;

            CommitRevive();
            return true;
        }

        public WormReviveStageCompletion CompleteRollback()
        {
            if (Phase != WormRevivePhase.Reviving || !_isRollbackPending)
                return WormReviveStageCompletion.Ignored;

            _isRollbackPending = false;
            return TryCompleteRevive();
        }

        public WormReviveStageCompletion CompletePopupClose()
        {
            if (Phase != WormRevivePhase.Reviving || !_isPopupClosePending)
                return WormReviveStageCompletion.Ignored;

            _isPopupClosePending = false;
            return TryCompleteRevive();
        }

        public bool TryBeginGiveUp()
        {
            if (Phase != WormRevivePhase.AwaitingDecision)
                return false;

            Phase = WormRevivePhase.GivingUp;
            return true;
        }

        public void CompleteGiveUp()
        {
            if (Phase == WormRevivePhase.GivingUp)
                Phase = WormRevivePhase.Running;
        }

        public void Deactivate()
        {
            _rewardedAdOperation.Cancel(this);
            ClearTransientState();
            Phase = WormRevivePhase.Running;
        }

        public void ResetSession()
        {
            _rewardedAdOperation.Cancel(this);
            RemainingAttempts = _maximumAttempts;
            ClearTransientState();
            Phase = WormRevivePhase.Running;
        }

        private void ResolveReward(bool rewardGranted, Action<bool> onResolved)
        {
            if (Phase != WormRevivePhase.AwaitingReward)
                return;

            if (rewardGranted)
                CommitRevive();
            else
                Phase = WormRevivePhase.AwaitingDecision;

            onResolved(rewardGranted);
        }

        private void RecoverRewardOperation(Action onOperationFailed)
        {
            bool shouldRestoreDecision = Phase is WormRevivePhase.AwaitingReward
                or WormRevivePhase.AwaitingDecision;

            if (Phase == WormRevivePhase.AwaitingReward)
                Phase = WormRevivePhase.AwaitingDecision;

            if (shouldRestoreDecision)
                onOperationFailed?.Invoke();
        }

        private void CommitRevive()
        {
            RemainingAttempts--;
            _isPopupClosePending = true;
            _isRollbackPending = true;
            Phase = WormRevivePhase.Reviving;
        }

        private WormReviveStageCompletion TryCompleteRevive()
        {
            if (_isPopupClosePending || _isRollbackPending)
                return WormReviveStageCompletion.WaitingForOtherStage;

            Phase = WormRevivePhase.Running;
            return WormReviveStageCompletion.FlowCompleted;
        }

        private void ClearTransientState()
        {
            _isPopupClosePending = false;
            _isRollbackPending = false;
        }
    }
}
