using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RewardPopupView : PopupView
{
    [SerializeField] private List<RewardButtonView> _buttons;
    [SerializeField] private RewardVisualCatalog _visualCatalog;
    [SerializeField] private Button _rerollButton;
    [SerializeField] private Button _adRerollButton;
    [SerializeField] private Button _takeAllButton;

    [Header("Animation")]
    [SerializeField] private RewardPopupAnimationConfig _animationConfig;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform[] _topSlideGroups;

    [Header("Animation Audio")]
    [SerializeField] private AudioSource _animationAudioSource;
    [SerializeField] private AudioClip _showWhooshClip;
    [SerializeField] private AudioClip _showSettleClip;
    [SerializeField] private AudioClip _refreshClip;
    [SerializeField] private AudioClip _cardRevealClip;
    [SerializeField, Range(0f, 1f)] private float _animationVolume = 1f;

    [Header("Action State Text")]
    [SerializeField] private RewardPopupActionPresentationConfig _actionPresentationConfig;
    [SerializeField] private TMP_Text _rerollAttemptsText;
    [SerializeField] private TMP_Text _adRerollAttemptsText;
    [SerializeField] private TMP_Text _takeAllAttemptsText;
    [SerializeField] private TMP_Text _guaranteeText;
    [SerializeField] private TMP_Text _adRerollGuaranteeText;

    public event Action<RewardChoiceData> Selected;
    public event Action RerollRequested;
    public event Action AdRerollRequested;
    public event Action TakeAllRequested;

    private RewardPopupChoiceBinder _choiceBinder;
    private RewardPopupActionControls _actionControls;
    private RewardPopupAnimator _animator;
    private RewardPopupInteractionGate _interactionGate;
    private bool _hasBoundChoices;

    private bool CanAcceptInteraction =>
        _interactionGate != null
        && _interactionGate.IsOpen
        && (_animator == null || !_animator.IsTransitioning);

    private void Awake()
    {
        EnsureControllers();
    }

    private void OnValidate()
    {
        if (_animationConfig == null)
            Debug.LogError($"{nameof(RewardPopupView)} requires an animation config.", this);

        if (_actionPresentationConfig == null)
            Debug.LogError($"{nameof(RewardPopupView)} requires an action presentation config.", this);
    }

    private void OnEnable()
    {
        EnsureControllers();
        _actionControls.Subscribe();
    }

    private void OnDisable()
    {
        _interactionGate?.Stop();
        _animator?.Stop();
        _actionControls?.Unsubscribe();
    }

    private void Update()
    {
        _interactionGate?.Tick();
    }

    public bool Bind(
        List<RewardChoiceData> choices,
        RewardPopupState state,
        bool animateChoiceChanges = false)
    {
        EnsureControllers();

        if (choices == null || choices.Count == 0 || !_choiceBinder.HasBindableButtons)
        {
            Debug.LogWarning("RewardPopupView: reward choices or buttons are not assigned.", this);
            RequestClose();
            return false;
        }

        bool shouldAnimateRefresh = animateChoiceChanges && IsVisible && _hasBoundChoices;

        if (shouldAnimateRefresh)
        {
            _animator.PlayRewardRefresh(
                choices,
                state,
                ApplyRefreshedState,
                StartInteractionGateWhenSafe);
            return true;
        }

        _choiceBinder.ApplyChoices(choices, false);
        ApplyState(state, false, true);
        _hasBoundChoices = true;

        if (IsVisible)
            StartInteractionGateWhenSafe();

        return true;
    }

    public void Close()
    {
        RequestClose();
    }

    public void SetAllButtonsInteractable(bool interactable)
    {
        EnsureControllers();

        if (!interactable)
        {
            CloseInteractionGate();
            return;
        }

        StartInteractionGateWhenSafe();
    }

    protected override void OnShown()
    {
        EnsureControllers();
        _animator.PlayShow(StartInteractionGateWhenSafe);
    }

    protected override void OnHidden()
    {
        CloseInteractionGate();
        _animator?.Stop();
        _animator?.ResetAnimatedLayout();
        _hasBoundChoices = false;
    }

    private void EnsureControllers()
    {
        if (_choiceBinder != null)
            return;

        if (_animationConfig == null)
        {
            throw new InvalidOperationException(
                $"{nameof(RewardPopupView)} on '{name}' requires an animation config.");
        }

        if (_actionPresentationConfig == null)
        {
            throw new InvalidOperationException(
                $"{nameof(RewardPopupView)} on '{name}' requires an action presentation config.");
        }

        _choiceBinder = new RewardPopupChoiceBinder(
            _buttons,
            _visualCatalog,
            OnClicked);

        _actionControls = new RewardPopupActionControls(
            _rerollButton,
            _adRerollButton,
            _takeAllButton,
            _rerollAttemptsText,
            _adRerollAttemptsText,
            _takeAllAttemptsText,
            _guaranteeText,
            _adRerollGuaranteeText,
            _actionPresentationConfig.CreateTextSettings(),
            _actionPresentationConfig.SingleActionButtonAnchoredX,
            OnRerollClicked,
            OnAdRerollClicked,
            OnTakeAllClicked);

        _animator = new RewardPopupAnimator(
            _canvasGroup,
            _topSlideGroups,
            _choiceBinder,
            _actionControls,
            _animationConfig.CreateSettings(),
            new RewardPopupAudioPlayer(
                _animationAudioSource,
                _showWhooshClip,
                _showSettleClip,
                _refreshClip,
                _cardRevealClip,
                _animationVolume),
            CloseInteractionGate);

        _interactionGate = new RewardPopupInteractionGate(
            CanOpenInteractionGate,
            SetInteractionEnabled);
    }

    private bool CanOpenInteractionGate()
    {
        return isActiveAndEnabled && (_animator == null || !_animator.IsTransitioning);
    }

    private void ApplyRefreshedState(RewardPopupState state)
    {
        ApplyState(state, false, false);
        _hasBoundChoices = true;
    }

    private void ApplyState(
        RewardPopupState state,
        bool interactable,
        bool resetActionLayout)
    {
        _actionControls.ApplyState(state, interactable, resetActionLayout);
    }

    private void SetInteractionEnabled(bool enabled)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = enabled;
        }

        _choiceBinder.SetInteractable(enabled);
        _actionControls.SetInteractable(enabled);
    }

    private void CloseInteractionGate()
    {
        _interactionGate?.Close();
    }

    private void StartInteractionGateWhenSafe()
    {
        _interactionGate?.StartWhenSafe();
    }

    private void OnClicked(RewardChoiceData data)
    {
        if (!CanAcceptInteraction)
            return;

        CloseInteractionGate();
        Selected?.Invoke(data);
        _animator.PlaySelectionDismiss(data, RequestClose);
    }

    private void OnRerollClicked()
    {
        if (!CanAcceptInteraction)
            return;

        CloseInteractionGate();
        RerollRequested?.Invoke();
    }

    private void OnTakeAllClicked()
    {
        if (!CanAcceptInteraction)
            return;

        CloseInteractionGate();
        TakeAllRequested?.Invoke();
    }

    private void OnAdRerollClicked()
    {
        if (!CanAcceptInteraction)
            return;

        CloseInteractionGate();
        AdRerollRequested?.Invoke();
    }
}
