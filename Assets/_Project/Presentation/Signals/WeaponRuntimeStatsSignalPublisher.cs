using System;
using Zenject;

using Game.Core.Timing;
using Game.Gameplay.Signals;

namespace Game.Presentation.Signals
{
    public sealed class WeaponRuntimeStatsSignalPublisher : IWeaponRuntimeStatsPublisher
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

        public void Publish(in WeaponRuntimeStatsSnapshot snapshot)
        {
            _signalBus.Fire(new WeaponRuntimeStatsChangedSignal(
                in snapshot,
                _gameTimeProvider.Time));
        }
    }
}
