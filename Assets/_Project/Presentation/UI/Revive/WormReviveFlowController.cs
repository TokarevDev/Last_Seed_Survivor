using System;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Pooling;
using Game.Gameplay.Signals;
using Game.Infrastructure.Advertising;
using Game.Infrastructure.Navigation;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.Worm;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Game.Presentation.UI.Revive
{
    [DisallowMultipleComponent]
    public sealed class WormReviveFlowController : MonoBehaviour
    {
        [SerializeField] private WormController _wormController;
        [SerializeField] private WormCombatController _wormCombat;
        [SerializeField] private PoolRegistry _projectilePoolRegistry;
        [SerializeField] private ProjectileWeapon _projectileWeapon;
        [SerializeField] private AcaciaThornWeapon _acaciaThornWeapon;
        [SerializeField] private WormDamagePopupPresenter _damagePopupPresenter;
        [SerializeField] private PopupRoot _popupRoot;
        [SerializeField] private RevivalPopupView _revivalPopup;
        [SerializeField] private RewardedAdService _rewardedAdService;
        [SerializeField, Min(0)] private int _maxReviveAttempts = 1;
        [SerializeField, Range(0f, 1f)] private float _fallbackRemainingLevelNormalized = 0.5f;
        [FormerlySerializedAs("_reloadCurrentSceneOnGiveUp")]
        [SerializeField] private bool _returnToLobbyOnGiveUp = true;
        [FormerlySerializedAs("_giveUpRestartAnimationDuration")]
        [SerializeField, Min(0f)] private float _popupCloseAnimationDuration = 0.55f;
        [FormerlySerializedAs("_giveUpRestartTargetScale")]
        [SerializeField, Range(0.5f, 1f)] private float _popupCloseAnimationTargetScale = 0.92f;

        private RevivalPopupViewModel _revivalPopupViewModel;
        private WormReviveApplicationFlow _applicationFlow;
        private ISceneNavigator<GameSceneId> _sceneNavigator;
        private SignalBus _signalBus;
        private bool _isSubscribedToSignals;

        [Inject]
        public void Construct(
            ISceneNavigator<GameSceneId> sceneNavigator,
            SignalBus signalBus,
            WormReviveApplicationFlow applicationFlow)
        {
            _sceneNavigator = sceneNavigator;
            _signalBus = signalBus;
            _applicationFlow = applicationFlow ??
                throw new ArgumentNullException(nameof(applicationFlow));
            SubscribeToSignals();
        }

#if UNITY_EDITOR
        public int EditorMaxReviveAttempts => _maxReviveAttempts;
#endif

        private void Awake()
        {
            _applicationFlow.InitializeSession(_maxReviveAttempts);
        }

        private void OnEnable()
        {
            SubscribeToSignals();

            if (_revivalPopup != null)
                _revivalPopup.Intent += HandleRevivalPopupIntent;
        }

        private void OnDisable()
        {
            UnsubscribeFromSignals();

            if (_revivalPopup != null)
                _revivalPopup.Intent -= HandleRevivalPopupIntent;

            try
            {
                _popupRoot?.ReleaseGameplayLock();
            }
            finally
            {
                _applicationFlow?.Deactivate();
            }
        }

        private void HandlePathCompleted(WormPathCompletedSignal signal)
        {
            if (!_applicationFlow.TryBeginFailure())
                return;

            ClearTransientGameplay();

            try
            {
                ShowRevivalPopup();
            }
            catch
            {
                _applicationFlow.AbortFailure();
                _popupRoot?.ReleaseGameplayLock();
                throw;
            }
        }

        private void SubscribeToSignals()
        {
            if (_signalBus == null || _isSubscribedToSignals || !isActiveAndEnabled)
                return;

            _signalBus.Subscribe<WormPathCompletedSignal>(HandlePathCompleted);
            _isSubscribedToSignals = true;
        }

        private void UnsubscribeFromSignals()
        {
            if (_signalBus == null || !_isSubscribedToSignals)
                return;

            _signalBus.Unsubscribe<WormPathCompletedSignal>(HandlePathCompleted);
            _isSubscribedToSignals = false;
        }

        private void ShowRevivalPopup()
        {
            if (_popupRoot == null || _revivalPopup == null)
            {
                Debug.LogError("WormReviveFlowController: popup references are missing.", this);
                _applicationFlow.AbortFailure();
                return;
            }

            _revivalPopupViewModel = new RevivalPopupViewModel(
                _applicationFlow.RemainingAttempts,
                GetCurrentLevelProgressNormalized(),
                GetCurrentRemainingLevelNormalized(),
                _applicationFlow.CanRevive,
                _applicationFlow.IsWaiting);
            _revivalPopup.Render(_revivalPopupViewModel);

            _popupRoot.Show(_revivalPopup);
        }

        private void HandleRevivalPopupIntent(RevivalPopupIntent intent)
        {
            switch (intent)
            {
                case RevivalPopupIntent.Revive:
                    HandleReviveRequested();
                    break;
                case RevivalPopupIntent.GiveUp:
                    HandleGiveUpRequested();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
            }
        }

        private void HandleReviveRequested()
        {
            if (!_applicationFlow.CanRevive)
                return;

            SetPopupWaiting(true);

            if (_rewardedAdService == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("WormReviveFlowController: rewarded ad service is missing. Granting revive in editor/development build.", this);
                if (_applicationFlow.TryCommitDevelopmentRevive())
                    CompleteRewardedRevive();
#else
            Debug.LogError("WormReviveFlowController: rewarded ad service is missing.", this);
            SetPopupWaiting(false);
#endif
                return;
            }

            if (!_applicationFlow.TryBeginRewardedRevive(
                    CompleteRewardedAd,
                    RestorePopupInteraction))
                SetPopupWaiting(false);
        }

        private void RestorePopupInteraction()
        {
            if (isActiveAndEnabled)
                SetPopupWaiting(false);
        }

        private void CompleteRewardedAd(bool rewardGranted)
        {
            if (!rewardGranted)
            {
                SetPopupWaiting(false);
                return;
            }

            CompleteRewardedRevive();
        }

        private void CompleteRewardedRevive()
        {
            Exception failure = null;

            TryRunStage(
                () => _signalBus.Fire<WormReviveGrantedSignal>(),
                ref failure);
            TryRunStage(StartReviveRollback, ref failure, CompleteReviveRollback);
            TryRunStage(
                () => PlayPopupCloseAnimation(CompleteRevivePopupClose),
                ref failure,
                CompleteRevivePopupClose);

            if (failure != null)
                Debug.LogException(failure, this);
        }

        private void StartReviveRollback()
        {
            if (_wormController == null)
            {
                Debug.LogError("WormReviveFlowController: worm controller is missing.", this);
                CompleteReviveRollback();
                return;
            }

            if (!_wormController.RollbackToReviveStart(CompleteReviveRollback))
                CompleteReviveRollback();
        }

        private void CompleteReviveRollback()
        {
            WormReviveStageCompletion completion =
                _applicationFlow.CompleteRollback();

            if (completion == WormReviveStageCompletion.Ignored)
                return;

            try
            {
                _signalBus.Fire<WormReviveRollbackCompletedSignal>();
            }
            finally
            {
                if (completion == WormReviveStageCompletion.FlowCompleted)
                    _popupRoot?.ReleaseGameplayLock();
            }
        }

        private void CompleteRevivePopupClose()
        {
            WormReviveStageCompletion completion =
                _applicationFlow.CompletePopupClose();

            if (completion == WormReviveStageCompletion.Ignored)
                return;

            try
            {
                _popupRoot?.HideActive(releaseGameplayLock: false);
            }
            finally
            {
                if (completion == WormReviveStageCompletion.FlowCompleted)
                    _popupRoot?.ReleaseGameplayLock();
            }
        }

        private void HandleGiveUpRequested()
        {
            if (!_applicationFlow.TryBeginGiveUp())
                return;

            if (!_returnToLobbyOnGiveUp)
            {
                CompleteGiveUpWithoutNavigation();
                return;
            }

            PlayPopupCloseAnimation(RequestLobbyLoad);
        }

        public void ResetForNewRun()
        {
            _applicationFlow.ResetSession();
        }

        private void RequestLobbyLoad()
        {
            try
            {
                _popupRoot?.HideActive();
            }
            finally
            {
                _applicationFlow.CompleteGiveUp();
            }

            NavigateToLobbyAsync().Forget();
        }

        private void CompleteGiveUpWithoutNavigation()
        {
            try
            {
                _popupRoot?.HideActive();
            }
            finally
            {
                _applicationFlow.CompleteGiveUp();
            }
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

        private void SetPopupWaiting(bool isWaiting)
        {
            if (_revivalPopup == null)
                return;

            _revivalPopupViewModel = _revivalPopupViewModel.WithWaiting(isWaiting);
            _revivalPopup.Render(_revivalPopupViewModel);
        }

        private void PlayPopupCloseAnimation(System.Action onComplete)
        {
            if (_revivalPopup == null)
            {
                onComplete?.Invoke();
                return;
            }

            SetPopupWaiting(true);
            _revivalPopup.PlayCloseAnimation(
                _popupCloseAnimationDuration,
                _popupCloseAnimationTargetScale,
                onComplete);
        }

        private static void TryRunStage(
            Action stage,
            ref Exception failure,
            Action recover = null)
        {
            try
            {
                stage();
            }
            catch (Exception exception)
            {
                failure = failure == null
                    ? exception
                    : new AggregateException(failure, exception);

                if (recover == null)
                    return;

                try
                {
                    recover();
                }
                catch (Exception recoveryException)
                {
                    failure = new AggregateException(failure, recoveryException);
                }
            }
        }

        private float GetCurrentRemainingLevelNormalized()
        {
            if (_wormCombat == null || _wormCombat.TotalProgressSegments <= 0)
                return _fallbackRemainingLevelNormalized;

            return _wormCombat.RemainingProgressNormalized;
        }

        private float GetCurrentLevelProgressNormalized()
        {
            if (_wormCombat == null || _wormCombat.TotalProgressSegments <= 0)
                return 1f - _fallbackRemainingLevelNormalized;

            return _wormCombat.DestructionProgressNormalized;
        }

        private void ClearTransientGameplay()
        {
            Exception failure = null;
            TryRunStage(
                () => _projectilePoolRegistry?.ReleaseAllActiveProjectiles(),
                ref failure);
            TryRunStage(
                () => _projectileWeapon?.ClearTransientState(),
                ref failure);
            TryRunStage(
                () => _acaciaThornWeapon?.ClearTransientState(),
                ref failure);
            TryRunStage(
                () => _damagePopupPresenter?.ClearActivePopups(),
                ref failure);

            if (failure != null)
                Debug.LogException(failure, this);
        }
    }

}
