using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Presentation.UI.Rewards;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class RewardSelectionCommitterTests
    {
        [Test]
        public void Commit_AppliesChoiceAndMarksContinuation()
        {
            RewardRequestLifecycle lifecycle = CreateActiveLifecycle();
            RewardChoiceData choice = CreateChoice();
            RecordingChoiceApplier applier = new();
            RewardSelectionCommitter committer = new(applier, lifecycle);

            committer.Commit(choice);

            Assert.That(applier.Applied, Is.EqualTo(new[] { choice }));
            Assert.That(lifecycle.ShouldOpenNext, Is.True);
        }

        [Test]
        public void Commit_WhenApplyThrows_ReportsFailureWithoutBlockingDismissal()
        {
            RewardRequestLifecycle lifecycle = CreateActiveLifecycle();
            RewardSelectionCommitter committer = new(
                new ThrowingChoiceApplier(),
                lifecycle);
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: Reward application failed\\."));

            Assert.DoesNotThrow(() => committer.Commit(CreateChoice()));
            Assert.That(lifecycle.ShouldOpenNext, Is.True);
        }

        private static RewardRequestLifecycle CreateActiveLifecycle()
        {
            RewardRequestLifecycle lifecycle = new();
            lifecycle.Begin(default);
            return lifecycle;
        }

        private static RewardChoiceData CreateChoice()
        {
            return new RewardChoiceData(new RewardModifierEntry());
        }

        private sealed class RecordingChoiceApplier : IRewardChoiceApplier
        {
            public List<RewardChoiceData> Applied { get; } = new();

            public void Apply(RewardChoiceData choice)
            {
                Applied.Add(choice);
            }
        }

        private sealed class ThrowingChoiceApplier : IRewardChoiceApplier
        {
            public void Apply(RewardChoiceData choice)
            {
                throw new InvalidOperationException("Reward application failed.");
            }
        }
    }
}
