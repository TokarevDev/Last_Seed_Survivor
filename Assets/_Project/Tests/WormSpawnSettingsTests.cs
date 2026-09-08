using NUnit.Framework;

using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Spawning;

namespace Game.Tests
{
    public sealed class WormSpawnSettingsTests
    {
        [Test]
        public void Constructor_ClampsInvalidValuesAtCompositionBoundary()
        {
            WormSpawnSettings settings = new WormSpawnSettings(
                sectionCount: 0,
                poolPadding: -1,
                prewarmBatchSize: 0);

            Assert.That(settings.SectionCount, Is.EqualTo(1));
            Assert.That(settings.PoolPadding, Is.Zero);
            Assert.That(settings.PrewarmBatchSize, Is.EqualTo(1));
        }

        [Test]
        public void BodyPoolCapacity_IncludesGeneratedBodyAndPadding()
        {
            WormSpawnSettings settings = new WormSpawnSettings(
                sectionCount: 3,
                poolPadding: 7,
                prewarmBatchSize: 10);

            int expectedBodyCount = WormPatternBuilder.GetBodySegmentCount(3);
            Assert.That(settings.BodyPoolCapacity, Is.EqualTo(expectedBodyCount + 7));
        }

        [Test]
        public void BuildPattern_UsesInjectedIntegerRollsAndPreservesSegmentCount()
        {
            TestRandomSource fourPerGroup = new(fallback: 0f);
            TestRandomSource fivePerGroup = new(fallback: 0.99f);

            var fourPattern = WormPatternBuilder.BuildPattern(2, fourPerGroup);
            var fivePattern = WormPatternBuilder.BuildPattern(2, fivePerGroup);

            Assert.That(fourPattern.Count, Is.EqualTo(16));
            Assert.That(fivePattern.Count, Is.EqualTo(16));
            Assert.That(fourPattern[0].Type, Is.EqualTo(WormSegmentType.Head));
            Assert.That(fourPattern[^1].Type, Is.EqualTo(WormSegmentType.Tail));
            Assert.That(fourPerGroup.Calls, Is.EqualTo(4));
            Assert.That(fivePerGroup.Calls, Is.EqualTo(3));
        }
    }
}
