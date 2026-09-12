using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Services;

namespace Game.Tests
{
    public sealed class RewardSelectionPolicyTests
    {
        [Test]
        public void ShouldUseAssistDpsBias_OnlyForReviveOrPaidRoll()
        {
            var normal = new RewardRollContext(0f, 0f, false);
            var revived = new RewardRollContext(0f, 0f, true);
            RewardRollContext paid = normal.WithPaidAssistRoll();

            Assert.That(RewardSelectionPolicy.ShouldUseAssistDpsBias(normal), Is.False);
            Assert.That(RewardSelectionPolicy.ShouldUseAssistDpsBias(revived), Is.True);
            Assert.That(RewardSelectionPolicy.ShouldUseAssistDpsBias(paid), Is.True);
        }

        [Test]
        public void IsEligible_AnyModeAcceptsPositiveWeightEntry()
        {
            var entry = new RewardModifierEntry();

            bool isEligible = RewardSelectionPolicy.IsEligible(
                entry,
                new HashSet<RewardModifierCategory>(),
                new HashSet<int>(),
                RewardPickMode.Any,
                RewardWeaponGroup.None);

            Assert.That(isEligible, Is.True);
        }

        [Test]
        public void IsEligible_RejectsNullEntry()
        {
            bool isEligible = RewardSelectionPolicy.IsEligible(
                null,
                new HashSet<RewardModifierCategory>(),
                new HashSet<int>(),
                RewardPickMode.Any,
                RewardWeaponGroup.None);

            Assert.That(isEligible, Is.False);
        }

        [Test]
        public void GetEffectiveWeight_UsesConfiguredAssistMultipliers()
        {
            var entry = new RewardModifierEntry();
            SetField(entry, "_category", RewardModifierCategory.Damage);
            var tuning = new RewardSelectionTuning(
                postRevivePrimaryDpsWeightMultiplier: 7f,
                postReviveSecondaryDpsWeightMultiplier: 2f,
                paidAssistPrimaryDpsWeightMultiplier: 5f,
                paidAssistSecondaryDpsWeightMultiplier: 1.5f);
            var revived = new RewardRollContext(0f, 0f, true);
            RewardRollContext paid = new RewardRollContext(0f, 0f, false)
                .WithPaidAssistRoll();

            float revivedWeight = RewardSelectionPolicy.GetEffectiveWeight(
                entry,
                revived,
                tuning);
            float paidWeight = RewardSelectionPolicy.GetEffectiveWeight(
                entry,
                paid,
                tuning);

            Assert.That(revivedWeight, Is.EqualTo(7f));
            Assert.That(paidWeight, Is.EqualTo(5f));
        }

        private static void SetField<TValue>(object target, string fieldName, TValue value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing test field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
