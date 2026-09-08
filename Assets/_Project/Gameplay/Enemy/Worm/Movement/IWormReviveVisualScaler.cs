using System.Collections.Generic;

public interface IWormReviveVisualScaler
{
    void Capture(IReadOnlyList<WormSegment> segments);

    void Apply(
        IReadOnlyList<WormSegment> segments,
        float xMultiplier,
        float yMultiplier);

    void RestoreAndClear(IReadOnlyList<WormSegment> segments);
}
