using System;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Signals;
using Zenject;

namespace Game.Presentation.Worm
{
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
