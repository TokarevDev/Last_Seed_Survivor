
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Enemy.Worm.Combat;

namespace Game.Gameplay.Signals
{
    public interface IWormCombatEventPublisher
    {
        void PublishDamage(in DamageViewRequest request);

        void PublishRewardRequested(
            CocoonRewardProfile rewardProfile,
            float headPathProgressNormalized,
            float wormDestructionProgressNormalized);

        void PublishWormDied();

        void PublishDestructionProgressChanged(
            in WormDestructionProgressSnapshot snapshot);
    }
}
