using System.Collections.Generic;
using Game.Core.World;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Movement;
using NumericVector3 = System.Numerics.Vector3;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public sealed class WormSegmentVisualChainPresenter
    {
        public float GetHeadFollowDistanceOffset(
            IReadOnlyList<WormSegment> segments,
            int index,
            WormSegment segment,
            in WormSegmentChainLayout layout)
        {
            if (!ShouldAttachTailToHeadFollowChain(segments, index, segment))
                return 0f;

            WormSegment head = segments[0];
            return WormSegmentPoseCalculator.GetHeadFollowDistanceOffset(
                head.HeadFollowPartCount,
                layout);
        }

        public void RenderTailVisualChain(
            IReadOnlyList<WormSegment> segments,
            IPathSampler<NumericVector3> rail,
            int index,
            WormSegment segment,
            float tailDistance,
            in WormSegmentChainLayout layout)
        {
            if (!segment.HasTailVisualChain)
                return;

            segment.ResetTailVisualRootRotation();
            float spacing = WormSegmentPoseCalculator.GetTailVisualSpacing(layout);
            NumericVector3 previousPosition = ToNumeric(
                ResolveTailLeaderPosition(segments, index, segment));

            for (int partIndex = 0; partIndex < segment.TailVisualPartCount; partIndex++)
            {
                float visualDistance = WormSegmentPoseCalculator.GetTailVisualDistance(
                    tailDistance,
                    partIndex,
                    spacing);
                NumericVector3 visualPosition = WormSegmentPoseCalculator.CalculatePosition(
                    rail,
                    visualDistance,
                    layout);
                float angle = WormSegmentPoseCalculator.CalculateLookAngle(
                    visualPosition,
                    previousPosition);
                segment.SetTailVisualPartPose(partIndex, ToUnity(visualPosition), angle);
                previousPosition = visualPosition;
            }
        }

        private static Vector3 ResolveTailLeaderPosition(
            IReadOnlyList<WormSegment> segments,
            int index,
            WormSegment tail)
        {
            int previousIndex = index - 1;
            if (previousIndex < 0 || previousIndex >= segments.Count)
                return tail.CachedTransform.position;

            WormSegment previous = segments[previousIndex];
            if (ShouldAttachTailToHeadFollowChain(segments, index, tail)
                && previous != null
                && previous.TryGetLastHeadFollowPartPosition(out Vector3 headFollowPosition))
            {
                return headFollowPosition;
            }

            return previous != null
                ? previous.CachedTransform.position
                : tail.CachedTransform.position;
        }

        public void RenderHeadFollowChain(
            IReadOnlyList<WormSegment> segments,
            IPathSampler<NumericVector3> rail,
            int index,
            WormSegment segment,
            float headDistance,
            in WormSegmentChainLayout layout)
        {
            if (!segment.HasHeadFollowChain)
                return;

            bool visible = ShouldShowHeadFollowChain(segments, index, segment);
            segment.SetHeadFollowChainVisible(visible);

            if (!visible)
                return;

            float spacing = WormSegmentPoseCalculator.GetHeadBridgeSpacing(layout);
            NumericVector3 previousPosition = ToNumeric(segment.CachedTransform.position);

            for (int partIndex = 0; partIndex < segment.HeadFollowPartCount; partIndex++)
            {
                float visualDistance = WormSegmentPoseCalculator.GetHeadFollowVisualDistance(
                    headDistance,
                    partIndex,
                    spacing);
                NumericVector3 visualPosition = WormSegmentPoseCalculator.CalculatePosition(
                    rail,
                    visualDistance,
                    layout);
                float angle = WormSegmentPoseCalculator.CalculateLookAngle(
                    visualPosition,
                    previousPosition);
                segment.SetHeadFollowPartPose(partIndex, ToUnity(visualPosition), angle);
                previousPosition = visualPosition;
            }
        }

        private static bool ShouldShowHeadFollowChain(
            IReadOnlyList<WormSegment> segments,
            int index,
            WormSegment segment)
        {
            return index == 0
                && segment.Type == WormSegmentType.Head
                && segment.HasHeadFollowChain
                && segments.Count == 2
                && segments[1] != null
                && segments[1].Type == WormSegmentType.Tail;
        }

        private static bool ShouldAttachTailToHeadFollowChain(
            IReadOnlyList<WormSegment> segments,
            int index,
            WormSegment segment)
        {
            return index == 1
                && segment != null
                && segment.Type == WormSegmentType.Tail
                && segments.Count == 2
                && segments[0] != null
                && segments[0].HasHeadFollowChain;
        }

        private static NumericVector3 ToNumeric(Vector3 value)
        {
            return new NumericVector3(value.x, value.y, value.z);
        }

        private static Vector3 ToUnity(in NumericVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
    }

}
