using System;
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    public sealed class RewardGrantedActionService
    {
        private readonly RewardAttemptState _attempts;
        private readonly RewardRequestLifecycle _requestLifecycle;
        private readonly IRewardChoiceRollService _choiceRollService;
        private readonly RewardBatchApplyService _batchApplyService;

        public RewardGrantedActionService(
            RewardAttemptState attempts,
            RewardRequestLifecycle requestLifecycle,
            IRewardChoiceRollService choiceRollService,
            RewardBatchApplyService batchApplyService)
        {
            _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
            _requestLifecycle = requestLifecycle ??
                throw new ArgumentNullException(nameof(requestLifecycle));
            _choiceRollService = choiceRollService ??
                throw new ArgumentNullException(nameof(choiceRollService));
            _batchApplyService = batchApplyService ??
                throw new ArgumentNullException(nameof(batchApplyService));
        }

        public bool CompleteAdReroll()
        {
            if (!_requestLifecycle.IsActive || !_attempts.ConsumeAdReroll())
                return false;

            RewardChoiceRollResult result = _choiceRollService.RollAdAssisted(
                _requestLifecycle.CocoonProfile,
                _requestLifecycle.RollContext);
            _requestLifecycle.SetRollResult(result.GuaranteeRarity, result.Choices);
            return result.HasChoices;
        }

        public bool CompleteTakeAll()
        {
            if (!_requestLifecycle.IsActive || _requestLifecycle.Choices == null ||
                _requestLifecycle.Choices.Count == 0 || !_attempts.ConsumeTakeAll())
            {
                return false;
            }

            _requestLifecycle.MarkShouldOpenNext();
            _batchApplyService.ApplyAll(_requestLifecycle.Choices);
            return true;
        }
    }

}
