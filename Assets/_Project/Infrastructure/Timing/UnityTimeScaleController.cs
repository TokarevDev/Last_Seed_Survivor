using UnityEngine;

using Game.Core.Timing;

namespace Game.Infrastructure.Timing
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
