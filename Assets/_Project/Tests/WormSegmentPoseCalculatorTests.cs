using System;
using System.Numerics;
using LastSeed.Core.World;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class WormSegmentPoseCalculatorTests
    {
        [Test]
        public void TryGetActiveRange_ReturnsOnlyIndicesInsideRailWindow()
        {
            WormSegmentChainLayout layout = CreateLayout(
                headDistance: 5f,
                segmentSpacing: 1f,
                activeDistancePadding: 0f);

            bool found = WormSegmentPoseCalculator.TryGetActiveRange(
                segmentCount: 6,
                railLength: 3f,
                layout,
                out int startIndex,
                out int endIndex);

            Assert.That(found, Is.True);
            Assert.That(startIndex, Is.EqualTo(2));
            Assert.That(endIndex, Is.EqualTo(5));
        }

        [Test]
        public void CalculatePosition_CombinesPurePathSampleWaveAndVerticalOffset()
        {
            WormSegmentChainLayout layout = new(
                headDistance: 0f,
                segmentSpacing: 1f,
                tailVisualSpacingMultiplier: 1f,
                headBridgeSpacingMultiplier: 1f,
                activeDistancePadding: 0f,
                waveAmplitude: 0.5f,
                waveFrequency: 0f,
                waveTime: MathF.PI * 0.5f,
                verticalOffset: 1f,
                isSectionRollback: false,
                isReviveRollback: false);

            Vector3 position = WormSegmentPoseCalculator.CalculatePosition(
                new LinearPathSampler(),
                distance: 4f,
                layout);

            Assert.That(position.X, Is.EqualTo(4f));
            Assert.That(position.Y, Is.EqualTo(3.5f).Within(0.0001f));
            Assert.That(position.Z, Is.EqualTo(3f));
        }

        [Test]
        public void GetSegmentDistance_AppliesBridgeAndRollbackAnchor()
        {
            WormSegmentChainLayout layout = new(
                headDistance: 10f,
                segmentSpacing: 1f,
                tailVisualSpacingMultiplier: 1f,
                headBridgeSpacingMultiplier: 1f,
                activeDistancePadding: 0f,
                waveAmplitude: 0f,
                waveFrequency: 0f,
                waveTime: 0f,
                verticalOffset: 0f,
                isSectionRollback: true,
                isReviveRollback: false);

            float distance = WormSegmentPoseCalculator.GetSegmentDistance(
                index: 2,
                layout,
                headFollowDistanceOffset: 3f,
                hasRollbackAnchor: true,
                rollbackAnchorDistance: 4f);

            Assert.That(distance, Is.EqualTo(4f));
        }

        [Test]
        public void TryCalculateLookAngle_RejectsCoincidentPointsAndCalculatesDirection()
        {
            Vector3 origin = Vector3.Zero;

            bool hasCoincidentAngle = WormSegmentPoseCalculator.TryCalculateLookAngle(
                origin,
                origin,
                out _);
            bool hasDirectionAngle = WormSegmentPoseCalculator.TryCalculateLookAngle(
                origin,
                Vector3.UnitY,
                out float angle);

            Assert.That(hasCoincidentAngle, Is.False);
            Assert.That(hasDirectionAngle, Is.True);
            Assert.That(angle, Is.EqualTo(90f).Within(0.0001f));
        }

        private static WormSegmentChainLayout CreateLayout(
            float headDistance,
            float segmentSpacing,
            float activeDistancePadding)
        {
            return new WormSegmentChainLayout(
                headDistance,
                segmentSpacing,
                tailVisualSpacingMultiplier: 1f,
                headBridgeSpacingMultiplier: 1f,
                activeDistancePadding,
                waveAmplitude: 0f,
                waveFrequency: 0f,
                waveTime: 0f,
                verticalOffset: 0f,
                isSectionRollback: false,
                isReviveRollback: false);
        }

        private sealed class LinearPathSampler : IPathSampler<Vector3>
        {
            public float TotalLength => 10f;

            public Vector3 GetPoint(float distance)
            {
                return new Vector3(distance, 2f, 3f);
            }
        }
    }
}
