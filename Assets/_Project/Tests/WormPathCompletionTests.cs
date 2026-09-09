using Game.Gameplay.Enemy.Worm.Movement;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class WormPathCompletionTests
    {
        [Test]
        public void FrameResult_CarriesCompleteImmutablePayload()
        {
            WormPathCompletion completion = new(
                finalDistance: 24f,
                normalizedProgress: 1f,
                reason: WormPathCompletionReason.ReachedRailEnd);
            WormFrameResult result = new(completion);

            Assert.That(result.PathCompletion.HasValue, Is.True);
            Assert.That(result.PathCompletion.Value.FinalDistance, Is.EqualTo(24f));
            Assert.That(result.PathCompletion.Value.NormalizedProgress, Is.EqualTo(1f));
            Assert.That(result.PathCompletion.Value.Reason,
                Is.EqualTo(WormPathCompletionReason.ReachedRailEnd));
        }

        [Test]
        public void FrameResult_WithoutCompletion_HasNoPayload()
        {
            WormFrameResult result = new(pathCompletion: null);

            Assert.That(result.PathCompletion.HasValue, Is.False);
        }
    }
}
