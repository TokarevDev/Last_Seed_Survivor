using LastSeed.Core.Timing;
using UnityEngine;

namespace LastSeed.Infrastructure.Timing
{
    public sealed class UnityTimeScaleController : ITimeScaleController
    {
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
    }
}
