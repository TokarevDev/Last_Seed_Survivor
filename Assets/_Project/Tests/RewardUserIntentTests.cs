using System;
using Game.Gameplay.Rewards.Runtime;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class RewardUserIntentTests
    {
        [Test]
        public void Select_RequiresChoice()
        {
            Assert.Throws<ArgumentNullException>(() => RewardUserIntent.Select(null));
        }

        [TestCase(RewardUserIntentType.Reroll)]
        [TestCase(RewardUserIntentType.AdReroll)]
        [TestCase(RewardUserIntentType.TakeAll)]
        public void ActionIntent_DoesNotCarryChoice(RewardUserIntentType type)
        {
            RewardUserIntent intent = type switch
            {
                RewardUserIntentType.Reroll => RewardUserIntent.Reroll(),
                RewardUserIntentType.AdReroll => RewardUserIntent.AdReroll(),
                RewardUserIntentType.TakeAll => RewardUserIntent.TakeAll(),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            Assert.That(intent.Type, Is.EqualTo(type));
            Assert.That(intent.Choice, Is.Null);
        }
    }
}
