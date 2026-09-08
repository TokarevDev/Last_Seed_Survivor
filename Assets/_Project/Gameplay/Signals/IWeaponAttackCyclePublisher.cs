namespace Game.Gameplay.Signals
{
    public interface IWeaponAttackCyclePublisher
    {
        void Publish(float currentCooldown, float baseCooldown);
    }
}
