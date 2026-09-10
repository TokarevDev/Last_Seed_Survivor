using System.Collections.Generic;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    public interface IRewardChoiceRoller
    {
        List<RewardChoiceData> Roll3(
            RewardRuntimeContext context,
            CocoonRewardProfile cocoonProfile = null,
            RewardRarity? guaranteedRarity = null,
            int guaranteedRaritySlotCount = 1,
            RewardRollContext rollContext = default);

        RewardRarity RollGuaranteeRarity(
            RewardRuntimeContext context,
            CocoonRewardProfile cocoonProfile = null,
            RewardRollContext rollContext = default);
    }

}
