using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

using Game.Infrastructure.Advertising;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class RewardedAdOperationTests
    {
        [Test]
        public void TryBegin_WhenAdCompletes_ForwardsResultAndClearsPending()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool? result = null;

            bool started = operation.TryBegin(value => result = value);
            adService.Complete(0, true);

            Assert.That(started, Is.True);
            Assert.That(result, Is.True);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void TryBegin_WhenAdIsDenied_ForwardsDenialAndClearsPending()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool? result = null;

            operation.TryBegin(value => result = value);
            adService.Complete(0, false);

            Assert.That(result, Is.False);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void TryBegin_WhenServiceThrows_ReportsFailureAndRollsBackPendingState()
        {
            RewardedAdOperation operation = new(new ThrowingRewardedAdService());
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: Ad service failed to start\\."));

            bool started = operation.TryBegin(_ => { });

            Assert.That(started, Is.False);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void Complete_WhenCompletionThrows_ReportsFailureAndInvokesRecovery()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool recoveryCalled = false;
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: Reward completion failed\\."));
            operation.TryBegin(
                _ => throw new InvalidOperationException("Reward completion failed."),
                () => recoveryCalled = true);

            Assert.DoesNotThrow(() => adService.Complete(0, true));
            Assert.That(recoveryCalled, Is.True);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void TryBegin_WhenServiceIsNotReady_DoesNotInvokeService()
        {
            UnavailableRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool? result = null;

            bool started = operation.TryBegin(value => result = value);

            Assert.That(started, Is.False);
            Assert.That(result, Is.Null);
            Assert.That(operation.IsPending, Is.False);
            Assert.That(adService.ShowCalls, Is.Zero);
        }

        [Test]
        public void TryBegin_WhenReadinessThrows_ReportsFailureWithoutEscapingBoundary()
        {
            RewardedAdOperation operation = new(new ThrowingReadinessRewardedAdService());
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: Ad readiness failed\\."));

            bool started = false;
            Assert.DoesNotThrow(() => started = operation.TryBegin(_ => { }));

            Assert.That(started, Is.False);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void Cancel_InvalidatesLateCallback()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool wasCalled = false;

            operation.TryBegin(_ => wasCalled = true);
            operation.Cancel();
            adService.Complete(0, true);

            Assert.That(wasCalled, Is.False);
            Assert.That(operation.IsPending, Is.False);
        }

        [Test]
        public void PreviousCallback_CannotCompleteNewOperation()
        {
            DelayedRewardedAdService adService = new();
            RewardedAdOperation operation = new(adService);
            bool newOperationCompleted = false;

            operation.TryBegin(_ => { });
            operation.Cancel();
            operation.TryBegin(_ => newOperationCompleted = true);

            adService.Complete(0, true);

            Assert.That(newOperationCompleted, Is.False);
            Assert.That(operation.IsPending, Is.True);

            adService.Complete(1, true);

            Assert.That(newOperationCompleted, Is.True);
            Assert.That(operation.IsPending, Is.False);
        }

        private sealed class DelayedRewardedAdService : IRewardedAdService
        {
            private readonly List<Action<bool>> _callbacks = new();

            public bool IsReady => true;

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                _callbacks.Add(onCompleted);
            }

            public void Complete(int index, bool rewardGranted)
            {
                _callbacks[index](rewardGranted);
            }
        }

        private sealed class ThrowingRewardedAdService : IRewardedAdService
        {
            public bool IsReady => true;

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                throw new InvalidOperationException("Ad service failed to start.");
            }
        }

        private sealed class UnavailableRewardedAdService : IRewardedAdService
        {
            public bool IsReady => false;
            public int ShowCalls { get; private set; }

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                ShowCalls++;
            }
        }

        private sealed class ThrowingReadinessRewardedAdService : IRewardedAdService
        {
            public bool IsReady =>
                throw new InvalidOperationException("Ad readiness failed.");

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                throw new AssertionException("Show must not be called.");
            }
        }
    }
}
