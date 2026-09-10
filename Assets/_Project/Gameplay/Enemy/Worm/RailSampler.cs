using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public static class RailSampler
    {
        public static Vector3[] Build(
            Vector3[] pathPoints,
            float[] distances,
            float totalLength,
            float sampleStep,
            float minimumSegmentLength)
        {
            if (totalLength <= minimumSegmentLength)
                return new[] { pathPoints[0], pathPoints[^1] };

            int count = Mathf.Max(2, Mathf.CeilToInt(totalLength / sampleStep) + 1);
            Vector3[] samples = new Vector3[count];

            for (int index = 0; index < count; index++)
            {
                float distance = Mathf.Min(index * sampleStep, totalLength);
                samples[index] = GetPoint(
                    pathPoints,
                    distances,
                    totalLength,
                    distance,
                    minimumSegmentLength);
            }

            return samples;
        }

        private static Vector3 GetPoint(
            Vector3[] pathPoints,
            float[] distances,
            float totalLength,
            float distance,
            float minimumSegmentLength)
        {
            distance = Mathf.Clamp(distance, 0f, totalLength);

            for (int index = 1; index < distances.Length; index++)
            {
                if (distance > distances[index])
                    continue;

                float segmentLength = distances[index] - distances[index - 1];
                if (segmentLength <= minimumSegmentLength)
                    return pathPoints[index];

                float t = (distance - distances[index - 1]) / segmentLength;
                return Vector3.Lerp(pathPoints[index - 1], pathPoints[index], t);
            }

            return pathPoints[^1];
        }
    }
}
