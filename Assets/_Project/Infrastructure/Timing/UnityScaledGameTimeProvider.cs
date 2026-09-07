using LastSeed.Core.Timing;
using UnityEngine;

namespace LastSeed.Infrastructure.Timing
{
    public sealed class UnityScaledGameTimeProvider : IGameTimeProvider
    {
        public float Time => UnityEngine.Time.time;
    }
}
