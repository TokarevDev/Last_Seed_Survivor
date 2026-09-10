using System;
using System.Numerics;
using Game.Core.World;
using Game.Gameplay.Enemy.Worm.Presentation;

namespace Game.Gameplay.Enemy.Worm.Movement
{
    public static class WormSegmentPoseCalculator
    {
        private const float MinimumSpacing = 0.01f;
        private const float DirectionSqrMagnitudeThreshold = 0.0001f;
        private const float RadiansToDegrees = 180f / MathF.PI;

        public static bool TryGetActiveRange(
            int segmentCount,
            float railLength,
            in WormSegmentChainLayout layout,
            out int startIndex,
            out int endIndex)
        {
            float spacing = Math.Max(MinimumSpacing, layout.SegmentSpacing);
            float maxDistance = railLength + layout.ActiveDistancePadding;

            startIndex = Math.Max(
                0,
                (int)Math.Ceiling((layout.HeadDistance - maxDistance) / spacing));
            endIndex = Math.Min(
                segmentCount - 1,
                (int)Math.Floor(layout.HeadDistance / spacing));

            return startIndex <= endIndex;
        }

        public static float GetSegmentDistance(
            int index,
            in WormSegmentChainLayout layout,
            float headFollowDistanceOffset,
            bool hasRollbackAnchor,
            float rollbackAnchorDistance)
        {
            float distance = layout.HeadDistance - index * layout.SegmentSpacing;
            distance -= headFollowDistanceOffset;

            return layout.IsSectionRollback && hasRollbackAnchor
                ? Math.Min(distance, rollbackAnchorDistance)
                : distance;
        }

        public static Vector3 CalculatePosition(
            IPathSampler<Vector3> rail,
            float distance,
            in WormSegmentChainLayout layout)
        {
            Vector3 position = rail.GetPoint(distance);
            float wave = MathF.Sin(distance * layout.WaveFrequency + layout.WaveTime);

            return new Vector3(
                position.X,
                position.Y + wave * layout.WaveAmplitude + layout.VerticalOffset,
                position.Z);
        }

        public static float CalculateLookAngle(in Vector3 from, in Vector3 to)
        {
            return TryCalculateLookAngle(from, to, out float angle) ? angle : 0f;
        }

        public static bool TryCalculateLookAngle(
            in Vector3 from,
            in Vector3 to,
            out float angle)
        {
            Vector3 direction = to - from;
            if (direction.LengthSquared() <= DirectionSqrMagnitudeThreshold)
            {
                angle = 0f;
                return false;
            }

            angle = MathF.Atan2(direction.Y, direction.X) * RadiansToDegrees;
            return true;
        }

        public static float GetTailVisualSpacing(in WormSegmentChainLayout layout)
        {
            return Math.Max(
                MinimumSpacing,
                layout.SegmentSpacing * layout.TailVisualSpacingMultiplier);
        }

        public static float GetHeadBridgeSpacing(in WormSegmentChainLayout layout)
        {
            return Math.Max(
                MinimumSpacing,
                layout.SegmentSpacing * layout.HeadBridgeSpacingMultiplier);
        }

        public static float GetHeadFollowDistanceOffset(
            int headFollowPartCount,
            in WormSegmentChainLayout layout)
        {
            return (headFollowPartCount + 1) * GetHeadBridgeSpacing(layout)
                - Math.Max(MinimumSpacing, layout.SegmentSpacing);
        }

        public static float GetTailVisualDistance(
            float tailDistance,
            int partIndex,
            float spacing)
        {
            return Math.Max(0f, tailDistance - partIndex * spacing);
        }

        public static float GetHeadFollowVisualDistance(
            float headDistance,
            int partIndex,
            float spacing)
        {
            return Math.Max(0f, headDistance - (partIndex + 1) * spacing);
        }
    }

}
