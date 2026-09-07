namespace LastSeed.Gameplay.Signals
{
    public interface IWeaponRuntimeStatsPublisher
    {
        void Publish(WeaponRuntimeStatsSource source);
    }
}
