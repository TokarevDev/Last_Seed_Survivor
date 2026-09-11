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
    }

    public interface IWormDestructionProgressSnapshotProvider
    {
        WormDestructionProgressSnapshot CurrentProgress { get; }
    }
}
