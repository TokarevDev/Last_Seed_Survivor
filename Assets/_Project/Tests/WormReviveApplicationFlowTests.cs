using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Infrastructure.Advertising;
using Game.Presentation.UI.Revive;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class WormReviveApplicationFlowTests
    {
        [Test]
        public void RewardDenied_RestoresDecisionWithoutConsumingAttempt()
        {
            DelayedRewardedAdService adService = new();
            WormReviveApplicationFlow flow = CreateFlow(adService, attempts: 1);
            bool? resolved = null;
            Assert.That(flow.TryBeginFailure(), Is.True);

            bool started = flow.TryBeginRewardedRevive(
                value => resolved = value,
                () => Assert.Fail("Operation recovery was not expected."));
            adService.Complete(false);

            Assert.That(started, Is.True);
            Assert.That(resolved, Is.False);
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.AwaitingDecision));
            Assert.That(flow.RemainingAttempts, Is.EqualTo(1));
            Assert.That(flow.CanRevive, Is.True);
        }

        [Test]
        public void RewardGranted_CommitsBeforeEffectsAndCompletesAfterBothStages()
        {
            DelayedRewardedAdService adService = new();
            WormReviveApplicationFlow flow = CreateFlow(adService, attempts: 1);
            Assert.That(flow.TryBeginFailure(), Is.True);
            flow.TryBeginRewardedRevive(
                granted =>
                {
                    Assert.That(granted, Is.True);
                    Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Reviving));
                    Assert.That(flow.RemainingAttempts, Is.Zero);
                },
                () => Assert.Fail("Committed revive must not become retryable."));

            adService.Complete(true);

            Assert.That(
                flow.CompletePopupClose(),
                Is.EqualTo(WormReviveStageCompletion.WaitingForOtherStage));
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Reviving));
            Assert.That(
                flow.CompleteRollback(),
                Is.EqualTo(WormReviveStageCompletion.FlowCompleted));
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Running));
            Assert.That(
                flow.CompleteRollback(),
                Is.EqualTo(WormReviveStageCompletion.Ignored));
        }

        [Test]
        public void GrantedEffectFailure_DoesNotRestoreRetryableDecision()
        {
            DelayedRewardedAdService adService = new();
            WormReviveApplicationFlow flow = CreateFlow(adService, attempts: 1);
            bool recoveryCalled = false;
            Assert.That(flow.TryBeginFailure(), Is.True);
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: Revive effect failed\\."));
            flow.TryBeginRewardedRevive(
                _ => throw new InvalidOperationException("Revive effect failed."),
                () => recoveryCalled = true);

            Assert.DoesNotThrow(() => adService.Complete(true));

            Assert.That(recoveryCalled, Is.False);
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Reviving));
            Assert.That(flow.RemainingAttempts, Is.Zero);
            Assert.That(flow.CanRevive, Is.False);
        }

        [Test]
        public void UnavailableReward_RestoresDecisionAndDoesNotStart()
        {
            WormReviveApplicationFlow flow = CreateFlow(
                new UnavailableRewardedAdService(),
                attempts: 1);
            Assert.That(flow.TryBeginFailure(), Is.True);

            bool started = flow.TryBeginRewardedRevive(_ => { }, null);

            Assert.That(started, Is.False);
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.AwaitingDecision));
            Assert.That(flow.RemainingAttempts, Is.EqualTo(1));
        }

        [Test]
        public void ResetSession_InvalidatesLateRewardAndRestoresAttempts()
        {
            DelayedRewardedAdService adService = new();
            WormReviveApplicationFlow flow = CreateFlow(adService, attempts: 2);
            bool resolved = false;
            flow.TryBeginFailure();
            flow.TryBeginRewardedRevive(_ => resolved = true, null);

            flow.ResetSession();
            adService.Complete(true);

            Assert.That(resolved, Is.False);
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Running));
            Assert.That(flow.RemainingAttempts, Is.EqualTo(2));
        }

        [Test]
        public void GiveUp_IsAcceptedOnlyFromDecisionPhase()
        {
            WormReviveApplicationFlow flow = CreateFlow(
                new UnavailableRewardedAdService(),
                attempts: 1);

            Assert.That(flow.TryBeginGiveUp(), Is.False);
            Assert.That(flow.TryBeginFailure(), Is.True);
            Assert.That(flow.TryBeginGiveUp(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.GivingUp));
            Assert.That(flow.TryBeginRewardedRevive(_ => { }, null), Is.False);

            flow.CompleteGiveUp();

            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Running));
        }

        [Test]
        public void AbortFailure_OnlyReturnsDecisionPhaseToRunning()
        {
            WormReviveApplicationFlow flow = CreateFlow(
                new UnavailableRewardedAdService(),
                attempts: 1);

            Assert.That(flow.AbortFailure(), Is.False);
            Assert.That(flow.TryBeginFailure(), Is.True);
            Assert.That(flow.AbortFailure(), Is.True);

            Assert.That(flow.Phase, Is.EqualTo(WormRevivePhase.Running));
            Assert.That(flow.AbortFailure(), Is.False);
        }

        [Test]
        public void ResetSession_DoesNotCancelAnotherFlowsRewardOperation()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            WormReviveApplicationFlow flow = new(operation);
            flow.InitializeSession(1);
            object otherOwner = new();
            Assert.That(operation.TryBegin(otherOwner, _ => { }), Is.True);

            flow.ResetSession();

            Assert.That(operation.IsPending, Is.True);
            Assert.That(operation.Cancel(otherOwner), Is.True);
        }

        private static WormReviveApplicationFlow CreateFlow(
            IRewardedAdService adService,
            int attempts)
        {
            WormReviveApplicationFlow flow = new(new RewardedAdOperation(adService));
            flow.InitializeSession(attempts);
            return flow;
        }

        private sealed class DelayedRewardedAdService : IRewardedAdService
        {
            private readonly List<Action<bool>> _callbacks = new();

            public bool IsReady => true;

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                _callbacks.Add(onCompleted);
            }

            public void Complete(bool granted)
            {
                _callbacks[0](granted);
            }
        }

        private sealed class UnavailableRewardedAdService : IRewardedAdService
        {
            public bool IsReady => false;

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                throw new AssertionException("Unavailable service must not be shown.");
            }
        }
    }
}
