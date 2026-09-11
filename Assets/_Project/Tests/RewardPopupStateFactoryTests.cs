using System;
using NUnit.Framework;

using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Infrastructure.Advertising;
using Game.Presentation.UI.Rewards;

namespace Game.Tests
{
    public sealed class RewardPopupStateFactoryTests
    {
        [Test]
        public void Create_WithFreeAttempt_EnablesOnlyFreeReroll()
        {
            RewardAttemptState attempts = new(new RewardFlowSettings(1, 1, 1));
            RewardPopupStateFactory factory = new(
                attempts,
                CreateOperation(new StubRewardedAdService(isReady: true)));
            RewardRollContext context = new(
                headPathProgressNormalized: 1f,
                wormDestructionProgressNormalized: 0f,
                hasRevivedThisRun: false);

            RewardPopupState state = factory.Create(
                RewardRarity.Common,
                cocoonProfile: null,
                context,
                isRewardOperationPending: false);

            Assert.That(state.CanFreeReroll, Is.True);
            Assert.That(state.CanAdReroll, Is.False);
            Assert.That(state.CanTakeAll, Is.True);
        }

        [Test]
        public void Create_AfterFreeAttemptConsumed_EnablesAdReroll()
        {
            RewardAttemptState attempts = new(new RewardFlowSettings(1, 1, 0));
            RewardPopupStateFactory factory = new(
                attempts,
                CreateOperation(new StubRewardedAdService(isReady: true)));
            attempts.ConsumeFreeReroll();

            RewardPopupState state = factory.Create(
                RewardRarity.Rare,
                cocoonProfile: null,
                rollContext: default,
                isRewardOperationPending: false);

            Assert.That(state.CanFreeReroll, Is.False);
            Assert.That(state.CanAdReroll, Is.True);
            Assert.That(state.CanTakeAll, Is.False);
        }

        [Test]
        public void Create_WhileOperationPending_DisablesAllActions()
        {
            RewardAttemptState attempts = new(new RewardFlowSettings(1, 1, 1));
            RewardPopupStateFactory factory = new(
                attempts,
                CreateOperation(new StubRewardedAdService(isReady: true)));
            RewardRollContext context = new(
                headPathProgressNormalized: 1f,
                wormDestructionProgressNormalized: 0f,
                hasRevivedThisRun: false);

            RewardPopupState state = factory.Create(
                RewardRarity.Legendary,
                cocoonProfile: null,
                context,
                isRewardOperationPending: true);

            Assert.That(state.CanFreeReroll, Is.False);
            Assert.That(state.CanAdReroll, Is.False);
            Assert.That(state.CanTakeAll, Is.False);
        }

        [Test]
        public void Create_WhenRewardedAdIsUnavailable_DisablesAdBackedActions()
        {
            RewardAttemptState attempts = new(new RewardFlowSettings(0, 1, 1));
            RewardPopupStateFactory factory = new(
                attempts,
                CreateOperation(new DisabledRewardedAdService()));
            RewardRollContext context = new(
                headPathProgressNormalized: 1f,
                wormDestructionProgressNormalized: 0f,
                hasRevivedThisRun: false);

            RewardPopupState state = factory.Create(
                RewardRarity.Common,
                cocoonProfile: null,
                context,
                isRewardOperationPending: false);

            Assert.That(state.CanFreeReroll, Is.False);
            Assert.That(state.CanAdReroll, Is.False);
            Assert.That(state.CanTakeAll, Is.False);
        }

        [Test]
        public void Create_AfterRewardedAdBecomesReady_UsesCurrentAvailability()
        {
            RewardAttemptState attempts = new(new RewardFlowSettings(0, 1, 1));
            StubRewardedAdService adService = new(isReady: false);
            RewardPopupStateFactory factory = new(attempts, CreateOperation(adService));
            RewardRollContext context = new(
                headPathProgressNormalized: 1f,
                wormDestructionProgressNormalized: 0f,
                hasRevivedThisRun: false);

            RewardPopupState unavailableState = factory.Create(
                RewardRarity.Common,
                cocoonProfile: null,
                context,
                isRewardOperationPending: false);
            adService.IsReady = true;
            RewardPopupState readyState = factory.Create(
                RewardRarity.Common,
                cocoonProfile: null,
                context,
                isRewardOperationPending: false);

            Assert.That(unavailableState.CanAdReroll, Is.False);
            Assert.That(unavailableState.CanTakeAll, Is.False);
            Assert.That(readyState.CanAdReroll, Is.True);
            Assert.That(readyState.CanTakeAll, Is.True);
        }

        private sealed class StubRewardedAdService : IRewardedAdService
        {
            public StubRewardedAdService(bool isReady)
            {
                IsReady = isReady;
            }

            public bool IsReady { get; set; }

            public void ShowRewardedAd(Action<bool> onCompleted)
            {
                onCompleted?.Invoke(IsReady);
            }
        }

        private static RewardedAdOperation CreateOperation(
            IRewardedAdService rewardedAdService)
        {
            return new RewardedAdOperation(rewardedAdService);
        }
    }
}
