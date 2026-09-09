using System;
using System.Collections.Generic;
using Game.Core.Collections;
using Game.Gameplay.Enemy.Worm.Presentation;

namespace Game.Gameplay.Enemy.Worm.Movement
{
    public sealed class WormLifecycleController
    {
        private readonly WormReviveSequence _reviveSequence;
        private readonly OrderedReferenceSet<WormSegment> _segmentChain;
        private readonly WormSegmentChainPresenter _segmentPresenter;
        private readonly WormSectionRollbackState<WormSegment> _rollbackState;
        private readonly WormCombatBurstController _combatBurst;
        private readonly WormPathProgressState _pathProgress;
        private readonly WormRailTargetResolver _railTargets;

        public WormLifecycleController(
            WormReviveSequence reviveSequence,
            OrderedReferenceSet<WormSegment> segmentChain,
            WormSegmentChainPresenter segmentPresenter,
            WormSectionRollbackState<WormSegment> rollbackState,
            WormCombatBurstController combatBurst,
            WormPathProgressState pathProgress,
            WormRailTargetResolver railTargets)
        {
            _reviveSequence = reviveSequence ?? throw new ArgumentNullException(nameof(reviveSequence));
            _segmentChain = segmentChain ?? throw new ArgumentNullException(nameof(segmentChain));
            _segmentPresenter = segmentPresenter ?? throw new ArgumentNullException(nameof(segmentPresenter));
            _rollbackState = rollbackState ?? throw new ArgumentNullException(nameof(rollbackState));
            _combatBurst = combatBurst ?? throw new ArgumentNullException(nameof(combatBurst));
            _pathProgress = pathProgress ?? throw new ArgumentNullException(nameof(pathProgress));
            _railTargets = railTargets ?? throw new ArgumentNullException(nameof(railTargets));
        }

        public bool IsCombatBurstActive => _combatBurst.IsActive;

        public void Initialize(
            IReadOnlyList<WormSegment> segments,
            float baseSpeed,
            bool startsCatchingUp)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            ResetSharedState(baseSpeed);
            _segmentChain.ReplaceWith(segments);
            _pathProgress.Reset(startsCatchingUp);
        }

        public void Clear(float baseSpeed)
        {
            ResetSharedState(baseSpeed);
            _segmentChain.Clear();
            _pathProgress.Reset();
        }

        public void CancelPendingOperations()
        {
            _reviveSequence.Cancel();
        }

        private void ResetSharedState(float baseSpeed)
        {
            _reviveSequence.Cancel();
            _segmentPresenter.Reset();
            _rollbackState.Complete();
            _combatBurst.Reset(baseSpeed);
            _railTargets.Clear();
        }
    }
}
