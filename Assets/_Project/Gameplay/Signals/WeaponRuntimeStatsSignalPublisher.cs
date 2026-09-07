using System;
using LastSeed.Core.Timing;
using Zenject;

namespace LastSeed.Gameplay.Signals
{
    public sealed class WeaponRuntimeStatsSignalPublisher
    {
        private readonly SignalBus _signalBus;
        private readonly IGameTimeProvider _gameTimeProvider;

        public WeaponRuntimeStatsSignalPublisher(
            SignalBus signalBus,
            IGameTimeProvider gameTimeProvider)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _gameTimeProvider = gameTimeProvider ??
                throw new ArgumentNullException(nameof(gameTimeProvider));
        }

        public void Publish(WeaponRuntimeStatsSource source)
        {
            _signalBus.Fire(new WeaponRuntimeStatsChangedSignal(
                source,
                _gameTimeProvider.Time));
        }
    }
}
