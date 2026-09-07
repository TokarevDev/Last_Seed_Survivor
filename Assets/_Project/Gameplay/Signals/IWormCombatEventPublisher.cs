namespace LastSeed.Gameplay.Signals
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
            int destroyedSegments,
            int totalSegments,
            float normalizedProgress);
    }
}
