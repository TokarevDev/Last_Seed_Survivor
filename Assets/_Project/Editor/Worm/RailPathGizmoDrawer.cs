using UnityEditor;
using UnityEngine;

internal static class RailPathGizmoDrawer
{
    private const float PointRadius = 0.08f;
    private static readonly Color PathColor = new(0.1f, 1f, 0.25f, 0.9f);
    private static readonly Color LegacyPathColor = new(1f, 0.8f, 0.1f, 0.9f);

    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    private static void DrawSelected(RailPath path, GizmoType gizmoType)
    {
        if (path == null)
            return;

        RailPathSerializedData data = new(path, new SerializedObject(path));
        Vector3[] points = data.BuildPreviewWorldPoints();

        if (points.Length < 2)
            return;

        Gizmos.color = data.PointCount >= 2 ? PathColor : LegacyPathColor;
        Vector3 previous = points[0];
        Gizmos.DrawSphere(previous, PointRadius);

        for (int index = 1; index < points.Length; index++)
        {
            Vector3 current = points[index];
            Gizmos.DrawLine(previous, current);
            Gizmos.DrawSphere(current, PointRadius);
            previous = current;
        }
    }
}
