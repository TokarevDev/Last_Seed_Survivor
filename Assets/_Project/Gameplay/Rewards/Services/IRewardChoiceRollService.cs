
using Game.Gameplay.Rewards.Data;

namespace Game.Gameplay.Rewards.Services
{
    public interface IRewardChoiceRollService
    {
        RewardChoiceRollResult RollStandard(
            CocoonRewardProfile cocoonProfile,
            RewardRollContext rollContext);

        RewardChoiceRollResult RollAdAssisted(
            CocoonRewardProfile cocoonProfile,
            RewardRollContext rollContext);
    }

}
