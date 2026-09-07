using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class RewardRequestCoordinatorTests
    {
        [Test]
        public void Submit_BeginsFirstRequestAndQueuesFollowingRequest()
        {
            RewardRequestLifecycle lifecycle = new();
            RewardRequestCoordinator coordinator = new(new RewardRequestQueue(), lifecycle);
            RewardOpenRequest first = new(null, new RewardRollContext(0.1f, 0f, false));
            RewardOpenRequest second = new(null, new RewardRollContext(0.2f, 0f, false));

            RewardRequestSubmission firstResult = coordinator.Submit(first);
            RewardRequestSubmission secondResult = coordinator.Submit(second);

            Assert.That(firstResult, Is.EqualTo(RewardRequestSubmission.Begun));
            Assert.That(secondResult, Is.EqualTo(RewardRequestSubmission.Queued));
            Assert.That(lifecycle.RollContext.HeadPathProgressNormalized, Is.EqualTo(0.1f));
            Assert.That(coordinator.PendingCount, Is.EqualTo(1));
        }

        [Test]
        public void CompleteThenBeginNext_PreservesFifoOrder()
        {
            RewardRequestLifecycle lifecycle = new();
            RewardRequestCoordinator coordinator = new(new RewardRequestQueue(), lifecycle);
            coordinator.Submit(new RewardOpenRequest(
                null,
                new RewardRollContext(0.1f, 0f, false)));
            coordinator.Submit(new RewardOpenRequest(
                null,
                new RewardRollContext(0.2f, 0f, false)));
            coordinator.Submit(new RewardOpenRequest(
                null,
                new RewardRollContext(0.3f, 0f, false)));

            coordinator.CompleteActive();
            bool beganSecond = coordinator.TryBeginNext(out RewardOpenRequest second);
            coordinator.CompleteActive();
            bool beganThird = coordinator.TryBeginNext(out RewardOpenRequest third);

            Assert.That(beganSecond, Is.True);
            Assert.That(second.RollContext.HeadPathProgressNormalized, Is.EqualTo(0.2f));
            Assert.That(beganThird, Is.True);
            Assert.That(third.RollContext.HeadPathProgressNormalized, Is.EqualTo(0.3f));
            Assert.That(coordinator.PendingCount, Is.Zero);
        }

        [Test]
        public void Reset_ClearsActiveAndPendingRequests()
        {
            RewardRequestLifecycle lifecycle = new();
            RewardRequestCoordinator coordinator = new(new RewardRequestQueue(), lifecycle);
            coordinator.Submit(default);
            coordinator.Submit(default);

            coordinator.Reset();

            Assert.That(lifecycle.IsActive, Is.False);
            Assert.That(coordinator.PendingCount, Is.Zero);
            Assert.That(coordinator.TryBeginNext(out _), Is.False);
        }
    }
}
