namespace Game.Gameplay.Enemy.Worm
{
    using UnityEngine;

    public static class RailNearestPointQuery
    {
        public static float FindSampleDistance(
            Vector3[] samples,
            Vector3 worldPosition,
            float sampleStep,
            float totalLength)
        {
            if (samples == null || samples.Length == 0)
                return 0f;

            int closestIndex = 0;
            float closestSqrDistance = float.MaxValue;

            for (int index = 0; index < samples.Length; index++)
            {
                float sqrDistance = Vector3.SqrMagnitude(samples[index] - worldPosition);
                if (sqrDistance >= closestSqrDistance)
                    continue;

                closestSqrDistance = sqrDistance;
                closestIndex = index;
            }

            return Mathf.Clamp(closestIndex * sampleStep, 0f, totalLength);
        }
    }
}
