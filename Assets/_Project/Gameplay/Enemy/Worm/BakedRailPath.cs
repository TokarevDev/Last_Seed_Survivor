using System;
using Game.Core.Collections;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public sealed class BakedRailPath
    {
        private readonly Vector3[] _samples;
        private readonly float[] _controlPointDistances;
        private readonly float _sampleStep;

        public BakedRailPath(
            Vector3[] samples,
            float[] controlPointDistances,
            float sampleStep,
            float totalLength)
        {
            if (samples == null)
                throw new ArgumentNullException(nameof(samples));

            if (samples.Length == 0)
                throw new ArgumentException(
                    "A baked rail path requires at least one sample.",
                    nameof(samples));

            if (controlPointDistances == null)
                throw new ArgumentNullException(nameof(controlPointDistances));

            _samples = (Vector3[])samples.Clone();
            _controlPointDistances = (float[])controlPointDistances.Clone();
            _sampleStep = Mathf.Max(RailPath.MinimumSampleStep, sampleStep);
            TotalLength = Mathf.Max(0f, totalLength);
        }

        public float TotalLength { get; }

        public Vector3 GetPoint(float distance)
        {
            distance = Mathf.Clamp(distance, 0f, TotalLength);

            if (distance >= TotalLength)
                return _samples[^1];

            int index = Mathf.FloorToInt(distance / _sampleStep);
            if (index >= _samples.Length - 1)
                return _samples[^1];

            float intervalStart = index * _sampleStep;
            float intervalEnd = Mathf.Min(intervalStart + _sampleStep, TotalLength);
            float intervalLength = intervalEnd - intervalStart;
            float interpolation = intervalLength > RailPath.MinimumSegmentLength
                ? (distance - intervalStart) / intervalLength
                : 0f;

            return Vector3.Lerp(
                _samples[index],
                _samples[index + 1],
                interpolation);
        }

        public float GetClosestDistance(Vector3 worldPosition)
        {
            return RailNearestPointQuery.FindSampleDistance(
                _samples,
                worldPosition,
                _sampleStep,
                TotalLength);
        }

        public bool TryGetControlPointDistance(int pointIndex, out float distance)
        {
            distance = 0f;

            if (pointIndex < 0 || pointIndex >= _controlPointDistances.Length)
                return false;

            distance = _controlPointDistances[pointIndex];
            return true;
        }

        public float GetControlPointProgressNormalized(float distance)
        {
            if (_controlPointDistances.Length <= 1)
                return 0f;

            float clampedDistance = Mathf.Clamp(distance, 0f, TotalLength);
            int passedPointIndex = SortedSearch.FindLastIndexAtMost(
                _controlPointDistances,
                clampedDistance + RailPath.MinimumSegmentLength);

            return Mathf.Clamp01(
                passedPointIndex / (float)(_controlPointDistances.Length - 1));
        }
    }
}
