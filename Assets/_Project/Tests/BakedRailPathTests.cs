using Game.Gameplay.Enemy.Worm;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class BakedRailPathTests
    {
        private const float Tolerance = 0.001f;

        [Test]
        public void Queries_UseImmutableBakedData()
        {
            Vector3[] samples =
            {
                Vector3.zero,
                Vector3.right,
                Vector3.right * 2f
            };
            float[] controlPointDistances = { 0f, 2f };
            BakedRailPath path = new(
                samples,
                controlPointDistances,
                sampleStep: 1f,
                totalLength: 2f);

            samples[1] = Vector3.up * 100f;
            controlPointDistances[1] = 0f;

            Assert.That(path.GetPoint(0.5f).x, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(path.GetClosestDistance(new Vector3(1.1f, 0f, 0f)),
                Is.EqualTo(1f).Within(Tolerance));
            Assert.That(path.GetControlPointProgressNormalized(1f), Is.Zero);
            Assert.That(path.GetControlPointProgressNormalized(2f), Is.EqualTo(1f));
        }

        [Test]
        public void TryGetControlPointDistance_RejectsOutOfRangeIndex()
        {
            BakedRailPath path = new(
                new[] { Vector3.zero },
                new[] { 0f },
                sampleStep: 1f,
                totalLength: 0f);

            Assert.That(path.TryGetControlPointDistance(-1, out _), Is.False);
            Assert.That(path.TryGetControlPointDistance(1, out _), Is.False);
            Assert.That(path.TryGetControlPointDistance(0, out float distance), Is.True);
            Assert.That(distance, Is.Zero);
        }
    }
}
