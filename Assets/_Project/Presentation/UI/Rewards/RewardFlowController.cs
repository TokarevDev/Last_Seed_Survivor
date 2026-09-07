using System;

public sealed class RewardFlowController : IDisposable
{
    private readonly RewardChoiceRollService _choiceRollService;
    private readonly IRewardChoiceApplier _applyService;
    private readonly RewardBatchApplyService _batchApplyService;
    private readonly RewardPopupView _popup;
    private readonly PopupRoot _popupRoot;
    private readonly RewardAdOperation _rewardAdOperation;
    private readonly RewardAttemptState _attempts;
    private readonly RewardRequestQueue _requestQueue;
    private readonly RewardRequestLifecycle _requestLifecycle;
    private readonly RewardPopupStateFactory _popupStateFactory;

    private bool _isDisposed;

    public RewardFlowController(
        RewardChoiceRollService choiceRollService,
        IRewardChoiceApplier applyService,
        RewardBatchApplyService batchApplyService,
        RewardPopupView popup,
        PopupRoot popupRoot,
        RewardAdOperation rewardAdOperation,
        RewardAttemptState attempts,
        RewardRequestQueue requestQueue,
        RewardRequestLifecycle requestLifecycle,
        RewardPopupStateFactory popupStateFactory)
    {
        _choiceRollService = choiceRollService ??
            throw new ArgumentNullException(nameof(choiceRollService));
        _applyService = applyService ?? throw new ArgumentNullException(nameof(applyService));
        _batchApplyService = batchApplyService ??
            throw new ArgumentNullException(nameof(batchApplyService));
        _popup = popup;
        _popupRoot = popupRoot;
        _rewardAdOperation = rewardAdOperation
            ?? throw new ArgumentNullException(nameof(rewardAdOperation));
        _attempts = attempts ?? throw new ArgumentNullException(nameof(attempts));
        _requestQueue = requestQueue ?? throw new ArgumentNullException(nameof(requestQueue));
        _requestLifecycle = requestLifecycle
            ?? throw new ArgumentNullException(nameof(requestLifecycle));
        _popupStateFactory = popupStateFactory
            ?? throw new ArgumentNullException(nameof(popupStateFactory));

        if (_popup == null)
        {
            UnityEngine.Debug.LogWarning("RewardFlowController: reward popup is not assigned.");
            return;
        }

        _popup.Selected += HandleSelected;
        _popup.RerollRequested += HandleRerollRequested;
        _popup.AdRerollRequested += HandleAdRerollRequested;
        _popup.TakeAllRequested += HandleTakeAllRequested;
        _popup.Hidden += HandlePopupHidden;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (_popup != null)
        {
            _popup.Selected -= HandleSelected;
            _popup.RerollRequested -= HandleRerollRequested;
            _popup.AdRerollRequested -= HandleAdRerollRequested;
            _popup.TakeAllRequested -= HandleTakeAllRequested;
            _popup.Hidden -= HandlePopupHidden;
        }

        _rewardAdOperation.Cancel();
        _requestQueue.Clear();
        _requestLifecycle.Reset();
        _isDisposed = true;
    }

    public bool Open(
        CocoonRewardProfile cocoonProfile = null,
        RewardRollContext rollContext = default)
    {
        if (_isDisposed)
            return false;

        RewardOpenRequest request = new(cocoonProfile, rollContext);

        if (_requestLifecycle.IsActive)
        {
            _requestQueue.Enqueue(request);
            return true;
        }

        return StartOpenRequest(request);
    }

    private bool StartOpenRequest(RewardOpenRequest request)
    {
        _requestLifecycle.Begin(request);
        _rewardAdOperation.Cancel();

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
        _requestQueue.Clear();
        _requestLifecycle.Reset();
        _attempts.Reset();
        _rewardAdOperation.Cancel();
    }

