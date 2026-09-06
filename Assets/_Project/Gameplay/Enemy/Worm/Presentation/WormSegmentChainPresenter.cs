using System;
using System.Collections.Generic;
using LastSeed.Core.World;
using UnityEngine;
using NumericVector3 = System.Numerics.Vector3;

public sealed class WormSegmentChainPresenter
{
    private const float PositionSqrMagnitudeThreshold = 0.000001f;
    private const float RotationThresholdDegrees = 0.1f;

    private readonly WormSegmentVisualChainPresenter _visualChainPresenter;
    private int _activeStartIndex = -1;
    private int _activeEndIndex = -1;
    private Vector3 _temporaryEuler;

    public WormSegmentChainPresenter(WormSegmentVisualChainPresenter visualChainPresenter)
    {
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

            UpdateSegmentPosition(segment, position);
            _visualChainPresenter.RenderHeadFollowChain(
                segments,
                rail,
                index,
                segment,
                distance,
                layout);

            if (index > start && !segment.HasTailVisualChain)
                UpdateSegmentRotation(segments, index, segment, position);

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
            UpdateSegmentPosition(segment, position);
            _visualChainPresenter.RenderHeadFollowChain(
                segments,
                rail,
                index,
                segment,
                distance,
                layout);

            if (index > 0 && !segment.HasTailVisualChain)
                UpdateSegmentRotation(segments, index, segment, position);

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

    private static NumericVector3 ToNumeric(Vector3 value)
    {
        return new NumericVector3(value.x, value.y, value.z);
    }

    private static Vector3 ToUnity(in NumericVector3 value)
    {
        return new Vector3(value.X, value.Y, value.Z);
    }
}
