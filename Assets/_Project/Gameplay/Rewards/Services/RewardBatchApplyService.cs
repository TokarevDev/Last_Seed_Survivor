using System;
using System.Collections.Generic;
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
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

            List<Exception> failures = null;

            for (int index = 0; index < choices.Count; index++)
            {
                try
                {
                    _choiceApplier.Apply(choices[index]);
                }
                catch (Exception exception)
                {
                    failures ??= new List<Exception>();
                    failures.Add(exception);
                }
            }

            if (failures != null)
            {
                throw new AggregateException(
                    "One or more rewards could not be applied.",
                    failures);
            }
        }
    }

}
