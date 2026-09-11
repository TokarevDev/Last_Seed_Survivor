using System;

namespace Game.Gameplay.Enemy.Worm.Combat
{
    public sealed class WormDestructionProgressState :
        IWormDestructionProgressSnapshotProvider
    {
        private int _destroyedSegments;
        private int _totalSegments;

        public WormDestructionProgressSnapshot CurrentProgress => new(
            _destroyedSegments,
            _totalSegments);

        public void Reset(int totalSegments)
        {
            if (totalSegments < 0)
                throw new ArgumentOutOfRangeException(nameof(totalSegments));

            _destroyedSegments = 0;
            _totalSegments = totalSegments;
        }

        public void RecordDestroyed(int segmentCount)
        {
            if (segmentCount < 0)
                throw new ArgumentOutOfRangeException(nameof(segmentCount));

            int remainingSegments = _totalSegments - _destroyedSegments;
            _destroyedSegments += Math.Min(segmentCount, remainingSegments);
        }

        public void Clear()
        {
            _destroyedSegments = 0;
            _totalSegments = 0;
        }
    }
}
