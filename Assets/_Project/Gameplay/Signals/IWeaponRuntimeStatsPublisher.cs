namespace Game.Gameplay.Signals
{
    public interface IWeaponRuntimeStatsPublisher
    {
        void Publish(in WeaponRuntimeStatsSnapshot snapshot);
    }
}
