using System.Collections.Generic;
using System.Reflection;
using Game.Editor.Validation;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ProjectConfigurationValidatorTests
    {
        [Test]
        public void ValidateAllProjectConfigs_CurrentAssetsAreValid()
        {
            Assert.DoesNotThrow(ProjectAssetValidationService.ValidateAllProjectConfigs);
        }

        [Test]
        public void ValidateRequiredReferences_ReportsMissingProjectileAndFallbackAssets()
        {
            RewardVisualCatalog visualCatalog = ScriptableObject.CreateInstance<RewardVisualCatalog>();
            WeaponConfig weaponConfig = ScriptableObject.CreateInstance<WeaponConfig>();
            ProjectileConfig projectileConfig = ScriptableObject.CreateInstance<ProjectileConfig>();
            AcaciaThornWeaponConfig acaciaConfig =
                ScriptableObject.CreateInstance<AcaciaThornWeaponConfig>();
            List<string> errors = new();

            try
            {
                ProjectConfigurationValidator.Validate(
                    rewardDatabase: null,
                    visualCatalog,
                    weaponConfig,
                    projectileConfig,
                    acaciaConfig,
                    "test config",
                    errors);

                Assert.That(errors, Has.Some.Contains("fallback icon profile"));
                Assert.That(errors, Has.Some.Contains("weapon config has no projectile config"));
                Assert.That(errors, Has.Some.Contains("projectile config has no projectile prefab"));
                Assert.That(errors, Has.Some.Contains("Acacia Thorn config has no projectile prefab"));
            }
            finally
            {
                Object.DestroyImmediate(visualCatalog);
                Object.DestroyImmediate(weaponConfig);
                Object.DestroyImmediate(projectileConfig);
                Object.DestroyImmediate(acaciaConfig);
            }
        }

        [Test]
        public void ValidateRewardMappings_ReportsDuplicateStableKeys()
        {
            RewardDatabase database = ScriptableObject.CreateInstance<RewardDatabase>();
            RewardVisualCatalog visualCatalog = ScriptableObject.CreateInstance<RewardVisualCatalog>();
            RewardModifierEntry firstReward = CreateRewardEntry(
                RewardModifierCategory.Damage,
                RewardRarity.Common);
            RewardModifierEntry duplicateReward = CreateRewardEntry(
                RewardModifierCategory.Damage,
                RewardRarity.Common);
            RewardCategoryVisualRule firstRule = CreateVisualRule(RewardModifierCategory.Damage);
            RewardCategoryVisualRule duplicateRule = CreateVisualRule(RewardModifierCategory.Damage);
            SetField(database, "_rewards", new List<RewardModifierEntry>
            {
                firstReward,
                duplicateReward
            });
            SetField(visualCatalog, "_rules", new List<RewardCategoryVisualRule>
            {
                firstRule,
                duplicateRule
            });
            List<string> errors = new();

            try
            {
                ProjectConfigurationValidator.ValidateRewardDatabase(
                    database,
                    "reward database",
                    errors);
                ProjectConfigurationValidator.ValidateRewardVisualCatalog(
                    visualCatalog,
                    "visual catalog",
                    errors);

                Assert.That(errors, Has.Some.Contains("duplicates stable key 'Damage/Common'"));
                Assert.That(errors, Has.Some.Contains("duplicates stable category key 'Damage'"));
            }
            finally
            {
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(visualCatalog);
            }
        }

        private static RewardModifierEntry CreateRewardEntry(
            RewardModifierCategory category,
            RewardRarity rarity)
        {
            RewardModifierEntry entry = new();
            SetField(entry, "_category", category);
            SetField(entry, "_rarity", rarity);
            return entry;
        }

        private static RewardCategoryVisualRule CreateVisualRule(
            RewardModifierCategory category)
        {
            RewardCategoryVisualRule rule = new();
            SetField(rule, "_category", category);
            return rule;
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
