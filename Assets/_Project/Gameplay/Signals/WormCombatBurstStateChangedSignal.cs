namespace Game.Gameplay.Signals
{
    public sealed class WormCombatBurstStateChangedSignal
    {
        public WormCombatBurstStateChangedSignal(bool isActive)
        {
            IsActive = isActive;
        }

        public bool IsActive { get; }
    }
}
