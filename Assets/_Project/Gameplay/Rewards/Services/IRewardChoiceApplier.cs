public interface IRewardChoiceApplier
{
    RewardRuntimeContext RuntimeContext { get; }

    void Apply(RewardChoiceData choice);
}
