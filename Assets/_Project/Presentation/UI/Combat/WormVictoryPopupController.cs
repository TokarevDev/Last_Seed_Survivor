using System;
using Cysharp.Threading.Tasks;
using Game.Gameplay.Signals;
using Game.Infrastructure.Navigation;
using Game.Presentation.UI.Common.Popups;
using UnityEngine;
using Zenject;

namespace Game.Presentation.UI.Combat
{
    [DisallowMultipleComponent]
    public sealed class WormVictoryPopupController : MonoBehaviour
    {
        [SerializeField] private string _victoryPopupId = "WinPopup";
        private ISceneNavigator<GameSceneId> _sceneNavigator;
        private SignalBus _signalBus;
        private bool _isSubscribedToSignals;

        [Inject]
        public void Construct(
            SignalBus signalBus,
            ISceneNavigator<GameSceneId> sceneNavigator)
        {
            _signalBus = signalBus;
            _sceneNavigator = sceneNavigator;
            SubscribeToSignals();
        }

        private void OnEnable()
        {
            SubscribeToSignals();
        }

        private void OnDisable()
        {
            UnsubscribeFromSignals();
        }

        private void HandleWormDied(WormDiedSignal signal)
        {
            if (string.IsNullOrEmpty(_victoryPopupId))
            {
                Debug.LogWarning("WormVictoryPopupController: victory popup id is empty.", this);
                return;
            }

            _signalBus.Fire(new ShowPopupRequestedSignal(_victoryPopupId));
        }

        private void HandleVictoryIntent(VictoryPopupIntentSignal signal)
        {
            switch (signal.Intent)
            {
                case VictoryPopupIntent.Accept:
                case VictoryPopupIntent.DoubleReward:
                    NavigateToLobbyAsync().Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(signal),
                        signal.Intent,
                        null);
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

        private void SubscribeToSignals()
        {
            if (_signalBus == null || _isSubscribedToSignals || !isActiveAndEnabled)
                return;

            _signalBus.Subscribe<WormDiedSignal>(HandleWormDied);
            _signalBus.Subscribe<VictoryPopupIntentSignal>(HandleVictoryIntent);
            _isSubscribedToSignals = true;
        }

        private void UnsubscribeFromSignals()
        {
            if (_signalBus == null || !_isSubscribedToSignals)
                return;

            _signalBus.Unsubscribe<WormDiedSignal>(HandleWormDied);
            _signalBus.Unsubscribe<VictoryPopupIntentSignal>(HandleVictoryIntent);
            _isSubscribedToSignals = false;
        }
    }

}
