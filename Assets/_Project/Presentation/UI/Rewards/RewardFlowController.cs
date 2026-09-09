
using Game.Gameplay.Rewards;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Infrastructure.Advertising;
using Game.Presentation.UI.Common.Popups;

namespace Game.Presentation.UI.Rewards
{
    using System;

    public sealed class RewardFlowController : IDisposable
    {
        private readonly IRewardChoiceRollService _choiceRollService;
        private readonly IRewardChoiceApplier _applyService;
        private readonly RewardGrantedActionService _grantedActionService;
        private readonly RewardPopupGateway _popupGateway;
        private readonly RewardedAdOperation _rewardedAdOperation;
        private readonly RewardAttemptState _attempts;
        private readonly RewardRequestCoordinator _requestCoordinator;
        private readonly RewardRequestLifecycle _requestLifecycle;

        private bool _isDisposed;

        public RewardFlowController(
            IRewardChoiceRollService choiceRollService,
            IRewardChoiceApplier applyService,
            RewardGrantedActionService grantedActionService,
            RewardPopupGateway popupGateway,
            RewardedAdOperation rewardedAdOperation,
            RewardAttemptState attempts,
            RewardRequestCoordinator requestCoordinator,
            RewardRequestLifecycle requestLifecycle)
        {
            _choiceRollService = choiceRollService ??
                throw new ArgumentNullException(nameof(choiceRollService));
            _applyService = applyService ?? throw new ArgumentNullException(nameof(applyService));
            _grantedActionService = grantedActionService ??
                throw new ArgumentNullException(nameof(grantedActionService));
            _popupGateway = popupGateway ??
                throw new ArgumentNullException(nameof(popupGateway));
            _rewardedAdOperation = rewardedAdOperation
                ?? throw new ArgumentNullException(nameof(rewardedAdOperation));
            _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
            _requestCoordinator = requestCoordinator ??
                throw new ArgumentNullException(nameof(requestCoordinator));
            _requestLifecycle = requestLifecycle
                ?? throw new ArgumentNullException(nameof(requestLifecycle));
            _popupGateway.Intent += HandleIntent;
            _popupGateway.Hidden += HandlePopupHidden;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _popupGateway.Intent -= HandleIntent;
            _popupGateway.Hidden -= HandlePopupHidden;

            _rewardedAdOperation.Cancel();
            _requestCoordinator.Reset();
            _isDisposed = true;
        }

        public bool Open(
            CocoonRewardProfile cocoonProfile = null,
            RewardRollContext rollContext = default)
        {
            if (_isDisposed)
                return false;

            RewardOpenRequest request = new(cocoonProfile, rollContext);

            if (_requestCoordinator.Submit(request) == RewardRequestSubmission.Queued)
                return true;

            return StartActiveRequest();
        }

        private bool StartActiveRequest()
        {
            _rewardedAdOperation.Cancel();

            if (!RollCurrentChoices())
            {
                CompleteCurrentPopupRequest();
                return false;
            }

            if (ShowCurrentChoices(false))
            {
                return true;
            }

            CompleteCurrentPopupRequest();
            return false;
        }

        public void ResetSession()
        {
            _requestCoordinator.Reset();
            _attempts.Reset();
            _rewardedAdOperation.Cancel();
        }

        private void HandleSelected(RewardChoiceData choice)
        {
            if (_rewardedAdOperation.IsPending)
                return;

            _requestLifecycle.MarkShouldOpenNext();
            _applyService.Apply(choice);
        }

        private void HandleIntent(RewardUserIntent intent)
        {
            switch (intent.Type)
            {
                case RewardUserIntentType.Select:
                    HandleSelected(intent.Choice);
                    break;
                case RewardUserIntentType.Reroll:
                    HandleRerollRequested();
                    break;
                case RewardUserIntentType.AdReroll:
                    HandleAdRerollRequested();
                    break;
                case RewardUserIntentType.TakeAll:
                    HandleTakeAllRequested();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleRerollRequested()
        {
            if (!_attempts.HasFreeReroll || _rewardedAdOperation.IsPending)
                return;

            if (!RollCurrentChoices())
            {
                _popupGateway.SetInteractable(true);
                return;
            }

            _attempts.ConsumeFreeReroll();
            ShowCurrentChoices(true);
        }

        private void HandleAdRerollRequested()
        {
            if (_attempts.HasFreeReroll || !_attempts.HasAdReroll)
                return;

            if (_rewardedAdOperation.IsPending)
                return;

            _popupGateway.SetInteractable(false);
            if (!_rewardedAdOperation.TryBegin(CompleteAdRerollReward))
                _popupGateway.SetInteractable(true);
        }

        private void HandleTakeAllRequested()
        {
            if (_requestLifecycle.Choices == null || _requestLifecycle.Choices.Count == 0)
                return;

            if (!RewardAdRerollPolicy.CanOfferTakeAll(_requestLifecycle.RollContext))
                return;

            if (!_attempts.HasTakeAll || _rewardedAdOperation.IsPending)
                return;

            _popupGateway.SetInteractable(false);
            if (!_rewardedAdOperation.TryBegin(CompleteTakeAllReward))
                _popupGateway.SetInteractable(true);
        }

        private void CompleteAdRerollReward(bool rewardGranted)
        {
            if (_isDisposed)
                return;

            if (!rewardGranted)
            {
                ShowCurrentChoices(false);
                return;
            }

            if (!_grantedActionService.CompleteAdReroll())
            {
                _popupGateway.SetInteractable(true);
                return;
            }

            ShowCurrentChoices(true);
        }

        private void CompleteTakeAllReward(bool rewardGranted)
        {
            if (_isDisposed)
                return;

            if (!rewardGranted)
            {
                ShowCurrentChoices(false);
                return;
            }

            if (!_grantedActionService.CompleteTakeAll())
            {
                _popupGateway.SetInteractable(true);
                return;
            }

            _popupGateway.Close();
        }

        private void HandlePopupHidden(PopupView _)
        {
            if (_isDisposed)
                return;

            bool shouldOpenNext = CompleteCurrentPopupRequest();

            if (shouldOpenNext)
            {
                TryOpenNextPendingRequest();
                return;
            }

            _requestCoordinator.ClearPending();
        }

        private bool CompleteCurrentPopupRequest()
        {
            _rewardedAdOperation.Cancel();
            return _requestCoordinator.CompleteActive();
        }

        private void TryOpenNextPendingRequest()
        {
            int pendingRequestCount = _requestCoordinator.PendingCount;

            for (int index = 0; index < pendingRequestCount; index++)
            {
                if (_isDisposed || _requestLifecycle.IsActive ||
                    !_requestCoordinator.TryBeginNext(out _))
                {
                    return;
                }

                if (StartActiveRequest())
                    return;
            }
        }

        private bool RollCurrentChoices()
        {
            RewardChoiceRollResult result = _choiceRollService.RollStandard(
                _requestLifecycle.CocoonProfile,
                _requestLifecycle.RollContext);

            _requestLifecycle.SetRollResult(result.GuaranteeRarity, result.Choices);
            return result.HasChoices;
        }

        private bool ShowCurrentChoices(bool animateChoiceChanges)
        {
            return _popupGateway.Show(
                animateChoiceChanges,
                _rewardedAdOperation.IsPending);
        }

    }

}
