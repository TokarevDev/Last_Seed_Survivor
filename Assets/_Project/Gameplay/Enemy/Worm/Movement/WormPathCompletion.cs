namespace Game.Gameplay.Enemy.Worm.Movement
{
    public enum WormPathCompletionReason
    {
        ReachedRailEnd = 0
    }

    public readonly struct WormPathCompletion
    {
        public WormPathCompletion(
            float finalDistance,
            float normalizedProgress,
            WormPathCompletionReason reason)
        {
            FinalDistance = finalDistance;
            NormalizedProgress = normalizedProgress;
            Reason = reason;
        }

        public float FinalDistance { get; }
        public float NormalizedProgress { get; }
        public WormPathCompletionReason Reason { get; }
    }
}
