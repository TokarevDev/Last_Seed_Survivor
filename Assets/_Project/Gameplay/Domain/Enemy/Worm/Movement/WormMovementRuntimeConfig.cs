namespace Game.Gameplay.Enemy.Worm.Movement
{
    public readonly struct WormMovementRuntimeConfig
    {
        public WormMovementRuntimeConfig(
            in WormForwardMotionSettings forwardMotion,
            float rollbackSpeed,
            float sectionRollbackForwardSpeedMultiplier)
        {
            ForwardMotion = forwardMotion;
            RollbackSpeed = rollbackSpeed;
            SectionRollbackForwardSpeedMultiplier = sectionRollbackForwardSpeedMultiplier;
        }

        public WormForwardMotionSettings ForwardMotion { get; }
        public float BaseSpeed => ForwardMotion.BaseSpeed;
        public float RollbackSpeed { get; }
        public float SectionRollbackForwardSpeedMultiplier { get; }
    }
}
