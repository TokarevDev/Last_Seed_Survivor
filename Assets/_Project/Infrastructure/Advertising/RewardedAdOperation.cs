using System;
using UnityEngine;

namespace Game.Infrastructure.Advertising
{
    public sealed class RewardedAdOperation
    {
        private readonly IRewardedAdService _rewardedAdService;
        private readonly object _defaultOwner = new();

        private int _version;
        private object _owner;

        public RewardedAdOperation(IRewardedAdService rewardedAdService)
        {
            _rewardedAdService = rewardedAdService
                ?? throw new ArgumentNullException(nameof(rewardedAdService));
        }

        public bool IsPending { get; private set; }
        public bool CanBegin => !IsPending && IsAvailable;

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
            return TryBegin(_defaultOwner, onCompleted, onCompletionFailed);
        }

        public bool TryBegin(
            object owner,
            Action<bool> onCompleted,
            Action onCompletionFailed = null)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (onCompleted == null)
                throw new ArgumentNullException(nameof(onCompleted));

            if (!CanBegin)
                return false;

            IsPending = true;
            _owner = owner;
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
            Cancel(_defaultOwner);
        }

        public bool Cancel(object owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            if (!IsPending || !ReferenceEquals(_owner, owner))
                return false;

            IsPending = false;
            _owner = null;
            _version++;
            return true;
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
            _owner = null;

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
            _owner = null;
            _version++;
        }
    }
}
