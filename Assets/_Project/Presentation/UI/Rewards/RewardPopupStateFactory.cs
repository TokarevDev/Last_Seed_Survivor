using System;
using Game.Gameplay.Rewards;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Infrastructure.Advertising;

namespace Game.Presentation.UI.Rewards
{
    public sealed class RewardPopupStateFactory
    {
        private readonly RewardAttemptState _attempts;
        private readonly IRewardedAdService _rewardedAdService;

        public RewardPopupStateFactory(
            RewardAttemptState attempts,
            IRewardedAdService rewardedAdService)
        {
            _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
            _rewardedAdService = rewardedAdService ??
                throw new ArgumentNullException(nameof(rewardedAdService));
        }

        public RewardPopupState Create(
            RewardRarity guaranteeRarity,
            CocoonRewardProfile cocoonProfile,
            in RewardRollContext rollContext,
            bool isRewardOperationPending)
        {
            bool canStartRewardedAd =
                !isRewardOperationPending && _rewardedAdService.IsReady;
            bool canTakeAll = _attempts.HasTakeAll
                && canStartRewardedAd
                && RewardAdRerollPolicy.CanOfferTakeAll(rollContext);

            return new RewardPopupState(
                _attempts.FreeRerollsLeft,
                _attempts.AdRerollsLeft,
                _attempts.TakeAllLeft,
                guaranteeRarity,
                RewardAdRerollPolicy.GetDisplayedGuaranteeRarity(cocoonProfile),
                _attempts.HasFreeReroll && !isRewardOperationPending,
                !_attempts.HasFreeReroll
                    && _attempts.HasAdReroll
                    && canStartRewardedAd,
                canTakeAll);
        }
    }

}
