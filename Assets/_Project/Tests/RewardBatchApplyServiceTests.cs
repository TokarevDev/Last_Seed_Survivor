using System;
using System.Collections.Generic;
using NUnit.Framework;

using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;

namespace Game.Tests
{
    public sealed class RewardBatchApplyServiceTests
    {
        [Test]
        public void ApplyAll_ForwardsEveryChoiceInStableOrder()
        {
            RewardChoiceData first = CreateChoice();
            RewardChoiceData second = CreateChoice();
            FakeChoiceApplier applier = new();
            RewardBatchApplyService service = new(applier);

            service.ApplyAll(new[] { first, second });

            Assert.That(applier.Applied, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void ApplyAll_EmptyCollection_PerformsNoWork()
        {
            FakeChoiceApplier applier = new();
            RewardBatchApplyService service = new(applier);

            service.ApplyAll(Array.Empty<RewardChoiceData>());

            Assert.That(applier.Applied, Is.Empty);
        }

        [Test]
        public void ApplyAll_NullCollection_ThrowsBeforeApplyingAnything()
        {
            FakeChoiceApplier applier = new();
            RewardBatchApplyService service = new(applier);

            Assert.Throws<ArgumentNullException>(() => service.ApplyAll(null));
            Assert.That(applier.Applied, Is.Empty);
        }

        private static RewardChoiceData CreateChoice()
        {
            return new RewardChoiceData(new RewardModifierEntry());
        }

        private sealed class FakeChoiceApplier : IRewardChoiceApplier
        {
            public List<RewardChoiceData> Applied { get; } = new();

            public void Apply(RewardChoiceData choice)
            {
                Applied.Add(choice);
            }
        }
    }
}
