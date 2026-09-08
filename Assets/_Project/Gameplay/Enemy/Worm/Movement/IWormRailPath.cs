namespace Game.Gameplay.Enemy.Worm.Movement
{
    public interface IWormRailPath
    {
        float TotalLength { get; }

        bool TryGetControlPointDistance(int pointIndex, out float distance);
    }

}
