using System.Collections.Generic;
using NUnit.Framework;

using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Rewards.Data;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Rewards.Services;

namespace Game.Tests
{
    public sealed class RewardChoiceRollServiceTests
    {
        [Test]
        public void RollStandard_UsesComputedGuaranteeAndOriginalContext()
        {
            RewardRuntimeContext runtimeContext = CreateRuntimeContext();
            FakeChoiceRoller roller = new() { GuaranteeRarity = RewardRarity.Legendary };
            TestRandomSource randomSource = new(values: new[] { 0.25f });
            RewardChoiceRollService service = new(
                roller,
                new FakeRuntimeContextProvider(runtimeContext),
                randomSource);
            RewardRollContext rollContext = new(0.2f, 0.3f, false);

            RewardChoiceRollResult result = service.RollStandard(null, rollContext);

            Assert.That(roller.GuaranteeCalls, Is.EqualTo(1));
            Assert.That(roller.LastRuntimeContext, Is.SameAs(runtimeContext));
            Assert.That(roller.LastRollContext.IsPaidAssistRoll, Is.False);
            Assert.That(roller.LastGuaranteedRarity, Is.EqualTo(RewardRarity.Legendary));
            Assert.That(result.GuaranteeRarity, Is.EqualTo(RewardRarity.Legendary));
            Assert.That(result.HasChoices, Is.True);
            Assert.That(randomSource.Calls, Is.Zero);
        }

        [Test]
        public void RollAdAssisted_UsesPolicyGuaranteeAndPaidAssistContext()
        {
            RewardRuntimeContext runtimeContext = CreateRuntimeContext();
            FakeChoiceRoller roller = new();
            TestRandomSource randomSource = new(values: new[] { 0.5f });
            RewardChoiceRollService service = new(
                roller,
                new FakeRuntimeContextProvider(runtimeContext),
                randomSource);
            RewardRollContext rollContext = new(0f, 0f, false);

            RewardChoiceRollResult result = service.RollAdAssisted(null, rollContext);

            Assert.That(roller.GuaranteeCalls, Is.Zero);
            Assert.That(roller.LastRuntimeContext, Is.SameAs(runtimeContext));
            Assert.That(roller.LastRollContext.IsPaidAssistRoll, Is.True);
            Assert.That(roller.LastGuaranteedRarity, Is.EqualTo(RewardRarity.Rare));
            Assert.That(roller.LastGuaranteedSlotCount, Is.EqualTo(1));
            Assert.That(result.GuaranteeRarity, Is.EqualTo(RewardRarity.Rare));
            Assert.That(result.HasChoices, Is.True);
            Assert.That(randomSource.Calls, Is.EqualTo(1));
        }

        private static RewardRuntimeContext CreateRuntimeContext()
        {
            return new RewardRuntimeContext(
                (WeaponRuntimeState)null,
                (AcaciaThornRuntimeState)null);
        }

        private sealed class FakeRuntimeContextProvider : IRewardRuntimeContextProvider
        {
            public FakeRuntimeContextProvider(RewardRuntimeContext runtimeContext)
            {
                RuntimeContext = runtimeContext;
            }

            public RewardRuntimeContext RuntimeContext { get; }
        }

        private sealed class FakeChoiceRoller : IRewardChoiceRoller
        {
            public RewardRarity GuaranteeRarity { get; set; } = RewardRarity.Common;
            public int GuaranteeCalls { get; private set; }
            public RewardRuntimeContext LastRuntimeContext { get; private set; }
            public RewardRarity? LastGuaranteedRarity { get; private set; }
            public int LastGuaranteedSlotCount { get; private set; }
            public RewardRollContext LastRollContext { get; private set; }

            public List<RewardChoiceData> Roll3(
                RewardRuntimeContext context,
                CocoonRewardProfile cocoonProfile = null,
                RewardRarity? guaranteedRarity = null,
                int guaranteedRaritySlotCount = 1,
                RewardRollContext rollContext = default)
            {
                LastRuntimeContext = context;
                LastGuaranteedRarity = guaranteedRarity;
                LastGuaranteedSlotCount = guaranteedRaritySlotCount;
                LastRollContext = rollContext;
                return new List<RewardChoiceData> { new(new RewardModifierEntry()) };
            }

            public RewardRarity RollGuaranteeRarity(
                RewardRuntimeContext context,
                CocoonRewardProfile cocoonProfile = null,
                RewardRollContext rollContext = default)
            {
                GuaranteeCalls++;
                LastRuntimeContext = context;
                LastRollContext = rollContext;
                return GuaranteeRarity;
            }
        }
    }
}
