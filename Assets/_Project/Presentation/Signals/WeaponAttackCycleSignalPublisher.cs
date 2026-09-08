using System;
using Zenject;

using Game.Gameplay.Signals;

namespace Game.Presentation.Signals
{
    public sealed class WeaponAttackCycleSignalPublisher : IWeaponAttackCyclePublisher
    {
        private readonly SignalBus _signalBus;

        public WeaponAttackCycleSignalPublisher(SignalBus signalBus)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        public void Publish(float currentCooldown, float baseCooldown)
        {
            _signalBus.Fire(new WeaponAttackCycleStartedSignal(
                currentCooldown,
                baseCooldown));
        }
    }
}
