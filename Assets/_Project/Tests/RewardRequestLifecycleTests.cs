using System;
using System.Collections.Generic;
using NUnit.Framework;

using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;

namespace Game.Tests
{
    public sealed class RewardRequestLifecycleTests
    {
        [Test]
        public void Begin_CapturesRequestAndRejectsConcurrentRequest()
        {
            RewardRequestLifecycle lifecycle = new();
            RewardOpenRequest request = new(
                null,
                new RewardRollContext(0.25f, 0.5f, true));

            lifecycle.Begin(request);

            Assert.That(lifecycle.IsActive, Is.True);
            Assert.That(lifecycle.RollContext.HeadPathProgressNormalized, Is.EqualTo(0.25f));
            Assert.That(lifecycle.RollContext.WormDestructionProgressNormalized, Is.EqualTo(0.5f));
            Assert.That(lifecycle.RollContext.HasRevivedThisRun, Is.True);
            Assert.Throws<InvalidOperationException>(() => lifecycle.Begin(request));
        }

        [Test]
        public void SetRollResult_RequiresActiveRequestAndStoresResult()
        {
            RewardRequestLifecycle lifecycle = new();
            List<RewardChoiceData> choices = new();

            Assert.Throws<InvalidOperationException>(() =>
                lifecycle.SetRollResult(RewardRarity.Rare, choices));

            lifecycle.Begin(default);
            lifecycle.SetRollResult(RewardRarity.Rare, choices);

            Assert.That(lifecycle.GuaranteeRarity, Is.EqualTo(RewardRarity.Rare));
            Assert.That(lifecycle.Choices, Is.SameAs(choices));
        }

        [Test]
        public void Complete_ReturnsContinuationIntentAndClearsReferences()
        {
            RewardRequestLifecycle lifecycle = new();
            lifecycle.Begin(default);
            lifecycle.SetRollResult(RewardRarity.Common, new List<RewardChoiceData>());
            lifecycle.MarkShouldOpenNext();

            bool shouldOpenNext = lifecycle.Complete();

            Assert.That(shouldOpenNext, Is.True);
            Assert.That(lifecycle.IsActive, Is.False);
            Assert.That(lifecycle.ShouldOpenNext, Is.False);
            Assert.That(lifecycle.Choices, Is.Null);
            Assert.That(lifecycle.CocoonProfile, Is.Null);
        }
    }
}
