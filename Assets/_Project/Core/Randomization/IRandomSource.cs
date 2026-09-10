namespace Game.Core.Randomization
{
    public interface IRandomSource
    {
        float NextUnitFloat();
        int NextInt(int minInclusive, int maxExclusive);
    }

}
