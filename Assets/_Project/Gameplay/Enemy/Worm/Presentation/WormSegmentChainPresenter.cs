using System;
using System.Collections.Generic;
using Game.Core.World;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Movement;
using NumericVector3 = System.Numerics.Vector3;

namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public sealed class WormSegmentChainPresenter
    {
        private readonly WormSegmentTransformPresenter _transformPresenter;
        private readonly WormSegmentVisualChainPresenter _visualChainPresenter;
        private int _activeStartIndex = -1;
        private int _activeEndIndex = -1;

        public WormSegmentChainPresenter(
            WormSegmentTransformPresenter transformPresenter,
            WormSegmentVisualChainPresenter visualChainPresenter)
        {
            _transformPresenter = transformPresenter ??
                throw new ArgumentNullException(nameof(transformPresenter));
            _visualChainPresenter = visualChainPresenter ??
                throw new ArgumentNullException(nameof(visualChainPresenter));
        }

        public void Reset()
        {
            _activeStartIndex = -1;
            _activeEndIndex = -1;
        }

        public void Render(
            IReadOnlyList<WormSegment> segments,
            IPathSampler<NumericVector3> rail,
            IReadOnlyDictionary<WormSegment, float> rollbackAnchoredDistances,
            in WormSegmentChainLayout layout)
        {
            if (segments == null || segments.Count == 0 || rail == null)
                return;

            if (layout.IsSectionRollback || layout.IsReviveRollback)
            {
                RenderDuringRollback(segments, rail, rollbackAnchoredDistances, layout);
                return;
            }

            if (!WormSegmentPoseCalculator.TryGetActiveRange(
                    segments.Count,
                    rail.TotalLength,
                    layout,
                    out int start,
                    out int end))
            {
                HidePreviousActiveRange(segments, -1, -1);
                return;
            }

            HidePreviousActiveRange(segments, start, end);

            for (int index = start; index <= end; index++)
            {
                WormSegment segment = segments[index];
                if (segment == null)
                    continue;

                float distance = GetSegmentDistance(
                    segments,
                    rollbackAnchoredDistances,
                    index,
                    segment,
                    layout);
                NumericVector3 position = WormSegmentPoseCalculator.CalculatePosition(
                    rail,
                    distance,
                    layout);

                _transformPresenter.ApplyPosition(segment, position);
                _visualChainPresenter.RenderHeadFollowChain(
                    segments,
                    rail,
                    index,
                    segment,
                    distance,
                    layout);

                if (index > start && !segment.HasTailVisualChain)
                    _transformPresenter.ApplyRotation(segments, index, segment, position);

                _visualChainPresenter.RenderTailVisualChain(
                    segments,
                    rail,
                    index,
                    segment,
                    distance,
                    layout);
                segment.SetRuntimeVisible(true);
                segment.UpdateCocoonPresentation();
            }

            _activeStartIndex = start;
            _activeEndIndex = end;
        }

        private void RenderDuringRollback(
            IReadOnlyList<WormSegment> segments,
            IPathSampler<NumericVector3> rail,
            IReadOnlyDictionary<WormSegment, float> rollbackAnchoredDistances,
            in WormSegmentChainLayout layout)
        {
            float maxDistance = rail.TotalLength + layout.ActiveDistancePadding;

            for (int index = 0; index < segments.Count; index++)
            {
                WormSegment segment = segments[index];
                if (segment == null)
                    continue;

                float distance = GetSegmentDistance(
                    segments,
                    rollbackAnchoredDistances,
                    index,
                    segment,
                    layout);

                if (distance < 0f || distance > maxDistance)
                {
                    segment.SetRuntimeVisible(false);
                    continue;
                }

                NumericVector3 position = WormSegmentPoseCalculator.CalculatePosition(
                    rail,
                    distance,
                    layout);
                _transformPresenter.ApplyPosition(segment, position);
                _visualChainPresenter.RenderHeadFollowChain(
                    segments,
                    rail,
                    index,
                    segment,
                    distance,
                    layout);

                if (index > 0 && !segment.HasTailVisualChain)
                    _transformPresenter.ApplyRotation(segments, index, segment, position);

                _visualChainPresenter.RenderTailVisualChain(
                    segments,
                    rail,
                    index,
                    segment,
                    distance,
                    layout);
                segment.SetRuntimeVisible(true);
                segment.UpdateCocoonPresentation();
            }

            Reset();
        }

        private void HidePreviousActiveRange(
            IReadOnlyList<WormSegment> segments,
            int nextStartIndex,
            int nextEndIndex)
        {
            if (_activeStartIndex < 0 || _activeEndIndex < _activeStartIndex)
                return;

            for (int index = _activeStartIndex; index <= _activeEndIndex; index++)
            {
                if (index >= nextStartIndex && index <= nextEndIndex)
                    continue;

                if (index < 0 || index >= segments.Count)
                    continue;

                WormSegment segment = segments[index];
                if (segment != null)
                    segment.SetRuntimeVisible(false);
            }
        }

        private float GetSegmentDistance(
            IReadOnlyList<WormSegment> segments,
            IReadOnlyDictionary<WormSegment, float> rollbackAnchoredDistances,
            int index,
            WormSegment segment,
            in WormSegmentChainLayout layout)
        {
            float headFollowDistanceOffset = _visualChainPresenter.GetHeadFollowDistanceOffset(
                segments,
                index,
                segment,
                layout);
            float anchoredDistance = 0f;
            bool hasRollbackAnchor = layout.IsSectionRollback
                && segment != null
                && rollbackAnchoredDistances.TryGetValue(segment, out anchoredDistance);

            return WormSegmentPoseCalculator.GetSegmentDistance(
                index,
                layout,
                headFollowDistanceOffset,
                hasRollbackAnchor,
                anchoredDistance);
        }
    }

}
