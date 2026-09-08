using UnityEngine;

using Game.Core.Timing;

namespace Game.Infrastructure.Timing
{
    public sealed class UnityScaledGameTimeProvider : IGameTimeProvider
    {
        public float Time => UnityEngine.Time.time;
    }
}
