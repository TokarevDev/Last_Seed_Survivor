using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class RailPathSerializedData
{
    private readonly RailPath _path;
    private readonly SerializedObject _serializedObject;
    private readonly SerializedProperty _localPoints;
    private readonly SerializedProperty _interpolationMode;
    private readonly SerializedProperty _cornerRadius;
    private readonly SerializedProperty _cornerSamples;
    private readonly SerializedProperty _legacyWaypoints;

    public RailPathSerializedData(RailPath path, SerializedObject serializedObject)
    {
        _path = path != null ? path : throw new ArgumentNullException(nameof(path));
        _serializedObject = serializedObject ??
            throw new ArgumentNullException(nameof(serializedObject));
        _localPoints = RequireProperty("_localPoints");
        _interpolationMode = RequireProperty("_interpolationMode");
        _cornerRadius = RequireProperty("_cornerRadius");
        _cornerSamples = RequireProperty("_cornerSamples");
        _legacyWaypoints = RequireProperty("_waypoints");
    }

    public int PointCount => _localPoints.arraySize;
    public int ChildTransformCount => _path.transform.childCount;

    public int LegacyWaypointCount
    {
        get
        {
            int count = 0;

            for (int index = 0; index < _legacyWaypoints.arraySize; index++)
            {
                if (_legacyWaypoints.GetArrayElementAtIndex(index).objectReferenceValue != null)
                    count++;
            }

            return count;
        }
    }

    public Vector3 GetWorldPoint(int index)
    {
        return _path.transform.TransformPoint(
            _localPoints.GetArrayElementAtIndex(index).vector3Value);
    }

    public void SetWorldPoint(int index, Vector3 worldPosition)
    {
        if (index < 0 || index >= PointCount)
            return;

        _localPoints.GetArrayElementAtIndex(index).vector3Value =
            _path.transform.InverseTransformPoint(worldPosition);
        Apply();
    }

    public void AddWorldPoint(Vector3 worldPosition)
    {
        int index = PointCount;
        _localPoints.InsertArrayElementAtIndex(index);
        _localPoints.GetArrayElementAtIndex(index).vector3Value =
            _path.transform.InverseTransformPoint(worldPosition);
        Apply();
    }

    public void InsertWorldPoint(int index, Vector3 worldPosition)
    {
        index = Mathf.Clamp(index, 0, PointCount);
        _localPoints.InsertArrayElementAtIndex(index);
        _localPoints.GetArrayElementAtIndex(index).vector3Value =
            _path.transform.InverseTransformPoint(worldPosition);
        Apply();
    }

    public void RemovePointAt(int index)
    {
        if (index < 0 || index >= PointCount)
            return;

        _localPoints.DeleteArrayElementAtIndex(index);
        Apply();
    }

    public void ReversePoints()
    {
        for (int left = 0, right = PointCount - 1; left < right; left++, right--)
        {
            SerializedProperty leftPoint = _localPoints.GetArrayElementAtIndex(left);
            SerializedProperty rightPoint = _localPoints.GetArrayElementAtIndex(right);
            Vector3 value = leftPoint.vector3Value;
            leftPoint.vector3Value = rightPoint.vector3Value;
            rightPoint.vector3Value = value;
        }

        Apply();
    }

    public void ClearPoints()
    {
        _localPoints.arraySize = 0;
        Apply();
    }

    public void FlattenLocalZ()
    {
        for (int index = 0; index < PointCount; index++)
        {
            SerializedProperty point = _localPoints.GetArrayElementAtIndex(index);
            Vector3 value = point.vector3Value;
            value.z = 0f;
            point.vector3Value = value;
        }

        Apply();
    }

    public int ImportLegacyWaypoints()
    {
        List<Vector3> localPoints = new(_legacyWaypoints.arraySize);

        for (int index = 0; index < _legacyWaypoints.arraySize; index++)
        {
            Transform waypoint = _legacyWaypoints
                .GetArrayElementAtIndex(index)
                .objectReferenceValue as Transform;

            if (waypoint != null)
                localPoints.Add(_path.transform.InverseTransformPoint(waypoint.position));
        }

        return ReplacePoints(localPoints);
    }

    public int ImportChildTransforms()
    {
        int childCount = _path.transform.childCount;
        List<Vector3> localPoints = new(childCount);

        for (int index = 0; index < childCount; index++)
        {
            localPoints.Add(_path.transform.InverseTransformPoint(
                _path.transform.GetChild(index).position));
        }

        return ReplacePoints(localPoints);
    }

    public void ClearLegacyWaypoints()
    {
        _legacyWaypoints.arraySize = 0;
        Apply();
    }

    public Vector3[] BuildPreviewWorldPoints()
    {
        Vector3[] worldPoints = BuildSourceWorldPoints();

        if (worldPoints.Length < 2)
            return worldPoints;

        return RailPathGeometry.BuildPathPoints(
            worldPoints,
            (RailPathInterpolationMode)_interpolationMode.enumValueIndex,
            _cornerRadius.floatValue,
            _cornerSamples.intValue,
            RailPath.MinimumSegmentLength);
    }

    private Vector3[] BuildSourceWorldPoints()
    {
        if (PointCount >= 2)
        {
            Vector3[] points = new Vector3[PointCount];

            for (int index = 0; index < points.Length; index++)
                points[index] = GetWorldPoint(index);

            return points;
        }

        List<Vector3> legacyPoints = new(_legacyWaypoints.arraySize);

        for (int index = 0; index < _legacyWaypoints.arraySize; index++)
        {
            Transform waypoint = _legacyWaypoints
                .GetArrayElementAtIndex(index)
                .objectReferenceValue as Transform;

            if (waypoint != null)
                legacyPoints.Add(waypoint.position);
        }

        return legacyPoints.ToArray();
    }

    private int ReplacePoints(IReadOnlyList<Vector3> localPoints)
    {
        if (localPoints.Count < 2)
            return 0;

        _localPoints.arraySize = localPoints.Count;

        for (int index = 0; index < localPoints.Count; index++)
            _localPoints.GetArrayElementAtIndex(index).vector3Value = localPoints[index];

        Apply();
        return localPoints.Count;
    }

    private SerializedProperty RequireProperty(string propertyName)
    {
        return _serializedObject.FindProperty(propertyName) ??
            throw new InvalidOperationException(
                $"RailPath serialized property '{propertyName}' was not found.");
    }

    private void Apply()
    {
        _serializedObject.ApplyModifiedProperties();
        _serializedObject.Update();
    }
}
