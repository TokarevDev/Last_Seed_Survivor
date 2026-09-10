using System;
using Game.Gameplay.Rewards.Runtime;
using Game.Presentation.UI.Common.Popups;

namespace Game.Presentation.UI.Rewards
{
    public sealed class RewardPopupGateway : IDisposable
    {
        private readonly RewardPopupView _popup;
        private readonly PopupRoot _popupRoot;
        private readonly RewardRequestLifecycle _requestLifecycle;
        private readonly RewardPopupStateFactory _stateFactory;

        public RewardPopupGateway(
            RewardPopupView popup,
            PopupRoot popupRoot,
            RewardRequestLifecycle requestLifecycle,
            RewardPopupStateFactory stateFactory)
        {
            _popup = popup ?? throw new ArgumentNullException(nameof(popup));
            _popupRoot = popupRoot ?? throw new ArgumentNullException(nameof(popupRoot));
            _requestLifecycle = requestLifecycle ??
                throw new ArgumentNullException(nameof(requestLifecycle));
            _stateFactory = stateFactory ?? throw new ArgumentNullException(nameof(stateFactory));
            _popup.Selected += HandleSelected;
            _popup.RerollRequested += HandleRerollRequested;
            _popup.AdRerollRequested += HandleAdRerollRequested;
            _popup.TakeAllRequested += HandleTakeAllRequested;
        }

        public event Action<RewardUserIntent> Intent;

        public event Action<PopupView> Hidden
        {
            add => _popup.Hidden += value;
            remove => _popup.Hidden -= value;
        }

        public bool Show(bool animateChoiceChanges, bool isRewardOperationPending)
        {
            bool isBound = _popup.Bind(
                _requestLifecycle.Choices,
                _stateFactory.Create(
                    _requestLifecycle.GuaranteeRarity,
                    _requestLifecycle.CocoonProfile,
                    _requestLifecycle.RollContext,
                    isRewardOperationPending),
                animateChoiceChanges);

            if (!isBound)
                return false;

            _popupRoot.Show(_popup);
            return true;
        }

        public void SetInteractable(bool interactable)
        {
            _popup.SetAllButtonsInteractable(interactable);
        }

        public void Close()
        {
            _popup.Close();
        }

        public void Dispose()
        {
            _popup.Selected -= HandleSelected;
            _popup.RerollRequested -= HandleRerollRequested;
            _popup.AdRerollRequested -= HandleAdRerollRequested;
            _popup.TakeAllRequested -= HandleTakeAllRequested;
            Intent = null;
        }

        private void HandleSelected(RewardChoiceData choice)
        {
            Intent?.Invoke(RewardUserIntent.Select(choice));
        }

        private void HandleRerollRequested()
        {
            Intent?.Invoke(RewardUserIntent.Reroll());
        }

        private void HandleAdRerollRequested()
        {
            Intent?.Invoke(RewardUserIntent.AdReroll());
        }

        private void HandleTakeAllRequested()
        {
            Intent?.Invoke(RewardUserIntent.TakeAll());
        }
    }

}
