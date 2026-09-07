public interface IRewardChoiceRollService
{
    RewardChoiceRollResult RollStandard(
        CocoonRewardProfile cocoonProfile,
        RewardRollContext rollContext);

    RewardChoiceRollResult RollAdAssisted(
        CocoonRewardProfile cocoonProfile,
        RewardRollContext rollContext);
}
