using System;
using UnityEngine;

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

        public bool IsAvailable
        {
            get
            {
                try
                {
                    return _rewardedAdService.IsReady;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return false;
                }
            }
        }

        public bool TryBegin(
            Action<bool> onCompleted,
            Action onCompletionFailed = null)
        {
            if (onCompleted == null)
                throw new ArgumentNullException(nameof(onCompleted));

            if (IsPending || !IsAvailable)
                return false;

            IsPending = true;
            int operationVersion = ++_version;
            try
            {
                _rewardedAdService.ShowRewardedAd(
                    rewardGranted => Complete(
                        operationVersion,
                        onCompleted,
                        onCompletionFailed,
                        rewardGranted));
            }
            catch (Exception exception)
            {
                Rollback(operationVersion);
                Debug.LogException(exception);
                return false;
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
            Action onCompletionFailed,
            bool rewardGranted)
        {
            if (!IsPending || operationVersion != _version)
                return;

            IsPending = false;

            try
            {
                onCompleted(rewardGranted);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                TryRecover(onCompletionFailed);
            }
        }

        private static void TryRecover(Action onCompletionFailed)
        {
            if (onCompletionFailed == null)
                return;

            try
            {
                onCompletionFailed();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
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
