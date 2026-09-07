using System;
using System.Collections.Generic;

public sealed class RewardFlowController : IDisposable
{
    private const int AdRerollGuaranteedSlots = 1;

    private readonly RewardRollService _rollService;
    private readonly IRewardChoiceApplier _applyService;
    private readonly IRewardRuntimeContextProvider _runtimeContextProvider;
    private readonly RewardBatchApplyService _batchApplyService;
    private readonly RewardPopupView _popup;
    private readonly PopupRoot _popupRoot;
    private readonly RewardAdOperation _rewardAdOperation;
    private readonly IRandomSource _randomSource;
    private readonly RewardAttemptState _attempts;
    private readonly RewardRequestQueue _requestQueue;
    private readonly RewardRequestLifecycle _requestLifecycle;
    private readonly RewardPopupStateFactory _popupStateFactory;

    private bool _isDisposed;

    public RewardFlowController(
        RewardRollService rollService,
        IRewardChoiceApplier applyService,
        IRewardRuntimeContextProvider runtimeContextProvider,
        RewardBatchApplyService batchApplyService,
        RewardPopupView popup,
        PopupRoot popupRoot,
        RewardAdOperation rewardAdOperation,
        IRandomSource randomSource,
        RewardAttemptState attempts,
        RewardRequestQueue requestQueue,
        RewardRequestLifecycle requestLifecycle,
        RewardPopupStateFactory popupStateFactory)
    {
        _rollService = rollService;
        _applyService = applyService ?? throw new ArgumentNullException(nameof(applyService));
        _runtimeContextProvider = runtimeContextProvider ??
            throw new ArgumentNullException(nameof(runtimeContextProvider));
        _batchApplyService = batchApplyService ??
            throw new ArgumentNullException(nameof(batchApplyService));
        _popup = popup;
        _popupRoot = popupRoot;
        _rewardAdOperation = rewardAdOperation
            ?? throw new ArgumentNullException(nameof(rewardAdOperation));
        _randomSource = randomSource
            ?? throw new ArgumentNullException(nameof(randomSource));
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

        RewardRarity adGuaranteeRarity = RewardAdRerollPolicy.RollGuaranteedRarity(
            _runtimeContextProvider.RuntimeContext,
            _requestLifecycle.CocoonProfile,
            _requestLifecycle.RollContext,
            _randomSource);

        if (!RollCurrentChoices(
                adGuaranteeRarity,
                AdRerollGuaranteedSlots,
                isPaidAssistRoll: true))
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

    private bool RollCurrentChoices(
        RewardRarity? forcedGuaranteeRarity = null,
        int forcedGuaranteeSlotCount = 1,
        bool isPaidAssistRoll = false)
    {
        RewardRollContext rollContext = isPaidAssistRoll
            ? _requestLifecycle.RollContext.WithPaidAssistRoll()
            : _requestLifecycle.RollContext;

        RewardRarity guaranteeRarity = forcedGuaranteeRarity
            ?? _rollService.RollGuaranteeRarity(
                _runtimeContextProvider.RuntimeContext,
                _requestLifecycle.CocoonProfile,
                rollContext);

        List<RewardChoiceData> choices = _rollService.Roll3(
            _runtimeContextProvider.RuntimeContext,
            _requestLifecycle.CocoonProfile,
            guaranteeRarity,
            forcedGuaranteeSlotCount,
            rollContext);
        _requestLifecycle.SetRollResult(guaranteeRarity, choices);

        return choices != null && choices.Count > 0;
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
