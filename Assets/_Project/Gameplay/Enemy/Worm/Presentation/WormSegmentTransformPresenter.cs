using System.Collections.Generic;
using UnityEngine;
using NumericVector3 = System.Numerics.Vector3;

public sealed class WormSegmentTransformPresenter
{
    private const float PositionSqrMagnitudeThreshold = 0.000001f;
    private const float RotationThresholdDegrees = 0.1f;

    private Vector3 _temporaryEuler;

    public void ApplyPosition(WormSegment segment, in NumericVector3 position)
    {
        Transform segmentTransform = segment.CachedTransform;
        Vector3 unityPosition = ToUnity(position);

        if ((segmentTransform.position - unityPosition).sqrMagnitude <=
            PositionSqrMagnitudeThreshold)
        {
            return;
        }

        segmentTransform.position = unityPosition;
    }

    public void ApplyRotation(
        IReadOnlyList<WormSegment> segments,
        int index,
        WormSegment segment,
        in NumericVector3 position)
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

    private static NumericVector3 ToNumeric(Vector3 value)
    {
        return new NumericVector3(value.x, value.y, value.z);
    }

    private static Vector3 ToUnity(in NumericVector3 value)
    {
        return new Vector3(value.X, value.Y, value.Z);
    }
}
