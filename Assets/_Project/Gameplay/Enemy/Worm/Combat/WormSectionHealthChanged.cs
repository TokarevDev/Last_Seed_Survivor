using Game.Core.Combat;

namespace Game.Gameplay.Enemy.Worm.Combat
{
    public readonly struct WormSectionHealthChanged
    {
        public WormSectionHealthChanged(
            WormSection section,
            in HealthChange change)
        {
            Section = section;
            Change = change;
        }

        public WormSection Section { get; }
        public HealthChange Change { get; }
    }

}
