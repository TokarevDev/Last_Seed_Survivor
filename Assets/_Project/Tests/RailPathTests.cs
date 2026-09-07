using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LastSeed.Tests
{
    public sealed class RailPathTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void LinearPath_UsesTransformedPointsAndDistanceSampling()
        {
            GameObject pathObject = new("RailPath");

            try
            {
                pathObject.transform.position = new Vector3(10f, -2f, 0f);
                RailPath path = pathObject.AddComponent<RailPath>();
                Configure(
                    path,
                    RailPathInterpolationMode.Linear,
                    Vector3.zero,
                    new Vector3(3f, 4f, 0f));

                Vector3 midpoint = path.GetPoint(2.5f);

                Assert.That(path.TotalLength, Is.EqualTo(5f).Within(Tolerance));
                Assert.That(midpoint.x, Is.EqualTo(11.5f).Within(Tolerance));
                Assert.That(midpoint.y, Is.EqualTo(0f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(pathObject);
            }
        }

        [Test]
        public void SmoothPath_PreservesEndpointsAndMonotonicControlDistances()
        {
            GameObject pathObject = new("RailPath");

            try
            {
                RailPath path = pathObject.AddComponent<RailPath>();
                Configure(
                    path,
                    RailPathInterpolationMode.Smooth,
                    Vector3.zero,
                    new Vector3(1f, 1f, 0f),
                    new Vector3(2f, 0f, 0f));

                Vector3 start = path.GetPoint(0f);
                Vector3 end = path.GetPoint(float.MaxValue);

                Assert.That(start, Is.EqualTo(Vector3.zero));
                Assert.That(end.x, Is.EqualTo(2f).Within(Tolerance));
                Assert.That(end.y, Is.Zero.Within(Tolerance));
                Assert.That(end.z, Is.Zero.Within(Tolerance));
                Assert.That(path.TryGetControlPointDistance(0, out float first), Is.True);
                Assert.That(path.TryGetControlPointDistance(1, out float middle), Is.True);
                Assert.That(path.TryGetControlPointDistance(2, out float last), Is.True);
                Assert.That(middle, Is.GreaterThanOrEqualTo(first));
                Assert.That(last, Is.GreaterThanOrEqualTo(middle));
            }
            finally
            {
                Object.DestroyImmediate(pathObject);
            }
        }

        [Test]
        public void DuplicateDegeneratePath_RemainsQueryableWithoutInvalidValues()
        {
            GameObject pathObject = new("RailPath");

            try
            {
                RailPath path = pathObject.AddComponent<RailPath>();
                Vector3 point = new(2f, 3f, 0f);
                Configure(path, RailPathInterpolationMode.Smooth, point, point, point);

                Vector3 sampled = path.GetPoint(100f);

                Assert.That(path.TotalLength, Is.Zero.Within(Tolerance));
                Assert.That(sampled, Is.EqualTo(point));
                Assert.That(float.IsNaN(sampled.x), Is.False);
                Assert.That(float.IsNaN(sampled.y), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(pathObject);
            }
        }

        private static void Configure(
            RailPath path,
            RailPathInterpolationMode interpolationMode,
            params Vector3[] localPoints)
        {
            SerializedObject serializedPath = new(path);
            SerializedProperty points = serializedPath.FindProperty("_localPoints");
            points.arraySize = localPoints.Length;

            for (int index = 0; index < localPoints.Length; index++)
                points.GetArrayElementAtIndex(index).vector3Value = localPoints[index];

            serializedPath.FindProperty("_interpolationMode").enumValueIndex =
                (int)interpolationMode;
            serializedPath.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
