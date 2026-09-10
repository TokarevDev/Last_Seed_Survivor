using System.Collections.Generic;
using Game.Core.World;
using Game.Gameplay.Enemy.Worm.Movement;
using NumericVector3 = System.Numerics.Vector3;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public enum RailPathInterpolationMode
    {
        Linear = 0,
        Smooth = 1
    }

    [DisallowMultipleComponent]
    public sealed class RailPath : MonoBehaviour, IWormRailPath, IPathSampler<NumericVector3>
    {
        private const float DefaultSampleStep = 0.1f;
        public const float MinimumSampleStep = 0.01f;
        public const int MinimumCornerSamples = 2;
        public const int MaximumCornerSamples = 16;
        public const float MinimumSegmentLength = 0.0001f;

        [SerializeField] private List<Vector3> _localPoints = new();
        [SerializeField][Min(MinimumSampleStep)] private float _sampleStep = DefaultSampleStep;
        [SerializeField] private RailPathInterpolationMode _interpolationMode = RailPathInterpolationMode.Linear;

        [Header("Smoothing")]
        [SerializeField][Min(0f)] private float _cornerRadius = 0.35f;
        [SerializeField][Range(MinimumCornerSamples, MaximumCornerSamples)] private int _cornerSamples = 6;

        [SerializeField][HideInInspector] private Transform[] _waypoints;

        private BakedRailPath _runtimePath;
        private Matrix4x4 _builtLocalToWorldMatrix;
        private bool _hasBuiltTransform;

        public int PointCount => _localPoints != null ? _localPoints.Count : 0;
        public float TotalLength
        {
            get
            {
                EnsureBuilt();
                return _runtimePath?.TotalLength ?? 0f;
            }
        }

        private void Reset()
        {
            _localPoints = new List<Vector3>
        {
            new(-1f, 0f, 0f),
            new(1f, 0f, 0f)
        };

            Invalidate();
        }

        private void Awake()
        {
            if (!EnsureBuilt())
            {
                Debug.LogError("RailPath requires at least 2 points.", this);
                return;
            }

            if (_runtimePath.TotalLength <= MinimumSegmentLength)
                Debug.LogError("RailPath total length must be greater than zero.", this);
        }

        private void OnValidate()
        {
            if (_localPoints == null)
                _localPoints = new List<Vector3>();

            if (_sampleStep < MinimumSampleStep)
                _sampleStep = DefaultSampleStep;

            if (_cornerRadius < 0f)
                _cornerRadius = 0f;

            _cornerSamples = Mathf.Clamp(
                _cornerSamples,
                MinimumCornerSamples,
                MaximumCornerSamples);

            Invalidate();
        }

        public Vector3 GetPoint(float distance)
        {
            if (!EnsureBuilt())
                return transform.position;

            return _runtimePath.GetPoint(distance);
        }

        NumericVector3 IPathSampler<NumericVector3>.GetPoint(float distance)
        {
            Vector3 point = GetPoint(distance);
            return new NumericVector3(point.x, point.y, point.z);
        }

        public float GetClosestDistance(Vector3 worldPosition)
        {
            if (!EnsureBuilt())
                return 0f;

            return _runtimePath.GetClosestDistance(worldPosition);
        }

        public bool TryGetControlPointDistance(int pointIndex, out float distance)
        {
            if (!EnsureBuilt())
            {
                distance = 0f;
                return false;
            }

            return _runtimePath.TryGetControlPointDistance(pointIndex, out distance);
        }

        public float GetControlPointProgressNormalized(float distance)
        {
            if (!EnsureBuilt())
                return 0f;

            return _runtimePath.GetControlPointProgressNormalized(distance);
        }

        public bool TryGetControlPointWorldPosition(int pointIndex, out Vector3 worldPosition)
        {
            worldPosition = default;

            if (pointIndex < 0)
                return false;

            if (_localPoints != null && _localPoints.Count >= 2)
            {
                if (pointIndex >= _localPoints.Count)
                    return false;

                worldPosition = transform.TransformPoint(_localPoints[pointIndex]);
                return true;
            }

            if (_waypoints == null)
                return false;

            int validPointIndex = 0;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                Transform waypoint = _waypoints[i];
                if (waypoint == null)
                    continue;

                if (validPointIndex == pointIndex)
                {
                    worldPosition = waypoint.position;
                    return true;
                }

                validPointIndex++;
            }

            return false;
        }

        private bool EnsureBuilt()
        {
            if (_runtimePath != null)
            {
                if (_hasBuiltTransform &&
                    _builtLocalToWorldMatrix == transform.localToWorldMatrix)
                {
                    return true;
                }

                Invalidate();
            }

            if (!TryBuildWorldPoints(
                    out Vector3[] worldPoints,
                    out RailPathDefinition definition))
                return false;

            Vector3[] pathPoints = BuildPathPoints(worldPoints, definition);
            if (pathPoints == null || pathPoints.Length < 2)
                return false;

            _sampleStep = definition != null
                ? definition.SampleStep
                : Mathf.Max(MinimumSampleStep, _sampleStep);
            float[] distances = RailDistanceTableBuilder.Build(
                pathPoints,
                out float totalLength);
            Vector3[] samples = RailSampler.Build(
                pathPoints,
                distances,
                totalLength,
                _sampleStep,
                MinimumSegmentLength);
            float[] controlPointDistances = BuildControlPointDistances(
                samples,
                totalLength);
            _runtimePath = new BakedRailPath(
                samples,
                controlPointDistances,
                _sampleStep,
                totalLength);
            _builtLocalToWorldMatrix = transform.localToWorldMatrix;
            _hasBuiltTransform = true;

            return true;
        }

        private bool TryBuildWorldPoints(
            out Vector3[] worldPoints,
            out RailPathDefinition definition)
        {
            if (_localPoints != null && _localPoints.Count >= 2)
            {
                definition = new RailPathDefinition(
                    _localPoints,
                    _sampleStep,
                    _interpolationMode,
                    _cornerRadius,
                    _cornerSamples);
                worldPoints = new Vector3[definition.PointCount];

                for (int i = 0; i < definition.PointCount; i++)
                    worldPoints[i] = transform.TransformPoint(definition.GetLocalPoint(i));

                return true;
            }

            int legacyWaypointCount = CountValidLegacyWaypoints();
            if (legacyWaypointCount < 2)
            {
                worldPoints = null;
                definition = null;
                return false;
            }

            worldPoints = new Vector3[legacyWaypointCount];
            definition = null;

            int pointIndex = 0;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                Transform waypoint = _waypoints[i];
                if (waypoint == null)
                    continue;

                worldPoints[pointIndex] = waypoint.position;
                pointIndex++;
            }

            return true;
        }

        private Vector3[] BuildPathPoints(
            Vector3[] worldPoints,
            RailPathDefinition definition)
        {
            return RailPathSmoother.Build(
                worldPoints,
                definition != null ? definition.InterpolationMode : _interpolationMode,
                definition != null ? definition.CornerRadius : _cornerRadius,
                definition != null ? definition.CornerSamples : _cornerSamples,
                MinimumSegmentLength);
        }

        private float[] BuildControlPointDistances(
            Vector3[] samples,
            float totalLength)
        {
            int controlPointCount = GetAvailableControlPointCount();
            float[] controlPointDistances = new float[controlPointCount];
            float previousDistance = 0f;

            for (int i = 0; i < controlPointCount; i++)
            {
                if (TryGetControlPointWorldPosition(i, out Vector3 worldPosition))
                {
                    previousDistance = Mathf.Max(
                        previousDistance,
                        RailNearestPointQuery.FindSampleDistance(
                            samples,
                            worldPosition,
                            _sampleStep,
                            totalLength));
                }

                controlPointDistances[i] = previousDistance;
            }

            return controlPointDistances;
        }

        private int GetAvailableControlPointCount()
        {
            return _localPoints != null && _localPoints.Count >= 2
                ? _localPoints.Count
                : CountValidLegacyWaypoints();
        }

        private int CountValidLegacyWaypoints()
        {
            if (_waypoints == null)
                return 0;

            int count = 0;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] != null)
                    count++;
            }

            return count;
        }

        private void Invalidate()
        {
            _runtimePath = null;
            _builtLocalToWorldMatrix = default;
            _hasBuiltTransform = false;
        }

    }

}
