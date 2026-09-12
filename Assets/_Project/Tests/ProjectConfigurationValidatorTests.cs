using System.Collections.Generic;
using System.Reflection;
using Game.EditorTools.Validation;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards;
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

        [Test]
        public void ValidateNumericConfigFamilies_ReportsNonFiniteAndOutOfRangeValues()
        {
            WeaponConfig weaponConfig = ScriptableObject.CreateInstance<WeaponConfig>();
            ProjectileConfig projectileConfig = ScriptableObject.CreateInstance<ProjectileConfig>();
            AcaciaThornWeaponConfig acaciaConfig =
                ScriptableObject.CreateInstance<AcaciaThornWeaponConfig>();
            WormHpScalingConfig hpConfig = ScriptableObject.CreateInstance<WormHpScalingConfig>();
            WormPressureConfig pressureConfig = ScriptableObject.CreateInstance<WormPressureConfig>();
            WormMovementConfig movementConfig = ScriptableObject.CreateInstance<WormMovementConfig>();
            RewardPopupAnimationConfig animationConfig =
                ScriptableObject.CreateInstance<RewardPopupAnimationConfig>();
            RewardPopupActionPresentationConfig actionConfig =
                ScriptableObject.CreateInstance<RewardPopupActionPresentationConfig>();
            List<string> errors = new();

            try
            {
                weaponConfig.FireRate = float.NaN;
                SetField(projectileConfig, "_lifeTime", float.PositiveInfinity);
                acaciaConfig.EstimatedSplitHitChance = 2f;
                SetField(hpConfig, "_hpMultiplier", float.NaN);
                SetField(
                    hpConfig,
                    "_pressureByProgress",
                    new AnimationCurve(
                        new Keyframe(0.25f, 0f),
                        new Keyframe(0.75f, 1f)));
                SetField(pressureConfig, "_sampleInterval", float.PositiveInfinity);
                SetField(movementConfig, "_reviveDecelerationPathFraction", 1f);
                SetField(animationConfig, "_rootFadeDuration", -1f);
                SetField(actionConfig, "_singleActionButtonAnchoredX", float.NaN);

                ProjectConfigurationValidator.ValidateWeaponConfig(
                    weaponConfig,
                    "weapon",
                    errors);
                ProjectConfigurationValidator.ValidateProjectileConfig(
                    projectileConfig,
                    "projectile",
                    errors);
                ProjectConfigurationValidator.ValidateAcaciaThornConfig(
                    acaciaConfig,
                    "acacia",
                    errors);
                ProjectConfigurationValidator.ValidateWormHpScalingConfig(
                    hpConfig,
                    "hp",
                    errors);
                ProjectConfigurationValidator.ValidateWormPressureConfig(
                    pressureConfig,
                    "pressure",
                    errors);
                ProjectConfigurationValidator.ValidateWormMovementConfig(
                    movementConfig,
                    "movement",
                    errors);
                ProjectConfigurationValidator.ValidateRewardPopupAnimationConfig(
                    animationConfig,
                    "animation",
                    errors);
                ProjectConfigurationValidator.ValidateRewardPopupActionPresentationConfig(
                    actionConfig,
                    "action",
                    errors);

                Assert.That(errors, Has.Some.Contains("weapon: FireRate must be finite"));
                Assert.That(errors, Has.Some.Contains("projectile: _lifeTime must be finite"));
                Assert.That(errors, Has.Some.Contains("acacia: EstimatedSplitHitChance must be in"));
                Assert.That(errors, Has.Some.Contains("hp: _hpMultiplier must be finite"));
                Assert.That(
                    errors,
                    Has.Some.Contains("pressure curve must cover normalized progress"));
                Assert.That(errors, Has.Some.Contains("pressure curve key [0] value must be positive"));
                Assert.That(errors, Has.Some.Contains("pressure: _sampleInterval must be finite"));
                Assert.That(
                    errors,
                    Has.Some.Contains("movement: _reviveDecelerationPathFraction must be in"));
                Assert.That(errors, Has.Some.Contains("animation: _rootFadeDuration must be at least"));
                Assert.That(
                    errors,
                    Has.Some.Contains("action: _singleActionButtonAnchoredX must be finite"));
            }
            finally
            {
                Object.DestroyImmediate(weaponConfig);
                Object.DestroyImmediate(projectileConfig);
                Object.DestroyImmediate(acaciaConfig);
                Object.DestroyImmediate(hpConfig);
                Object.DestroyImmediate(pressureConfig);
                Object.DestroyImmediate(movementConfig);
                Object.DestroyImmediate(animationConfig);
                Object.DestroyImmediate(actionConfig);
            }
        }

        [Test]
        public void ValidateRewardPresentationConfigs_ReportsMissingSpriteAndMalformedFormat()
        {
            RewardIconProfile iconProfile = ScriptableObject.CreateInstance<RewardIconProfile>();
            RewardPopupActionPresentationConfig actionConfig =
                ScriptableObject.CreateInstance<RewardPopupActionPresentationConfig>();
            SetField(actionConfig, "_attemptsFormat", "attempts: {broken}");
            SetField(actionConfig, "_guaranteeFormat", "guarantee");
            List<string> errors = new();

            try
            {
                ProjectConfigurationValidator.ValidateRewardIconProfile(
                    iconProfile,
                    "icon",
                    errors);
                ProjectConfigurationValidator.ValidateRewardPopupActionPresentationConfig(
                    actionConfig,
                    "action",
                    errors);

                Assert.That(errors, Has.Some.Contains("reward icon profile has no sprite"));
                Assert.That(errors, Has.Some.Contains("attempts format is not a valid"));
                Assert.That(errors, Has.Some.Contains("guarantee format must contain the '{0}'"));
            }
            finally
            {
                Object.DestroyImmediate(iconProfile);
                Object.DestroyImmediate(actionConfig);
            }
        }

        [Test]
        public void ValidateRewardDatabase_ReportsInvalidCocoonCollection()
        {
            RewardDatabase database = ScriptableObject.CreateInstance<RewardDatabase>();
            SetField(
                database,
                "_cocoonProfiles",
                new List<CocoonRewardProfile> { null });
            List<string> errors = new();

            try
            {
                ProjectConfigurationValidator.ValidateRewardDatabase(
                    database,
                    "rewards",
                    errors);

                Assert.That(errors, Has.Some.Contains("_cocoonProfiles[0] is null"));
                Assert.That(errors, Has.Some.Contains("no spawnable cocoon profile"));
            }
            finally
            {
                Object.DestroyImmediate(database);
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
