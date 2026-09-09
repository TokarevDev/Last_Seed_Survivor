using Game.Gameplay.Enemy.Worm.Movement;

namespace Game.Gameplay.Signals
{
    public sealed class WormPathCompletedSignal
    {
        public WormPathCompletedSignal(in WormPathCompletion completion)
        {
            Completion = completion;
        }

        public WormPathCompletion Completion { get; }
    }
}
