namespace Game.Core.World
{
    public interface IPathSampler<out TPoint>
    {
        float TotalLength { get; }

        TPoint GetPoint(float distance);
    }
}
