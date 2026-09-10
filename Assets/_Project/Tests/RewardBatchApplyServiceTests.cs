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

        [Test]
        public void ApplyAll_WhenChoiceFails_AttemptsRemainingChoicesAndReportsFailure()
        {
            RewardChoiceData first = CreateChoice();
            RewardChoiceData failing = CreateChoice();
            RewardChoiceData last = CreateChoice();
            FailingChoiceApplier applier = new(failing);
            RewardBatchApplyService service = new(applier);

            AggregateException exception = Assert.Throws<AggregateException>(() =>
                service.ApplyAll(new[] { first, failing, last }));

            Assert.That(applier.Attempted, Is.EqualTo(new[] { first, failing, last }));
            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(exception.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
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

        private sealed class FailingChoiceApplier : IRewardChoiceApplier
        {
            private readonly RewardChoiceData _failingChoice;

            public FailingChoiceApplier(RewardChoiceData failingChoice)
            {
                _failingChoice = failingChoice;
            }

            public List<RewardChoiceData> Attempted { get; } = new();

            public void Apply(RewardChoiceData choice)
            {
                Attempted.Add(choice);

                if (ReferenceEquals(choice, _failingChoice))
                    throw new InvalidOperationException("Reward application failed.");
            }
        }
    }
}
