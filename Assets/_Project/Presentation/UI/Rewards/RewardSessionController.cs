using System;
using Game.Gameplay.Rewards.Services;
using Game.Gameplay.Signals;
using Zenject;

namespace Game.Presentation.UI.Rewards
{
    public sealed class RewardSessionController : IInitializable, System.IDisposable
    {
        private readonly RewardFlowController _rewardFlow;
        private readonly SignalBus _signalBus;

        private bool _hasRevivedThisRun;
        private bool _isSubscribed;

        public RewardSessionController(
            RewardFlowController rewardFlow,
            SignalBus signalBus)
        {
            _rewardFlow = rewardFlow;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            if (_isSubscribed)
                return;

            _signalBus.Subscribe<WormReviveGrantedSignal>(HandleReviveGranted);

            try
            {
                _signalBus.Subscribe<WormRewardRequestedSignal>(HandleRewardRequested);
            }
            catch (Exception subscriptionException)
            {
                try
                {
                    _signalBus.Unsubscribe<WormReviveGrantedSignal>(HandleReviveGranted);
                }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "Reward session signal subscription and rollback both failed.",
                        subscriptionException,
                        rollbackException);
                }

                throw;
            }

            _isSubscribed = true;
        }

        public void Dispose()
        {
            if (!_isSubscribed)
                return;

            _signalBus.Unsubscribe<WormReviveGrantedSignal>(HandleReviveGranted);
            _signalBus.Unsubscribe<WormRewardRequestedSignal>(HandleRewardRequested);
            _isSubscribed = false;
        }

        public void ResetSession()
        {
            _hasRevivedThisRun = false;
            _rewardFlow.ResetSession();
        }

        private void HandleReviveGranted(WormReviveGrantedSignal signal)
        {
            _hasRevivedThisRun = true;
        }

        private void HandleRewardRequested(WormRewardRequestedSignal signal)
        {
            _rewardFlow.Open(
                signal.RewardProfile,
                new RewardRollContext(
                    signal.HeadPathProgressNormalized,
                    signal.WormDestructionProgressNormalized,
                    _hasRevivedThisRun));
        }
    }

}
