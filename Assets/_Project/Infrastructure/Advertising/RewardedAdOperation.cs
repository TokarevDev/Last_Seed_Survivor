using System;

namespace Game.Infrastructure.Advertising
{
    public sealed class RewardedAdOperation
    {
        private readonly IRewardedAdService _rewardedAdService;

        private int _version;

        public RewardedAdOperation(IRewardedAdService rewardedAdService)
        {
            _rewardedAdService = rewardedAdService
                ?? throw new ArgumentNullException(nameof(rewardedAdService));
        }

        public bool IsPending { get; private set; }

        public bool TryBegin(Action<bool> onCompleted)
        {
            if (onCompleted == null)
                throw new ArgumentNullException(nameof(onCompleted));

            if (IsPending)
                return false;

            IsPending = true;
            int operationVersion = ++_version;
            try
            {
                _rewardedAdService.ShowRewardedAd(
                    rewardGranted => Complete(operationVersion, onCompleted, rewardGranted));
            }
            catch
            {
                Rollback(operationVersion);
                throw;
            }

            return true;
        }

        public void Cancel()
        {
            IsPending = false;
            _version++;
        }

        private void Complete(
            int operationVersion,
            Action<bool> onCompleted,
            bool rewardGranted)
        {
            if (!IsPending || operationVersion != _version)
                return;

            IsPending = false;
            onCompleted(rewardGranted);
        }

        private void Rollback(int operationVersion)
        {
            if (operationVersion != _version)
                return;

            IsPending = false;
            _version++;
        }
    }
}
