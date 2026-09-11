using System;
using Zenject;

using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Signals;
using Game.Gameplay.Enemy.Worm.Combat;

namespace Game.Presentation.Signals
{
    public sealed class WormCombatSignalPublisher : IWormCombatEventPublisher
    {
        private readonly SignalBus _signalBus;

        public WormCombatSignalPublisher(SignalBus signalBus)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        public void PublishDamage(in DamageViewRequest request)
        {
            _signalBus.Fire(new WormDamageDealtSignal(request));
        }

        public void PublishRewardRequested(
            CocoonRewardProfile rewardProfile,
            float headPathProgressNormalized,
            float wormDestructionProgressNormalized)
        {
            _signalBus.Fire(new WormRewardRequestedSignal(
                rewardProfile,
                headPathProgressNormalized,
                wormDestructionProgressNormalized));
        }

        public void PublishWormDied()
        {
            _signalBus.Fire<WormDiedSignal>();
        }

        public void PublishDestructionProgressChanged(
            in WormDestructionProgressSnapshot snapshot)
        {
            _signalBus.Fire(new WormDestructionProgressChangedSignal(
                snapshot.DestroyedSegments,
                snapshot.TotalSegments,
                snapshot.NormalizedProgress));
        }
    }
}
