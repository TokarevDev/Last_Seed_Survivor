
using System;
using Game.Presentation.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI.Common.Popups
{
    public sealed class RevivalPopupView : PopupView
    {
        [SerializeField, RequiredViewReference] private Button _reviveButton;
        [SerializeField, RequiredViewReference] private Button _giveUpButton;
        [SerializeField] private RectTransform _animatedContentRoot;
        [SerializeField, RequiredViewReference] private Slider _remainingSlider;
        [SerializeField, RequiredViewReference] private TMP_Text _progressText;
        [SerializeField, RequiredViewReference] private TMP_Text _percentText;
        [SerializeField, RequiredViewReference] private TMP_Text _attemptsText;
        [SerializeField] private string _progressFormat = "Only {0}% of the level remains.";
        [SerializeField] private string _attemptsFormat = "attempts left: x{0}";
        [SerializeField, Min(0f)] private float _showAnimationDuration = 0.55f;
        [SerializeField, Range(0.5f, 1f)] private float _showAnimationStartScale = 0.92f;

        public event Action<RevivalPopupIntent> Intent;

        private bool _canRevive;
        private bool _hasRenderedModel;
        private RevivalPopupViewModel _renderedModel;
        private PopupScaleFadeAnimator _animator;

        private void OnEnable()
        {
            EnsureAnimator();

            if (_hasRenderedModel)
                ApplyModel(_renderedModel);

            if (_reviveButton != null)
                _reviveButton.onClick.AddListener(HandleReviveClicked);

            if (_giveUpButton != null)
                _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
        }

        private void OnDisable()
        {
            _animator?.CancelAndRestore();

            if (_reviveButton != null)
                _reviveButton.onClick.RemoveListener(HandleReviveClicked);

            if (_giveUpButton != null)
                _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
        }

        protected override void OnShown()
        {
            PlayShowAnimation();
        }

        public void Render(RevivalPopupViewModel model)
        {
            _renderedModel = model;
            _hasRenderedModel = true;
            ApplyModel(model);
        }

        private void ApplyModel(RevivalPopupViewModel model)
        {
            int currentPercent = Mathf.RoundToInt(
                Mathf.Clamp01(model.CurrentProgressNormalized) * 100f);
            int remainingPercent = Mathf.RoundToInt(
                Mathf.Clamp01(model.RemainingLevelNormalized) * 100f);
            _canRevive = model.CanRevive && model.AttemptsLeft > 0;

            if (_remainingSlider != null)
                _remainingSlider.SetValueWithoutNotify(currentPercent / 100f);

            if (_progressText != null)
                _progressText.SetText(_progressFormat, remainingPercent);

            if (_percentText != null)
                _percentText.SetText("{0}%", currentPercent);

            if (_attemptsText != null)
                _attemptsText.SetText(_attemptsFormat, Mathf.Max(0, model.AttemptsLeft));

            SetWaiting(model.IsWaiting);
        }

        private void SetWaiting(bool isWaiting)
        {
            if (_reviveButton != null)
                _reviveButton.interactable = !isWaiting && _canRevive;

            if (_giveUpButton != null)
                _giveUpButton.interactable = !isWaiting;
        }

        private void HandleReviveClicked()
        {
            if (!_canRevive)
                return;

            Intent?.Invoke(RevivalPopupIntent.Revive);
        }

        private void HandleGiveUpClicked()
        {
            Intent?.Invoke(RevivalPopupIntent.GiveUp);
        }

        public void PlayCloseAnimation(
            float duration,
            float targetScale,
            Action onComplete)
        {
            EnsureAnimator();
            _animator.PlayHide(duration, targetScale, onComplete);
        }

        private void PlayShowAnimation()
        {
            EnsureAnimator();
            _animator.PlayShow(_showAnimationDuration, _showAnimationStartScale);
        }

        private void EnsureAnimator()
        {
            _animator ??= new PopupScaleFadeAnimator(this, _animatedContentRoot);
        }

    }

}
