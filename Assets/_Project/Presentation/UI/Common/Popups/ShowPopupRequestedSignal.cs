namespace Game.Presentation.UI.Common.Popups
{
    public sealed class ShowPopupRequestedSignal
    {
        public ShowPopupRequestedSignal(string popupId)
        {
            PopupId = popupId;
        }

        public string PopupId { get; }
    }
}
