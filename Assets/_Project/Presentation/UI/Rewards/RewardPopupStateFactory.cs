
using Game.Gameplay.Rewards;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;

namespace Game.Presentation.UI.Rewards
{
    using System;

    public sealed class RewardPopupStateFactory
    {
        private readonly RewardAttemptState _attempts;

        public RewardPopupStateFactory(RewardAttemptState attempts)
        {
            _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
        }

        public RewardPopupState Create(
            RewardRarity guaranteeRarity,
            CocoonRewardProfile cocoonProfile,
            in RewardRollContext rollContext,
            bool isRewardOperationPending)
        {
            bool canTakeAll = _attempts.HasTakeAll
                && !isRewardOperationPending
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
                    && !isRewardOperationPending,
                canTakeAll);
        }
    }

}
