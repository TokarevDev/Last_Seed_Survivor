
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Signals;

namespace Game.Presentation.Worm
{
    using System;
    using Zenject;

    public sealed class WormPathCompletedSignalPublisher
    {
        private readonly SignalBus _signalBus;

        public WormPathCompletedSignalPublisher(SignalBus signalBus)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        public void Publish(in WormFrameResult result)
        {
            if (!result.PathCompletion.HasValue)
                return;

            WormPathCompletion completion = result.PathCompletion.Value;
            _signalBus.Fire(new WormPathCompletedSignal(completion));
        }
    }

}
