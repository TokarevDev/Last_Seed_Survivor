using System;

namespace Game.Gameplay.Rewards.Runtime
{
    public enum RewardUserIntentType
    {
        Select = 0,
        Reroll = 1,
        AdReroll = 2,
        TakeAll = 3
    }

    public readonly struct RewardUserIntent
    {
        private RewardUserIntent(
            RewardUserIntentType type,
            RewardChoiceData choice)
        {
            Type = type;
            Choice = choice;
        }

        public RewardUserIntentType Type { get; }
        public RewardChoiceData Choice { get; }

        public static RewardUserIntent Select(RewardChoiceData choice)
        {
            return new RewardUserIntent(
                RewardUserIntentType.Select,
                choice ?? throw new ArgumentNullException(nameof(choice)));
        }

        public static RewardUserIntent Reroll()
        {
            return new RewardUserIntent(RewardUserIntentType.Reroll, null);
        }

        public static RewardUserIntent AdReroll()
        {
            return new RewardUserIntent(RewardUserIntentType.AdReroll, null);
        }

        public static RewardUserIntent TakeAll()
        {
            return new RewardUserIntent(RewardUserIntentType.TakeAll, null);
        }
    }
}