    private void HandleSelected(RewardChoiceData choice)
    {
        if (_rewardAdOperation.IsPending)
            return;

        _requestLifecycle.MarkShouldOpenNext();
        _applyService.Apply(choice);
    }

    private void HandleRerollRequested()
    {
        if (!_attempts.HasFreeReroll || _rewardAdOperation.IsPending)
            return;

        if (!RollCurrentChoices())
        {
            _popup?.SetAllButtonsInteractable(true);
            return;
        }

        _attempts.ConsumeFreeReroll();
        ShowCurrentChoices(true);
    }

    private void HandleAdRerollRequested()
    {
        if (_attempts.HasFreeReroll || !_attempts.HasAdReroll)
            return;

        if (_rewardAdOperation.IsPending)
            return;

        _popup?.SetAllButtonsInteractable(false);
        if (!_rewardAdOperation.TryBegin(CompleteAdRerollReward))
            _popup?.SetAllButtonsInteractable(true);
    }

    private void HandleTakeAllRequested()
    {
        if (_requestLifecycle.Choices == null || _requestLifecycle.Choices.Count == 0)
            return;

        if (!RewardAdRerollPolicy.CanOfferTakeAll(_requestLifecycle.RollContext))
            return;

        if (!_attempts.HasTakeAll || _rewardAdOperation.IsPending)
            return;

        _popup?.SetAllButtonsInteractable(false);
        if (!_rewardAdOperation.TryBegin(CompleteTakeAllReward))
            _popup?.SetAllButtonsInteractable(true);
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

        _attempts.ConsumeAdReroll();

        if (!RollCurrentChoices(isPaidAssistRoll: true))
        {
            _popup?.SetAllButtonsInteractable(true);
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

        _attempts.ConsumeTakeAll();
        _requestLifecycle.MarkShouldOpenNext();

        _batchApplyService.ApplyAll(_requestLifecycle.Choices);

        _popup?.Close();
    }

    private void HandlePopupHidden(PopupView popup)
    {
        if (popup != _popup || _isDisposed)
            return;

        bool shouldOpenNext = CompleteCurrentPopupRequest();

        if (shouldOpenNext)
        {
            TryOpenNextPendingRequest();
            return;
        }

        _requestQueue.Clear();
    }

    private bool CompleteCurrentPopupRequest()
    {
        _rewardAdOperation.Cancel();
        return _requestLifecycle.Complete();
    }

    private void TryOpenNextPendingRequest()
    {
        int pendingRequestCount = _requestQueue.Count;

        for (int index = 0; index < pendingRequestCount; index++)
        {
            if (_isDisposed || _requestLifecycle.IsActive ||
                !_requestQueue.TryDequeue(out RewardOpenRequest request))
            {
                return;
            }

            if (StartOpenRequest(request))
                return;
        }
    }

    private bool RollCurrentChoices(bool isPaidAssistRoll = false)
    {
        RewardChoiceRollResult result = isPaidAssistRoll
            ? _choiceRollService.RollAdAssisted(
                _requestLifecycle.CocoonProfile,
                _requestLifecycle.RollContext)
            : _choiceRollService.RollStandard(
                _requestLifecycle.CocoonProfile,
                _requestLifecycle.RollContext);

        _requestLifecycle.SetRollResult(result.GuaranteeRarity, result.Choices);
        return result.HasChoices;
    }

    private bool ShowCurrentChoices(bool animateChoiceChanges)
    {
        if (_popup == null || _popupRoot == null)
        {
            UnityEngine.Debug.LogWarning("RewardFlowController: reward popup or popup root is not assigned.");
            return false;
        }

        bool isBound = _popup.Bind(
            _requestLifecycle.Choices,
            _popupStateFactory.Create(
                _requestLifecycle.GuaranteeRarity,
                _requestLifecycle.CocoonProfile,
                _requestLifecycle.RollContext,
                _rewardAdOperation.IsPending),
            animateChoiceChanges);

        if (!isBound)
            return false;

        _popupRoot.Show(_popup);
        return true;
    }

}
