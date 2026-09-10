using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public static class RailDistanceTableBuilder
    {
        public static float[] Build(Vector3[] pathPoints, out float totalLength)
        {
            float[] distances = new float[pathPoints.Length];
            distances[0] = 0f;

            for (int index = 1; index < pathPoints.Length; index++)
            {
                distances[index] = distances[index - 1] + Vector3.Distance(
                    pathPoints[index - 1],
                    pathPoints[index]);
            }

            totalLength = distances[^1];
            return distances;
        }
    }
}
