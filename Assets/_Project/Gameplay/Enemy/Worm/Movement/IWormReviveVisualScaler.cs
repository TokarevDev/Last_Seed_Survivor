using System.Collections.Generic;
using Game.Gameplay.Enemy.Worm;

namespace Game.Gameplay.Enemy.Worm.Movement
{
    public interface IWormReviveVisualScaler
    {
        void Capture(IReadOnlyList<WormSegment> segments);

        void Apply(
            IReadOnlyList<WormSegment> segments,
            float xMultiplier,
            float yMultiplier);

        void RestoreAndClear(IReadOnlyList<WormSegment> segments);
    }

}
