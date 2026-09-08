namespace Game.Gameplay.Signals
{
    public interface IWeaponRuntimeStatsPublisher
    {
        void Publish(WeaponRuntimeStatsSource source);
    }
}
