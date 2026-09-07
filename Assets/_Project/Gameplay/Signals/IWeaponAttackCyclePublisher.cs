namespace LastSeed.Gameplay.Signals
{
    public interface IWeaponAttackCyclePublisher
    {
        void Publish(float currentCooldown, float baseCooldown);
    }
}
