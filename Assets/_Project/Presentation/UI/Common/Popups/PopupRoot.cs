
using Game.Core.Collections;
using Game.Core.Timing;
using Game.Gameplay.Input;

namespace Game.Presentation.UI.Common.Popups
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Zenject;

    [DisallowMultipleComponent]
    public sealed class PopupRoot : MonoBehaviour
    {
        [SerializeField] private List<PopupView> _popups = new();
        [SerializeField] private bool _registerChildPopups = true;
        [SerializeField] private bool _hidePopupsOnAwake = true;
        [SerializeField] private bool _pauseTimeWhileModalVisible = true;

        private readonly QueuedActivationState<PopupView> _navigation = new();

        private PopupRegistry _registry;
        private PopupModalLock _modalLock;
        private SignalBus _signalBus;
        private bool _isSubscribedToSignals;

        [Inject]
        public void Construct(
            IGameplayInputLock gameplayInputLock,
            ITimeScaleController timeScaleController,
            SignalBus signalBus)
        {
            _modalLock = new PopupModalLock(
                gameplayInputLock,
                timeScaleController,
                _pauseTimeWhileModalVisible);
            _signalBus = signalBus;
            SubscribeToSignals();
        }

        private void Awake()
        {
            EnsureRegistry();
            RefreshRegistry();

            if (_hidePopupsOnAwake)
                HideAllPopups();
        }

        private void OnEnable()
        {
            SubscribeToSignals();
        }

        private void OnDisable()
        {
            UnsubscribeFromSignals();
            _navigation.ClearQueued();
            HideActiveInternal(true, false);
        }

        private void OnDestroy()
        {
            _registry?.Clear();
            ReleaseGameplayLock();
        }

        public bool Show(string popupId)
        {
            if (string.IsNullOrEmpty(popupId))
                return false;

            EnsureRegistry();

            if (!_registry.TryGet(popupId, out PopupView popup))
            {
                RefreshRegistry();

                if (!_registry.TryGet(popupId, out popup))
                {
                    Debug.LogWarning($"PopupRoot: popup '{popupId}' is not registered.", this);
                    return false;
                }
            }

            Show(popup);
            return true;
        }

        public void Show(PopupView popup)
        {
            if (popup == null)
                return;

            RegisterPopup(popup);

            if (ReferenceEquals(_navigation.ActiveItem, popup))
                return;

            ActivationRequestResult result = _navigation.Request(popup);

            if (result == ActivationRequestResult.Activated)
                ShowActivated(popup);
        }

        public void HideActive(bool releaseGameplayLock = true)
        {
            _navigation.ClearQueued();
            HideActiveInternal(releaseGameplayLock, false);
        }

        private void HideActiveInternal(
            bool releaseGameplayLock,
            bool showQueuedPopup)
        {
            if (_navigation.Deactivate(out PopupView popup))
                popup.Hide();

            if (_navigation.ActiveItem != null)
                return;

            if (releaseGameplayLock)
            {
                if (showQueuedPopup &&
                    _navigation.TryActivateNext(out PopupView queuedPopup))
                {
                    ShowActivated(queuedPopup);
                    return;
                }

                ReleaseGameplayLock();
            }
        }

        public void ReleaseGameplayLock()
        {
            _modalLock?.Release();
        }

        public void RefreshRegistry()
        {
            EnsureRegistry();
            _registry.Clear();
            _navigation.ClearQueued();

            if (_popups != null)
            {
                for (int i = 0; i < _popups.Count; i++)
                {
                    RegisterPopup(_popups[i]);
                }
            }

            if (!_registerChildPopups)
                return;

            PopupView[] childPopups = GetComponentsInChildren<PopupView>(true);

            for (int i = 0; i < childPopups.Length; i++)
            {
                RegisterPopup(childPopups[i]);
            }
        }

        private void RegisterPopup(PopupView popup)
        {
            if (popup == null)
                return;

            EnsureRegistry();
            PopupRegistrationResult result = _registry.Register(popup);

            if (result == PopupRegistrationResult.DuplicateId)
            {
                Debug.LogWarning($"PopupRoot: duplicate popup id '{popup.PopupId}'.", popup);
            }
        }

        private void HandlePopupCloseRequested(PopupView popup)
        {
            if (ReferenceEquals(popup, _navigation.ActiveItem))
            {
                HideActiveInternal(true, true);
                return;
            }

            if (_navigation.RemoveQueued(popup))
                return;

            popup?.Hide();
        }

        private void HandleShowRequested(ShowPopupRequestedSignal signal)
        {
            Show(signal.PopupId);
        }

        private void ShowActivated(PopupView popup)
        {
            if (popup == null)
                return;

            LockGameplay();
            popup.Show();
        }

        private void HideAllPopups()
        {
            _navigation.Clear();

            for (int i = 0; i < _registry.Count; i++)
            {
                PopupView popup = _registry.Items[i];

                if (popup != null)
                    popup.Hide();
            }
        }

        private void LockGameplay()
        {
            _modalLock.Acquire();
        }

        private void SubscribeToSignals()
        {
            if (_signalBus == null || _isSubscribedToSignals || !isActiveAndEnabled)
                return;

            _signalBus.Subscribe<ShowPopupRequestedSignal>(HandleShowRequested);
            _isSubscribedToSignals = true;
        }

        private void EnsureRegistry()
        {
            _registry ??= new PopupRegistry(HandlePopupCloseRequested);
        }

        private void UnsubscribeFromSignals()
        {
            if (_signalBus == null || !_isSubscribedToSignals)
                return;

            _signalBus.Unsubscribe<ShowPopupRequestedSignal>(HandleShowRequested);
            _isSubscribedToSignals = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (_popups == null)
                _popups = new List<PopupView>();

            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                if (_popups[i] == null)
                    _popups.RemoveAt(i);
            }

            PopupView[] childPopups = GetComponentsInChildren<PopupView>(true);

            for (int i = 0; i < childPopups.Length; i++)
            {
                if (!_popups.Contains(childPopups[i]))
                    _popups.Add(childPopups[i]);
            }
        }
#endif
    }

}
