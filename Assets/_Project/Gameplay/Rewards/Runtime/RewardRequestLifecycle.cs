
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Services;

namespace Game.Gameplay.Rewards.Runtime
{
    using System;
    using System.Collections.Generic;

    public sealed class RewardRequestLifecycle
    {
        public bool IsActive { get; private set; }
        public bool ShouldOpenNext { get; private set; }
        public CocoonRewardProfile CocoonProfile { get; private set; }
        public RewardRollContext RollContext { get; private set; }
        public RewardRarity GuaranteeRarity { get; private set; }
        public List<RewardChoiceData> Choices { get; private set; }

        public void Begin(in RewardOpenRequest request)
        {
            if (IsActive)
                throw new InvalidOperationException("A reward request is already active.");

            IsActive = true;
            ShouldOpenNext = false;
            CocoonProfile = request.CocoonProfile;
            RollContext = request.RollContext;
            GuaranteeRarity = default;
            Choices = null;
        }

        public void SetRollResult(
            RewardRarity guaranteeRarity,
            List<RewardChoiceData> choices)
        {
            if (!IsActive)
                throw new InvalidOperationException("A reward request must be active before rolling.");

            GuaranteeRarity = guaranteeRarity;
            Choices = choices;
        }

        public void MarkShouldOpenNext()
        {
            if (IsActive)
                ShouldOpenNext = true;
        }

        public bool Complete()
        {
            bool shouldOpenNext = ShouldOpenNext;
            Reset();
            return shouldOpenNext;
        }

        public void Reset()
        {
            Choices = null;
            CocoonProfile = null;
            RollContext = default;
            GuaranteeRarity = default;
            IsActive = false;
            ShouldOpenNext = false;
        }
    }

}
