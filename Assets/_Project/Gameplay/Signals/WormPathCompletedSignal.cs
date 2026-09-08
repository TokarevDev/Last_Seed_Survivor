namespace Game.Gameplay.Signals
{
    public sealed class WormPathCompletedSignal
    {
        public WormPathCompletedSignal(float headPathProgressNormalized)
        {
            HeadPathProgressNormalized = headPathProgressNormalized;
        }

        public float HeadPathProgressNormalized { get; }
    }
}
