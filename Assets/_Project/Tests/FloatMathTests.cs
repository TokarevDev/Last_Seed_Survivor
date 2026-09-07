using System;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class FloatMathTests
    {
        [TestCase(-1f, 0f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(2f, 1f)]
        public void Clamp01_ConstrainsValueToNormalizedRange(float value, float expected)
        {
            Assert.That(FloatMath.Clamp01(value), Is.EqualTo(expected));
        }

        [Test]
        public void Clamp_WithInvalidRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => FloatMath.Clamp(0f, 2f, 1f));
        }

        [Test]
        public void ClampOrMidpoint_WithInvertedRange_ReturnsRangeMidpoint()
        {
            Assert.That(FloatMath.ClampOrMidpoint(100f, 4f, 2f), Is.EqualTo(3f));
        }

        [TestCase(-1f, 10f)]
        [TestCase(0.5f, 15f)]
        [TestCase(2f, 20f)]
        public void Lerp_UsesClampedInterpolation(float time, float expected)
        {
            Assert.That(FloatMath.Lerp(10f, 20f, time), Is.EqualTo(expected));
        }

        [TestCase(0f, 0f)]
        [TestCase(15f, 0.5f)]
        [TestCase(30f, 1f)]
        public void InverseLerp_ReturnsNormalizedClampedPosition(float value, float expected)
        {
            Assert.That(FloatMath.InverseLerp(10f, 20f, value), Is.EqualTo(expected));
        }

        [Test]
        public void InverseLerp_DegenerateRange_ReturnsZero()
        {
            Assert.That(FloatMath.InverseLerp(5f, 5f, 10f), Is.Zero);
        }
    }
}
