
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    public interface IRewardChoiceApplier
    {
        void Apply(RewardChoiceData choice);
    }

}
