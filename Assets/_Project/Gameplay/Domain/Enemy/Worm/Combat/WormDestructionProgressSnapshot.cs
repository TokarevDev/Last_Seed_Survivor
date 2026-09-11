namespace Game.Gameplay.Enemy.Worm.Combat
{
    public readonly struct WormDestructionProgressSnapshot
    {
        public WormDestructionProgressSnapshot(
            int destroyedSegments,
            int totalSegments)
        {
            DestroyedSegments = destroyedSegments;
            TotalSegments = totalSegments;
        }

        public int DestroyedSegments { get; }
        public int TotalSegments { get; }

        public float NormalizedProgress
        {
            get
            {
                if (TotalSegments <= 0)
                    return 0f;

                float normalized = DestroyedSegments / (float)TotalSegments;

                if (normalized <= 0f)
                    return 0f;

                return normalized >= 1f ? 1f : normalized;
            }
        }

        public float RemainingNormalized => 1f - NormalizedProgress;
    }

    public interface IWormDestructionProgressSnapshotProvider
    {
        WormDestructionProgressSnapshot CurrentProgress { get; }
    }
}
