using System;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using UnityEngine;

namespace Game.Presentation.UI.Rewards
{
    public sealed class RewardSelectionCommitter
    {
        private readonly IRewardChoiceApplier _choiceApplier;
        private readonly RewardRequestLifecycle _requestLifecycle;

        public RewardSelectionCommitter(
            IRewardChoiceApplier choiceApplier,
            RewardRequestLifecycle requestLifecycle)
        {
            _choiceApplier = choiceApplier ??
                throw new ArgumentNullException(nameof(choiceApplier));
            _requestLifecycle = requestLifecycle ??
                throw new ArgumentNullException(nameof(requestLifecycle));
        }

        public void Commit(RewardChoiceData choice)
        {
            _requestLifecycle.MarkShouldOpenNext();

            try
            {
                _choiceApplier.Apply(choice);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
