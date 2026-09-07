using System;
using LastSeed.Core.Collections;
using LastSeed.Gameplay.Signals;
using Zenject;

namespace LastSeed.Bootstrap.Gameplay
{
    public sealed class WormRuntimeInitializer : IInitializable
    {
        private readonly WormController _wormController;
        private readonly WormCombatController _wormCombatController;
        private readonly WormCombatBurstController _combatBurstController;
        private readonly WormFrameSimulation _frameSimulation;
        private readonly WormRailTargetResolver _railTargetResolver;
        private readonly WormPathProgressState _pathProgress;
        private readonly WormSegmentChainPresenter _segmentChainPresenter;
        private readonly WormReviveSequence _reviveSequence;
        private readonly OrderedReferenceSet<WormSegment> _segmentChain;
        private readonly WormSectionRollbackState<WormSegment> _sectionRollbackState;
        private readonly IWormCombatEventPublisher _combatEventPublisher;

        public WormRuntimeInitializer(
            WormController wormController,
            WormCombatController wormCombatController,
            WormCombatBurstController combatBurstController,
            WormFrameSimulation frameSimulation,
            WormRailTargetResolver railTargetResolver,
            WormPathProgressState pathProgress,
            WormSegmentChainPresenter segmentChainPresenter,
            WormReviveSequence reviveSequence,
            OrderedReferenceSet<WormSegment> segmentChain,
            WormSectionRollbackState<WormSegment> sectionRollbackState,
            IWormCombatEventPublisher combatEventPublisher)
        {
            _wormController = wormController ??
                throw new ArgumentNullException(nameof(wormController));
            _wormCombatController = wormCombatController ??
                throw new ArgumentNullException(nameof(wormCombatController));
            _combatBurstController = combatBurstController ??
                throw new ArgumentNullException(nameof(combatBurstController));
            _frameSimulation = frameSimulation ??
                throw new ArgumentNullException(nameof(frameSimulation));
            _railTargetResolver = railTargetResolver ??
                throw new ArgumentNullException(nameof(railTargetResolver));
            _pathProgress = pathProgress ??
                throw new ArgumentNullException(nameof(pathProgress));
            _segmentChainPresenter = segmentChainPresenter ??
                throw new ArgumentNullException(nameof(segmentChainPresenter));
            _reviveSequence = reviveSequence ??
                throw new ArgumentNullException(nameof(reviveSequence));
            _segmentChain = segmentChain ??
                throw new ArgumentNullException(nameof(segmentChain));
            _sectionRollbackState = sectionRollbackState ??
                throw new ArgumentNullException(nameof(sectionRollbackState));
            _combatEventPublisher = combatEventPublisher ??
                throw new ArgumentNullException(nameof(combatEventPublisher));
        }

        public void Initialize()
        {
            _wormController.Configure(
                _combatBurstController,
                _frameSimulation,
                _railTargetResolver,
                _pathProgress,
                _segmentChainPresenter,
                _reviveSequence,
                _segmentChain,
                _sectionRollbackState);
            _wormCombatController.Configure(_combatEventPublisher);
        }
    }
}
