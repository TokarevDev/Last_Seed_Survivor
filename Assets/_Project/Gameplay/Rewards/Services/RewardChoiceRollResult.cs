using System.Collections.Generic;

public readonly struct RewardChoiceRollResult
{
    public RewardChoiceRollResult(
        RewardRarity guaranteeRarity,
        List<RewardChoiceData> choices)
    {
        GuaranteeRarity = guaranteeRarity;
        Choices = choices;
    }

    public RewardRarity GuaranteeRarity { get; }
    public List<RewardChoiceData> Choices { get; }
    public bool HasChoices => Choices != null && Choices.Count > 0;
}
