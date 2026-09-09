namespace Game.Gameplay.Enemy.Worm
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class RailPathDefinition
    {
        private readonly Vector3[] _localPoints;

        public RailPathDefinition(
            IReadOnlyList<Vector3> localPoints,
            float sampleStep,
            RailPathInterpolationMode interpolationMode,
            float cornerRadius,
            int cornerSamples)
        {
            if (localPoints == null)
                throw new ArgumentNullException(nameof(localPoints));

            if (localPoints.Count < 2)
                throw new ArgumentException(
                    "Rail path requires at least two control points.",
                    nameof(localPoints));

            _localPoints = new Vector3[localPoints.Count];

            for (int index = 0; index < localPoints.Count; index++)
                _localPoints[index] = localPoints[index];

            SampleStep = Mathf.Max(RailPath.MinimumSampleStep, sampleStep);
            InterpolationMode = interpolationMode;
            CornerRadius = Mathf.Max(0f, cornerRadius);
            CornerSamples = Mathf.Clamp(
                cornerSamples,
                RailPath.MinimumCornerSamples,
                RailPath.MaximumCornerSamples);
        }

        public int PointCount => _localPoints.Length;
        public float SampleStep { get; }
        public RailPathInterpolationMode InterpolationMode { get; }
        public float CornerRadius { get; }
        public int CornerSamples { get; }

        public Vector3 GetLocalPoint(int index)
        {
            if (index < 0 || index >= _localPoints.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _localPoints[index];
        }
    }
}
