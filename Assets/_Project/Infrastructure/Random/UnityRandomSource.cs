using UnityEngine;

public sealed class UnityRandomSource : IRandomSource
{
    public float NextUnitFloat()
    {
        return Random.value;
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        return Random.Range(minInclusive, maxExclusive);
    }
}
