namespace Game.Gameplay.Enemy.Worm.Movement
{
    public readonly struct WormReviveConfig
    {
        public WormReviveConfig(
            int rollbackRailPointIndex,
            in WormReviveAnimationSettings animationSettings)
        {
            RollbackRailPointIndex = rollbackRailPointIndex;
            AnimationSettings = animationSettings;
        }

        public int RollbackRailPointIndex { get; }
        public WormReviveAnimationSettings AnimationSettings { get; }
    }
}
