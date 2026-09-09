
using Game.Core.Collections;
using Game.Core.World;
using Game.Gameplay.Enemy.Worm.Movement;

namespace Game.Gameplay.Enemy.Worm
{
    using System.Collections.Generic;
    using UnityEngine;
    using NumericVector3 = System.Numerics.Vector3;

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

        private Vector3[] _worldPoints;
        private Vector3[] _samples;
        private float[] _distances;
        private float[] _controlPointDistances;
        private float _totalLength;
        private Matrix4x4 _builtLocalToWorldMatrix;
        private bool _hasBuiltTransform;
        private RailPathDefinition _definition;

        public int PointCount => _localPoints != null ? _localPoints.Count : 0;
        public float TotalLength
        {
            get
            {
                EnsureBuilt();
                return _totalLength;
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

            if (_totalLength <= MinimumSegmentLength)
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

            distance = Mathf.Clamp(distance, 0f, _totalLength);

            if (distance >= _totalLength)
                return _samples[^1];

            float fIndex = distance / _sampleStep;
            int index = Mathf.FloorToInt(fIndex);

            if (index >= _samples.Length - 1)
                return _samples[^1];

            float intervalStart = index * _sampleStep;
            float intervalEnd = Mathf.Min(intervalStart + _sampleStep, _totalLength);
            float intervalLength = intervalEnd - intervalStart;
            float t = intervalLength > MinimumSegmentLength
                ? (distance - intervalStart) / intervalLength
                : 0f;

            return Vector3.Lerp(
                _samples[index],
                _samples[index + 1],
                t);
        }

        NumericVector3 IPathSampler<NumericVector3>.GetPoint(float distance)
        {
            Vector3 point = GetPoint(distance);
            return new NumericVector3(point.x, point.y, point.z);
        }

        public float GetClosestDistance(Vector3 worldPosition)
        {
            if (!EnsureBuilt() || _samples == null || _samples.Length == 0)
                return 0f;

            return FindClosestSampleDistance(worldPosition);
        }

        private float FindClosestSampleDistance(Vector3 worldPosition)
        {
            return RailNearestPointQuery.FindSampleDistance(
                _samples,
                worldPosition,
                _sampleStep,
                _totalLength);
        }

        public bool TryGetControlPointDistance(int pointIndex, out float distance)
        {
            distance = 0f;

            if (pointIndex < 0 || !EnsureBuilt() ||
                _controlPointDistances == null ||
                pointIndex >= _controlPointDistances.Length)
            {
                return false;
            }

            distance = _controlPointDistances[pointIndex];
            return true;
        }

        public float GetControlPointProgressNormalized(float distance)
        {
            if (!EnsureBuilt() || PointCount <= 1)
                return 0f;

            float clampedDistance = Mathf.Clamp(distance, 0f, _totalLength);
            int passedPointIndex = SortedSearch.FindLastIndexAtMost(
                _controlPointDistances,
                clampedDistance + MinimumSegmentLength);

            return Mathf.Clamp01(passedPointIndex / (float)(PointCount - 1));
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
            if (_samples != null && _samples.Length > 0)
            {
                if (_hasBuiltTransform &&
                    _builtLocalToWorldMatrix == transform.localToWorldMatrix)
                {
                    return true;
                }

                Invalidate();
            }

            if (!TryBuildWorldPoints())
                return false;

            Vector3[] pathPoints = BuildPathPoints();
            if (pathPoints == null || pathPoints.Length < 2)
                return false;

            _sampleStep = _definition != null
                ? _definition.SampleStep
                : Mathf.Max(MinimumSampleStep, _sampleStep);
            CalculateDistances(pathPoints);
            BuildSamples(pathPoints);
            BuildControlPointDistances();
            _builtLocalToWorldMatrix = transform.localToWorldMatrix;
            _hasBuiltTransform = true;

            return _samples != null && _samples.Length > 0;
        }

        private bool TryBuildWorldPoints()
        {
            if (_localPoints != null && _localPoints.Count >= 2)
            {
                _definition = new RailPathDefinition(
                    _localPoints,
                    _sampleStep,
                    _interpolationMode,
                    _cornerRadius,
                    _cornerSamples);
                _worldPoints = new Vector3[_definition.PointCount];

                for (int i = 0; i < _definition.PointCount; i++)
                    _worldPoints[i] = transform.TransformPoint(_definition.GetLocalPoint(i));

                return true;
            }

            int legacyWaypointCount = CountValidLegacyWaypoints();
            if (legacyWaypointCount < 2)
                return false;

            _worldPoints = new Vector3[legacyWaypointCount];
            _definition = null;

            int pointIndex = 0;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                Transform waypoint = _waypoints[i];
                if (waypoint == null)
                    continue;

                _worldPoints[pointIndex] = waypoint.position;
                pointIndex++;
            }

            return true;
        }

        private Vector3[] BuildPathPoints()
        {
            return RailPathSmoother.Build(
                _worldPoints,
                _definition != null ? _definition.InterpolationMode : _interpolationMode,
                _definition != null ? _definition.CornerRadius : _cornerRadius,
                _definition != null ? _definition.CornerSamples : _cornerSamples,
                MinimumSegmentLength);
        }

        private void CalculateDistances(Vector3[] pathPoints)
        {
            _distances = RailDistanceTableBuilder.Build(pathPoints, out _totalLength);
        }

        private void BuildSamples(Vector3[] pathPoints)
        {
            _samples = RailSampler.Build(
                pathPoints,
                _distances,
                _totalLength,
                _sampleStep,
                MinimumSegmentLength);
        }

        private void BuildControlPointDistances()
        {
            int controlPointCount = GetAvailableControlPointCount();
            _controlPointDistances = new float[controlPointCount];
            float previousDistance = 0f;

            for (int i = 0; i < controlPointCount; i++)
            {
                if (TryGetControlPointWorldPosition(i, out Vector3 worldPosition))
                {
                    previousDistance = Mathf.Max(
                        previousDistance,
                        FindClosestSampleDistance(worldPosition));
                }

                _controlPointDistances[i] = previousDistance;
            }
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
            _worldPoints = null;
            _samples = null;
            _distances = null;
            _controlPointDistances = null;
            _totalLength = 0f;
            _builtLocalToWorldMatrix = default;
            _hasBuiltTransform = false;
            _definition = null;
        }

    }

}
