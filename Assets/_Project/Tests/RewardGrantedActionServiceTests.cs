using System.Collections.Generic;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class RewardGrantedActionServiceTests
    {
        [Test]
        public void CompleteAdReroll_ConsumesAttemptAndCommitsPaidRoll()
        {
            RewardAttemptState attempts = CreateAttempts();
            RewardRequestLifecycle lifecycle = CreateActiveLifecycle();
            List<RewardChoiceData> choices = new() { CreateChoice() };
            FakeChoiceRollService roller = new(
                new RewardChoiceRollResult(RewardRarity.Legendary, choices));
            RewardGrantedActionService service = CreateService(
                attempts,
                lifecycle,
                roller,
                new RecordingChoiceApplier());

            bool completed = service.CompleteAdReroll();

            Assert.That(completed, Is.True);
            Assert.That(attempts.AdRerollsLeft, Is.Zero);
            Assert.That(roller.AdRollCalls, Is.EqualTo(1));
            Assert.That(lifecycle.GuaranteeRarity, Is.EqualTo(RewardRarity.Legendary));
            Assert.That(lifecycle.Choices, Is.SameAs(choices));
        }

        [Test]
        public void CompleteAdReroll_WithoutActiveRequest_DoesNotConsumeAttempt()
        {
            RewardAttemptState attempts = CreateAttempts();
            RewardRequestLifecycle lifecycle = new();
            FakeChoiceRollService roller = new(default);
            RewardGrantedActionService service = CreateService(
                attempts,
                lifecycle,
                roller,
                new RecordingChoiceApplier());

            Assert.That(service.CompleteAdReroll(), Is.False);
            Assert.That(attempts.AdRerollsLeft, Is.EqualTo(1));
            Assert.That(roller.AdRollCalls, Is.Zero);
        }

        [Test]
        public void CompleteTakeAll_AppliesStableBatchAndMarksContinuation()
        {
            RewardAttemptState attempts = CreateAttempts();
            RewardRequestLifecycle lifecycle = CreateActiveLifecycle();
            RewardChoiceData first = CreateChoice();
            RewardChoiceData second = CreateChoice();
            lifecycle.SetRollResult(
                RewardRarity.Rare,
                new List<RewardChoiceData> { first, second });
            RecordingChoiceApplier applier = new();
            RewardGrantedActionService service = CreateService(
                attempts,
                lifecycle,
                new FakeChoiceRollService(default),
                applier);

            bool completed = service.CompleteTakeAll();

            Assert.That(completed, Is.True);
            Assert.That(attempts.TakeAllLeft, Is.Zero);
            Assert.That(lifecycle.ShouldOpenNext, Is.True);
            Assert.That(applier.Applied, Is.EqualTo(new[] { first, second }));
        }

        private static RewardGrantedActionService CreateService(
            RewardAttemptState attempts,
            RewardRequestLifecycle lifecycle,
            IRewardChoiceRollService roller,
            IRewardChoiceApplier applier)
        {
            return new RewardGrantedActionService(
                attempts,
                lifecycle,
                roller,
                new RewardBatchApplyService(applier));
        }

        private static RewardAttemptState CreateAttempts()
        {
            return new RewardAttemptState(new RewardFlowSettings(0, 1, 1));
        }

        private static RewardRequestLifecycle CreateActiveLifecycle()
        {
            RewardRequestLifecycle lifecycle = new();
            lifecycle.Begin(new RewardOpenRequest(null, default));
            return lifecycle;
        }

        private static RewardChoiceData CreateChoice()
        {
            return new RewardChoiceData(new RewardModifierEntry());
        }

        private sealed class FakeChoiceRollService : IRewardChoiceRollService
        {
            private readonly RewardChoiceRollResult _adResult;

            public FakeChoiceRollService(RewardChoiceRollResult adResult)
            {
                _adResult = adResult;
            }

            public int AdRollCalls { get; private set; }

            public RewardChoiceRollResult RollStandard(
                CocoonRewardProfile cocoonProfile,
                RewardRollContext rollContext)
            {
                return default;
            }

            public RewardChoiceRollResult RollAdAssisted(
                CocoonRewardProfile cocoonProfile,
                RewardRollContext rollContext)
            {
                AdRollCalls++;
                return _adResult;
            }
        }

        private sealed class RecordingChoiceApplier : IRewardChoiceApplier
        {
            public List<RewardChoiceData> Applied { get; } = new();

            public void Apply(RewardChoiceData choice)
            {
                Applied.Add(choice);
            }
        }
    }
}
