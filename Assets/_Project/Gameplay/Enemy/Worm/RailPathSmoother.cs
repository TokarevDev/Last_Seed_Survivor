namespace Game.Gameplay.Enemy.Worm
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class RailPathSmoother
    {
        private const float CornerDistanceFraction = 0.45f;

        public static Vector3[] Build(
            Vector3[] worldPoints,
            RailPathInterpolationMode interpolationMode,
            float cornerRadius,
            int cornerSamples,
            float minimumSegmentLength)
        {
            if (interpolationMode != RailPathInterpolationMode.Smooth ||
                worldPoints.Length < 3 ||
                cornerRadius <= minimumSegmentLength)
            {
                return worldPoints;
            }

            List<Vector3> points = new(worldPoints.Length * (cornerSamples + 1));
            AddPointIfSeparated(points, worldPoints[0], minimumSegmentLength);

            for (int index = 1; index < worldPoints.Length - 1; index++)
            {
                Vector3 previous = worldPoints[index - 1];
                Vector3 corner = worldPoints[index];
                Vector3 next = worldPoints[index + 1];
                float cornerDistance = Mathf.Min(
                    cornerRadius,
                    Vector3.Distance(previous, corner) * CornerDistanceFraction,
                    Vector3.Distance(corner, next) * CornerDistanceFraction);

                if (cornerDistance <= minimumSegmentLength)
                {
                    AddPointIfSeparated(points, corner, minimumSegmentLength);
                    continue;
                }

                Vector3 entry = corner + (previous - corner).normalized * cornerDistance;
                Vector3 exit = corner + (next - corner).normalized * cornerDistance;
                AddPointIfSeparated(points, entry, minimumSegmentLength);

                for (int sample = 1; sample <= cornerSamples; sample++)
                {
                    float t = sample / (float)cornerSamples;
                    float inverseT = 1f - t;
                    Vector3 point = inverseT * inverseT * entry +
                        2f * inverseT * t * corner +
                        t * t * exit;
                    AddPointIfSeparated(points, point, minimumSegmentLength);
                }
            }

            AddPointIfSeparated(points, worldPoints[^1], minimumSegmentLength);

            if (points.Count == 1)
                points.Add(worldPoints[^1]);

            return points.ToArray();
        }

        private static void AddPointIfSeparated(
            List<Vector3> points,
            Vector3 point,
            float minimumSegmentLength)
        {
            if (points.Count > 0 &&
                Vector3.SqrMagnitude(points[^1] - point) <=
                minimumSegmentLength * minimumSegmentLength)
            {
                return;
            }

            points.Add(point);
        }
    }
}
