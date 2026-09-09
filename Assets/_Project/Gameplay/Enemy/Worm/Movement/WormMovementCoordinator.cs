using System;
using Game.Core.Collections;

namespace Game.Gameplay.Enemy.Worm.Movement
{
    public sealed class WormMovementCoordinator
    {
        private readonly WormForwardMotionController _forwardMotion;
        private readonly WormPathProgressState _pathProgress;
        private readonly WormReviveSequence _reviveSequence;
        private readonly OrderedReferenceSet<WormSegment> _segmentChain;
        private readonly WormSectionRollbackMotionController<WormSegment> _rollbackMotion;
        private readonly WormSectionRollbackState<WormSegment> _rollbackState;

        public WormMovementCoordinator(
            WormForwardMotionController forwardMotion,
            WormPathProgressState pathProgress,
            WormReviveSequence reviveSequence,
            OrderedReferenceSet<WormSegment> segmentChain,
            WormSectionRollbackMotionController<WormSegment> rollbackMotion,
            WormSectionRollbackState<WormSegment> rollbackState)
        {
            _forwardMotion = forwardMotion ?? throw new ArgumentNullException(nameof(forwardMotion));
            _pathProgress = pathProgress ?? throw new ArgumentNullException(nameof(pathProgress));
            _reviveSequence = reviveSequence ?? throw new ArgumentNullException(nameof(reviveSequence));
            _segmentChain = segmentChain ?? throw new ArgumentNullException(nameof(segmentChain));
            _rollbackMotion = rollbackMotion ?? throw new ArgumentNullException(nameof(rollbackMotion));
            _rollbackState = rollbackState ?? throw new ArgumentNullException(nameof(rollbackState));
        }

        public WormMovementStepResult Advance(in WormFrameContext context)
        {
            if (_rollbackState.IsActive)
                return AdvanceRollback(context);

            if (_reviveSequence.IsActive)
                return AdvanceRevive(context);

            return AdvanceForward(context);
        }

        public void CompleteReviveAfterRender()
        {
            _reviveSequence.CompleteAfterFinalRender(null);
        }

        private WormMovementStepResult AdvanceForward(in WormFrameContext context)
        {
            WormForwardMotionResult result = _forwardMotion.Advance(
                _pathProgress.HeadDistance,
                context.DeltaTime,
                context.Rail,
                context.ForwardMotion);
            return new WormMovementStepResult(_pathProgress.Apply(result), false);
        }

        private WormMovementStepResult AdvanceRollback(in WormFrameContext context)
        {
            WormSectionRollbackMotionResult result = _rollbackMotion.Advance(
                _pathProgress.HeadDistance,
                _segmentChain.Items,
                context.Rail.TotalLength,
                context.BaseSpeed,
                context.RollbackForwardSpeedMultiplier,
                context.RollbackSpeed,
                context.UnscaledDeltaTime);
            _pathProgress.SetHeadDistance(result.HeadDistance);

            if (!result.Completed)
                return default;

            bool pathCompleted = _pathProgress.TryComplete(
                _pathProgress.HeadDistance >= context.Rail.TotalLength);
            _rollbackState.Complete();
            return new WormMovementStepResult(pathCompleted, false);
        }

        private WormMovementStepResult AdvanceRevive(in WormFrameContext context)
        {
            WormReviveAnimationFrame frame = _reviveSequence.Advance(context.UnscaledDeltaTime);
            _pathProgress.SetHeadDistance(frame.HeadDistance);
            return new WormMovementStepResult(false, frame.Completed);
        }
    }
}
