
using Game.Gameplay.Rewards.Runtime;
using Game.Presentation.UI.Common.Popups;

namespace Game.Presentation.UI.Rewards
{
    using System;

    public sealed class RewardPopupGateway
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
        }

        public event Action<RewardChoiceData> Selected
        {
            add => _popup.Selected += value;
            remove => _popup.Selected -= value;
        }

        public event Action RerollRequested
        {
            add => _popup.RerollRequested += value;
            remove => _popup.RerollRequested -= value;
        }

        public event Action AdRerollRequested
        {
            add => _popup.AdRerollRequested += value;
            remove => _popup.AdRerollRequested -= value;
        }

        public event Action TakeAllRequested
        {
            add => _popup.TakeAllRequested += value;
            remove => _popup.TakeAllRequested -= value;
        }

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
    }

}
