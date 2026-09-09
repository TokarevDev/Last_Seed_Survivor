namespace Game.Gameplay.Enemy.Worm.Movement
{
    public readonly struct WormFrameResult
    {
        public WormFrameResult(WormPathCompletion? pathCompletion)
        {
            PathCompletion = pathCompletion;
        }

        public WormPathCompletion? PathCompletion { get; }
    }

}
