namespace Game.Gameplay.Enemy.Worm.Movement
{
    public readonly struct WormMovementStepResult
    {
        public WormMovementStepResult(bool pathCompleted, bool completeReviveAfterRender)
        {
            PathCompleted = pathCompleted;
            CompleteReviveAfterRender = completeReviveAfterRender;
        }

        public bool PathCompleted { get; }
        public bool CompleteReviveAfterRender { get; }
    }
}
