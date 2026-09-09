
using Game.Infrastructure.Navigation;

namespace Game.Presentation.UI.Common.Popups
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using Zenject;

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

        public event Action AcceptRequested;

        public event Action DoubleRewardRequested;

        private bool _restartRequested;
        private PopupScaleFadeAnimator _animator;
        private ISceneNavigator<GameSceneId> _sceneNavigator;

        [Inject]
        public void Construct(ISceneNavigator<GameSceneId> sceneNavigator)
        {
            _sceneNavigator = sceneNavigator;
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
            AcceptRequested?.Invoke();

            RequestLobbyReturn(_closeOnAccept);
        }

        private void HandleDoubleRewardClicked()
        {
            DoubleRewardRequested?.Invoke();

            RequestLobbyReturn(_closeOnDoubleReward);
        }

        private void RequestLobbyReturn(bool closeOnComplete)
        {
            if (_restartRequested)
                return;

            _restartRequested = true;
            SetButtonsInteractable(false);
            PlayRestartAnimation(() =>
            {
                if (closeOnComplete)
                    RequestClose();

                NavigateToLobbyAsync().Forget();
            });
        }

        private async UniTask NavigateToLobbyAsync()
        {
            try
            {
                await _sceneNavigator.TryNavigateAsync(
                    GameSceneId.Lobby,
                    destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
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
