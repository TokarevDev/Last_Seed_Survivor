namespace Game.Presentation.UI.Common.Popups
{
    public readonly struct RevivalPopupViewModel
    {
        public RevivalPopupViewModel(
            int attemptsLeft,
            float currentProgressNormalized,
            float remainingLevelNormalized,
            bool canRevive,
            bool isWaiting)
        {
            AttemptsLeft = attemptsLeft;
            CurrentProgressNormalized = currentProgressNormalized;
            RemainingLevelNormalized = remainingLevelNormalized;
            CanRevive = canRevive;
            IsWaiting = isWaiting;
        }

        public int AttemptsLeft { get; }
        public float CurrentProgressNormalized { get; }
        public float RemainingLevelNormalized { get; }
        public bool CanRevive { get; }
        public bool IsWaiting { get; }

        public RevivalPopupViewModel WithWaiting(bool isWaiting)
        {
            return new RevivalPopupViewModel(
                AttemptsLeft,
                CurrentProgressNormalized,
                RemainingLevelNormalized,
                CanRevive,
                isWaiting);
        }
    }
}
