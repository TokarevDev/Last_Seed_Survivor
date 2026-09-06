using System.Collections.Generic;
using LastSeed.Core.World;
using UnityEngine;
using NumericVector3 = System.Numerics.Vector3;

public sealed class WormSegmentChainPresenter
{
    private const float PositionSqrMagnitudeThreshold = 0.000001f;
    private const float RotationThresholdDegrees = 0.1f;

    private int _activeStartIndex = -1;
    private int _activeEndIndex = -1;
    private Vector3 _temporaryEuler;

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

            UpdateSegmentPosition(segment, position);
            UpdateHeadFollowChain(segments, rail, index, segment, distance, layout);

            if (index > start && !segment.HasTailVisualChain)
                UpdateSegmentRotation(segments, index, segment, position);

            UpdateTailVisualChain(segments, rail, index, segment, distance, layout);
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
            UpdateSegmentPosition(segment, position);
            UpdateHeadFollowChain(segments, rail, index, segment, distance, layout);

            if (index > 0 && !segment.HasTailVisualChain)
                UpdateSegmentRotation(segments, index, segment, position);

            UpdateTailVisualChain(segments, rail, index, segment, distance, layout);
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

    private static void UpdateTailVisualChain(
        IReadOnlyList<WormSegment> segments,
        IPathSampler<NumericVector3> rail,
        int index,
        WormSegment segment,
        float tailDistance,
        in WormSegmentChainLayout layout)
    {
        if (segment == null || !segment.HasTailVisualChain)
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

        if (previousIndex >= 0 && previousIndex < segments.Count)
        {
            WormSegment previous = segments[previousIndex];

            if (ShouldAttachTailToHeadFollowChain(segments, index, tail)
                && previous != null
                && previous.TryGetLastHeadFollowPartPosition(out Vector3 headFollowPosition))
            {
                return headFollowPosition;
            }

            if (previous != null)
                return previous.CachedTransform.position;
        }

        return tail.CachedTransform.position;
    }

    private static void UpdateHeadFollowChain(
        IReadOnlyList<WormSegment> segments,
        IPathSampler<NumericVector3> rail,
        int index,
        WormSegment segment,
        float headDistance,
        in WormSegmentChainLayout layout)
    {
        if (segment == null || !segment.HasHeadFollowChain)
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

    private static void UpdateSegmentPosition(WormSegment segment, NumericVector3 position)
    {
        Transform segmentTransform = segment.CachedTransform;
        Vector3 unityPosition = ToUnity(position);

        if ((segmentTransform.position - unityPosition).sqrMagnitude >
            PositionSqrMagnitudeThreshold)
        {
            segmentTransform.position = unityPosition;
        }
    }

    private void UpdateSegmentRotation(
        IReadOnlyList<WormSegment> segments,
        int index,
        WormSegment segment,
        NumericVector3 position)
    {
        WormSegment previous = segments[index - 1];
        if (previous == null)
            return;

        if (!WormSegmentPoseCalculator.TryCalculateLookAngle(
                position,
                ToNumeric(previous.CachedTransform.position),
                out float angle))
        {
            return;
        }

        Transform visual = segment.VisualRoot;
        if (visual == null)
            return;

        Vector3 currentEuler = visual.localEulerAngles;
        if (Mathf.Abs(Mathf.DeltaAngle(currentEuler.z, angle)) <= RotationThresholdDegrees)
            return;

        _temporaryEuler.z = angle;
        visual.localEulerAngles = _temporaryEuler;
    }

    private static float GetSegmentDistance(
        IReadOnlyList<WormSegment> segments,
        IReadOnlyDictionary<WormSegment, float> rollbackAnchoredDistances,
        int index,
        WormSegment segment,
        in WormSegmentChainLayout layout)
    {
        float headFollowDistanceOffset = ShouldAttachTailToHeadFollowChain(
            segments,
            index,
            segment)
            ? GetHeadFollowChainDistanceOffset(segments, layout)
            : 0f;
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

    private static bool ShouldShowHeadFollowChain(
        IReadOnlyList<WormSegment> segments,
        int index,
        WormSegment segment)
    {
        return index == 0
            && segment != null
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

    private static float GetHeadFollowChainDistanceOffset(
        IReadOnlyList<WormSegment> segments,
        in WormSegmentChainLayout layout)
    {
        WormSegment head = segments.Count > 0 ? segments[0] : null;
        return head != null
            ? WormSegmentPoseCalculator.GetHeadFollowDistanceOffset(
                head.HeadFollowPartCount,
                layout)
            : 0f;
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
