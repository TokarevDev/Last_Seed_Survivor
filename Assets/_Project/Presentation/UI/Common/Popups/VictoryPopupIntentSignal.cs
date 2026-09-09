namespace Game.Presentation.UI.Common.Popups
{
    public enum VictoryPopupIntent
    {
        Accept,
        DoubleReward
    }

    public sealed class VictoryPopupIntentSignal
    {
        public VictoryPopupIntentSignal(VictoryPopupIntent intent)
        {
            Intent = intent;
        }

        public VictoryPopupIntent Intent { get; }
    }
}
