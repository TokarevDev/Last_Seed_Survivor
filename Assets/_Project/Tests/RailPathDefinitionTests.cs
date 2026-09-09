using Game.Gameplay.Enemy.Worm;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class RailPathDefinitionTests
    {
        [Test]
        public void Constructor_CopiesPointsAndNormalizesSamplingSettings()
        {
            Vector3[] source = { Vector3.zero, Vector3.right };
            RailPathDefinition definition = new(
                source,
                0f,
                RailPathInterpolationMode.Smooth,
                -1f,
                100);

            source[0] = Vector3.up;

            Assert.That(definition.GetLocalPoint(0), Is.EqualTo(Vector3.zero));
            Assert.That(definition.SampleStep, Is.EqualTo(RailPath.MinimumSampleStep));
            Assert.That(definition.CornerRadius, Is.Zero);
            Assert.That(definition.CornerSamples, Is.EqualTo(RailPath.MaximumCornerSamples));
        }

        [Test]
        public void Constructor_RejectsIncompletePath()
        {
            Assert.That(
                () => new RailPathDefinition(
                    new[] { Vector3.zero },
                    0.1f,
                    RailPathInterpolationMode.Linear,
                    0f,
                    2),
                Throws.ArgumentException);
        }
    }
}
