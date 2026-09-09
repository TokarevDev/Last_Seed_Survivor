
using Game.Core.Collections;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Presentation;

namespace Game.Gameplay.Enemy.Worm.Movement
{
    using System;
    using Unity.Profiling;

    public sealed class WormFrameSimulation
    {
        public const string ProfilerMarkerName = "LastSeed.Gameplay.WormFrame";

        private static readonly ProfilerMarker FrameProfilerMarker =
            new(ProfilerMarkerName);

        private readonly WormMovementCoordinator _movement;
        private readonly WormSegmentChainPresenter _segmentPresenter;
        private readonly OrderedReferenceSet<WormSegment> _segmentChain;
        private readonly WormSectionRollbackState<WormSegment> _rollbackState;

        public WormFrameSimulation(
            WormMovementCoordinator movement,
            WormSegmentChainPresenter segmentPresenter,
            OrderedReferenceSet<WormSegment> segmentChain,
            WormSectionRollbackState<WormSegment> rollbackState)
        {
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _segmentPresenter = segmentPresenter ?? throw new ArgumentNullException(nameof(segmentPresenter));
            _segmentChain = segmentChain ?? throw new ArgumentNullException(nameof(segmentChain));
            _rollbackState = rollbackState ?? throw new ArgumentNullException(nameof(rollbackState));
        }

        public bool Tick(in WormFrameContext context)
        {
            using (FrameProfilerMarker.Auto())
            {
                if (_segmentChain.Count == 0 || context.Rail == null)
                    return false;

                WormMovementStepResult movementResult = _movement.Advance(context);
                Render(context.Rail, context.SegmentLayout);

                if (movementResult.CompleteReviveAfterRender)
                    _movement.CompleteReviveAfterRender();

                return movementResult.PathCompleted;
            }
        }

        public void Render(RailPath rail, in WormSegmentChainLayout layout)
        {
            _segmentPresenter.Render(
                _segmentChain.Items,
                rail,
                _rollbackState.AnchoredDistances,
                layout);
        }

    }

}
