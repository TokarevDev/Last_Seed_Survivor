using System;
using System.Collections.Generic;

public sealed class RewardChoiceRollService
{
    private const int StandardGuaranteedSlotCount = 1;
    private const int AdGuaranteedSlotCount = 1;

    private readonly IRewardChoiceRoller _choiceRoller;
    private readonly IRewardRuntimeContextProvider _runtimeContextProvider;
    private readonly IRandomSource _randomSource;

    public RewardChoiceRollService(
        IRewardChoiceRoller choiceRoller,
        IRewardRuntimeContextProvider runtimeContextProvider,
        IRandomSource randomSource)
    {
        _choiceRoller = choiceRoller ??
            throw new ArgumentNullException(nameof(choiceRoller));
        _runtimeContextProvider = runtimeContextProvider ??
            throw new ArgumentNullException(nameof(runtimeContextProvider));
        _randomSource = randomSource ??
            throw new ArgumentNullException(nameof(randomSource));
    }

    public RewardChoiceRollResult RollStandard(
        CocoonRewardProfile cocoonProfile,
        RewardRollContext rollContext)
    {
        RewardRuntimeContext runtimeContext = _runtimeContextProvider.RuntimeContext;
        RewardRarity guaranteeRarity = _choiceRoller.RollGuaranteeRarity(
            runtimeContext,
            cocoonProfile,
            rollContext);

        return Roll(
            runtimeContext,
            cocoonProfile,
            rollContext,
            guaranteeRarity,
            StandardGuaranteedSlotCount);
    }

    public RewardChoiceRollResult RollAdAssisted(
        CocoonRewardProfile cocoonProfile,
        RewardRollContext rollContext)
    {
        RewardRuntimeContext runtimeContext = _runtimeContextProvider.RuntimeContext;
        RewardRarity guaranteeRarity = RewardAdRerollPolicy.RollGuaranteedRarity(
            runtimeContext,
            cocoonProfile,
            rollContext,
            _randomSource);

        return Roll(
            runtimeContext,
            cocoonProfile,
            rollContext.WithPaidAssistRoll(),
            guaranteeRarity,
            AdGuaranteedSlotCount);
    }

    private RewardChoiceRollResult Roll(
        RewardRuntimeContext runtimeContext,
        CocoonRewardProfile cocoonProfile,
        RewardRollContext rollContext,
        RewardRarity guaranteeRarity,
        int guaranteedSlotCount)
    {
        List<RewardChoiceData> choices = _choiceRoller.Roll3(
            runtimeContext,
            cocoonProfile,
            guaranteeRarity,
            guaranteedSlotCount,
            rollContext);

        return new RewardChoiceRollResult(guaranteeRarity, choices);
    }
}
