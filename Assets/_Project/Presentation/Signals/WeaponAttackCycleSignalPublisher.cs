using System;
using LastSeed.Gameplay.Signals;
using Zenject;

namespace LastSeed.Presentation.Signals
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
