using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Presentation.UI.Common.Popups
{
    [DisallowMultipleComponent]
    public sealed class WinPopupView : PopupView
    {
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _doubleRewardButton;
        [SerializeField] private RectTransform _animatedContentRoot;
        [SerializeField] private bool _closeOnAccept = true;
        [SerializeField] private bool _closeOnDoubleReward = true;
        [SerializeField, Min(0f)] private float _showAnimationDuration = 0.55f;
        [SerializeField, Range(0.5f, 1f)] private float _showAnimationStartScale = 0.92f;
        [SerializeField, Min(0f)] private float _restartAnimationDuration = 0.55f;
        [SerializeField, Range(0.5f, 1f)] private float _restartAnimationTargetScale = 0.92f;

        private bool _restartRequested;
        private PopupScaleFadeAnimator _animator;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            EnsureAnimator();

            if (_acceptButton != null)
                _acceptButton.onClick.AddListener(HandleAcceptClicked);

            if (_doubleRewardButton != null)
                _doubleRewardButton.onClick.AddListener(HandleDoubleRewardClicked);
        }

        private void OnDisable()
        {
            _animator?.CancelAndRestore();

            if (_acceptButton != null)
                _acceptButton.onClick.RemoveListener(HandleAcceptClicked);

            if (_doubleRewardButton != null)
                _doubleRewardButton.onClick.RemoveListener(HandleDoubleRewardClicked);
        }

        protected override void OnShown()
        {
            _restartRequested = false;
            SetButtonsInteractable(true);
            PlayShowAnimation();
        }

        private void HandleAcceptClicked()
        {
            RequestCompletion(_closeOnAccept, VictoryPopupIntent.Accept);
        }

        private void HandleDoubleRewardClicked()
        {
            RequestCompletion(
                _closeOnDoubleReward,
                VictoryPopupIntent.DoubleReward);
        }

        private void RequestCompletion(
            bool closeOnComplete,
            VictoryPopupIntent intent)
        {
            if (_restartRequested)
                return;

            _restartRequested = true;
            SetButtonsInteractable(false);
            PlayRestartAnimation(() =>
            {
                if (closeOnComplete)
                    RequestClose();

                _signalBus.Fire(new VictoryPopupIntentSignal(intent));
            });
        }

        private void PlayRestartAnimation(Action onComplete)
        {
            EnsureAnimator();
            _animator.PlayHide(
                _restartAnimationDuration,
                _restartAnimationTargetScale,
                onComplete);
        }

        private void PlayShowAnimation()
        {
            EnsureAnimator();
            _animator.PlayShow(_showAnimationDuration, _showAnimationStartScale);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_acceptButton != null)
                _acceptButton.interactable = interactable;

            if (_doubleRewardButton != null)
                _doubleRewardButton.interactable = interactable;
        }

        private void EnsureAnimator()
        {
            _animator ??= new PopupScaleFadeAnimator(this, _animatedContentRoot);
        }
    }

}
