
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    using System;
    using System.Collections.Generic;

    public sealed class RewardBatchApplyService
    {
        private readonly IRewardChoiceApplier _choiceApplier;

        public RewardBatchApplyService(IRewardChoiceApplier choiceApplier)
        {
            _choiceApplier = choiceApplier ??
                throw new ArgumentNullException(nameof(choiceApplier));
        }

        public void ApplyAll(IReadOnlyList<RewardChoiceData> choices)
        {
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            for (int index = 0; index < choices.Count; index++)
                _choiceApplier.Apply(choices[index]);
        }
    }

}
