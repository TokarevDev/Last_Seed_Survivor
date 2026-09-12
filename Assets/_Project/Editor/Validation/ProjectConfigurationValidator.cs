using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards;
using Game.Presentation.UI.Rewards.Visuals;
using UnityEngine;

namespace Game.EditorTools.Validation
{
    public static class ProjectConfigurationValidator
    {
        public static void Validate(
            RewardDatabase rewardDatabase,
            RewardVisualCatalog visualCatalog,
            WeaponConfig weaponConfig,
            ProjectileConfig projectileConfig,
            AcaciaThornWeaponConfig acaciaThornConfig,
            string context,
            ICollection<string> errors)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            ValidateRewardDatabase(rewardDatabase, context, errors);
            ValidateRewardVisualCatalog(visualCatalog, context, errors);
            ValidateWeaponConfig(weaponConfig, context, errors);
            ValidateProjectileConfig(projectileConfig, context, errors);
            ValidateAcaciaThornConfig(acaciaThornConfig, context, errors);
        }

        public static void ValidateRewardDatabase(
            RewardDatabase database,
            string context,
            ICollection<string> errors)
        {
            if (database == null)
                return;

            SerializedConfigurationValueValidator.Validate(database, context, errors);
            ValidateCocoonProfiles(database.EditorCocoonProfiles, context, errors);

            IReadOnlyList<RewardModifierEntry> rewards = database.EditorRewards;

            if (rewards == null || rewards.Count == 0)
            {
                errors.Add($"{context}: reward database has no reward entries.");
                return;
            }

            HashSet<RewardModifierCategory> configuredCategories = new();
            HashSet<string> configuredKeys = new(StringComparer.Ordinal);

            for (int index = 0; index < rewards.Count; index++)
            {
                RewardModifierEntry entry = rewards[index];
                string entryContext = $"{context}: reward entry [{index}]";

                if (entry == null)
                {
                    errors.Add($"{entryContext} is null.");
                    continue;
                }

                if (!IsStableCategory(entry.Category))
                {
                    errors.Add($"{entryContext} has unknown or empty category '{entry.Category}'.");
                    continue;
                }

                if (!Enum.IsDefined(typeof(RewardRarity), entry.Rarity))
                {
                    errors.Add($"{entryContext} has unknown rarity '{entry.Rarity}'.");
                    continue;
                }

                if (entry.Effect == null)
                    errors.Add($"{entryContext} has no reward effect.");

                if (entry.Weight <= 0f)
                    errors.Add($"{entryContext} must have a positive selection weight.");

                if (string.IsNullOrWhiteSpace(entry.Title))
                    errors.Add($"{entryContext} has no title.");

                if (string.IsNullOrWhiteSpace(entry.ValueText))
                    errors.Add($"{entryContext} has no value text.");

                configuredCategories.Add(entry.Category);
                string stableKey = $"{entry.Category}/{entry.Rarity}";

                if (!configuredKeys.Add(stableKey))
                    errors.Add($"{entryContext} duplicates stable key '{stableKey}'.");
            }

            AppendMissingCategoryErrors(
                configuredCategories,
                context,
                "reward database",
                errors);
        }

        public static void ValidateRewardVisualCatalog(
            RewardVisualCatalog catalog,
            string context,
            ICollection<string> errors)
        {
            if (catalog == null)
                return;

            SerializedConfigurationValueValidator.Validate(catalog, context, errors);

            if (catalog.EditorFallbackIconProfile == null)
                errors.Add($"{context}: reward visual catalog has no fallback icon profile.");

            IReadOnlyList<RewardCategoryVisualRule> rules = catalog.EditorRules;

            if (rules == null || rules.Count == 0)
            {
                errors.Add($"{context}: reward visual catalog has no category rules.");
                return;
            }

            HashSet<RewardModifierCategory> configuredCategories = new();

            for (int index = 0; index < rules.Count; index++)
            {
                RewardCategoryVisualRule rule = rules[index];
                string ruleContext = $"{context}: reward visual rule [{index}]";

                if (rule == null)
                {
                    errors.Add($"{ruleContext} is null.");
                    continue;
                }

                if (!IsStableCategory(rule.Category))
                {
                    errors.Add($"{ruleContext} has unknown or empty category '{rule.Category}'.");
                    continue;
                }

                if (!configuredCategories.Add(rule.Category))
                    errors.Add($"{ruleContext} duplicates stable category key '{rule.Category}'.");
            }

            AppendMissingCategoryErrors(
                configuredCategories,
                context,
                "reward visual catalog",
                errors);
        }

        public static void ValidateRewardIconProfile(
            RewardIconProfile profile,
            string context,
            ICollection<string> errors)
        {
            if (profile == null)
                return;

            SerializedConfigurationValueValidator.Validate(profile, context, errors);

            if (profile.Sprite == null)
                errors.Add($"{context}: reward icon profile has no sprite.");
        }

        public static void ValidateWeaponConfig(
            WeaponConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config == null)
                return;

            SerializedConfigurationValueValidator.Validate(config, context, errors);

            if (config.FireRate <= 0f)
                errors.Add($"{context}: weapon fire rate must be positive.");

            if (config.Projectile == null)
                errors.Add($"{context}: weapon config has no projectile config.");
        }

        public static void ValidateProjectileConfig(
            ProjectileConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config == null)
                return;

            SerializedConfigurationValueValidator.Validate(config, context, errors);

            if (config.Damage <= 0)
                errors.Add($"{context}: projectile damage must be positive.");

            if (config.Penetration < 0)
                errors.Add($"{context}: projectile penetration cannot be negative.");

            if (config.LifeTime <= 0f)
                errors.Add($"{context}: projectile lifetime must be positive.");

            if (config.Speed <= 0f)
                errors.Add($"{context}: projectile speed must be positive.");

            if (config.BounceCount < 0)
                errors.Add($"{context}: projectile bounce count cannot be negative.");

            if (config.Prefab == null)
                errors.Add($"{context}: projectile config has no projectile prefab.");
        }

        public static void ValidateAcaciaThornConfig(
            AcaciaThornWeaponConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config == null)
                return;

            SerializedConfigurationValueValidator.Validate(config, context, errors);

            if (config.ProjectilePrefab == null)
                errors.Add($"{context}: Acacia Thorn config has no projectile prefab.");
        }

        public static void ValidateWormHpScalingConfig(
            WormHpScalingConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config == null)
                return;

            SerializedConfigurationValueValidator.Validate(config, context, errors);

            if (config.MinHp > config.MaxHp)
                errors.Add($"{context}: minimum HP cannot exceed maximum HP.");

            ValidatePositiveNormalizedCurve(
                config.EditorTargetSectionLifetimeByProgress,
                $"{context}: target section lifetime curve",
                errors);
            ValidatePositiveNormalizedCurve(
                config.EditorPressureByProgress,
                $"{context}: pressure curve",
                errors);
        }

        public static void ValidateWormPressureConfig(
            WormPressureConfig config,
            string context,
            ICollection<string> errors)
        {
            SerializedConfigurationValueValidator.Validate(config, context, errors);
        }

        public static void ValidateWormMovementConfig(
            WormMovementConfig config,
            string context,
            ICollection<string> errors)
        {
            SerializedConfigurationValueValidator.Validate(config, context, errors);
        }

        public static void ValidateRewardPopupAnimationConfig(
            RewardPopupAnimationConfig config,
            string context,
            ICollection<string> errors)
        {
            SerializedConfigurationValueValidator.Validate(config, context, errors);
        }

        public static void ValidateRewardPopupActionPresentationConfig(
            RewardPopupActionPresentationConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config == null)
                return;

            SerializedConfigurationValueValidator.Validate(config, context, errors);
            RewardPopupActionControls.TextSettings textSettings = config.CreateTextSettings();
            ValidateCompositeFormat(
                textSettings.AttemptsFormat,
                $"{context}: attempts format",
                errors);
            ValidateCompositeFormat(
                textSettings.GuaranteeFormat,
                $"{context}: guarantee format",
                errors);
            ValidateCompositeFormat(
                textSettings.AdGuaranteeFormat,
                $"{context}: ad guarantee format",
                errors);
        }

        private static void ValidateCocoonProfiles(
            IReadOnlyList<CocoonRewardProfile> profiles,
            string context,
            ICollection<string> errors)
        {
            if (profiles == null || profiles.Count == 0)
            {
                errors.Add($"{context}: reward database has no cocoon profiles.");
                return;
            }

            HashSet<string> displayNames = new(StringComparer.Ordinal);
            bool hasSpawnableProfile = false;

            for (int profileIndex = 0; profileIndex < profiles.Count; profileIndex++)
            {
                CocoonRewardProfile profile = profiles[profileIndex];
                string profileContext = $"{context}: cocoon profile [{profileIndex}]";

                if (profile == null)
                    continue;

                if (string.IsNullOrWhiteSpace(profile.DisplayName))
                {
                    errors.Add($"{profileContext} has no display name.");
                }
                else if (!displayNames.Add(profile.DisplayName))
                {
                    errors.Add(
                        $"{profileContext} duplicates display name " +
                        $"'{profile.DisplayName}'.");
                }

                hasSpawnableProfile |= profile.SpawnWeight > 0f;
                IReadOnlyList<RewardRaritySlot> slots = profile.RaritySlots;

                if (slots == null || slots.Count == 0)
                {
                    errors.Add($"{profileContext} has no rarity slots.");
                    continue;
                }

                for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
                {
                    RewardRaritySlot slot = slots[slotIndex];

                    if (slot == null)
                        continue;

                    if (!Enum.IsDefined(typeof(RewardRarity), slot.Rarity))
                    {
                        errors.Add(
                            $"{profileContext}: rarity slot [{slotIndex}] has unknown " +
                            $"primary rarity '{slot.Rarity}'.");
                    }

                    if (!Enum.IsDefined(typeof(RewardRarity), slot.AlternateRarity))
                    {
                        errors.Add(
                            $"{profileContext}: rarity slot [{slotIndex}] has unknown " +
                            $"alternate rarity '{slot.AlternateRarity}'.");
                    }
                }
            }

            if (!hasSpawnableProfile)
                errors.Add($"{context}: reward database has no spawnable cocoon profile.");
        }

        private static void ValidateCompositeFormat(
            string format,
            string context,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                errors.Add($"{context} is empty.");
                return;
            }

            try
            {
                const string FormatProbe = "__value__";
                string formatted = string.Format(
                    CultureInfo.InvariantCulture,
                    format,
                    FormatProbe);

                if (!formatted.Contains(FormatProbe, StringComparison.Ordinal))
                    errors.Add($"{context} must contain the '{{0}}' value placeholder.");
            }
            catch (FormatException)
            {
                errors.Add($"{context} is not a valid one-argument composite format.");
            }
        }

        private static void ValidatePositiveNormalizedCurve(
            AnimationCurve curve,
            string context,
            ICollection<string> errors)
        {
            if (curve == null || curve.length == 0)
                return;

            Keyframe[] keys = curve.keys;

            if (keys[0].time > 0f || keys[keys.Length - 1].time < 1f)
                errors.Add($"{context} must cover normalized progress [0, 1].");

            for (int index = 0; index < keys.Length; index++)
            {
                Keyframe key = keys[index];

                if (key.time < 0f || key.time > 1f)
                    errors.Add($"{context} key [{index}] time must be in [0, 1].");

                if (key.value <= 0f)
                    errors.Add($"{context} key [{index}] value must be positive.");
            }
        }

        private static bool IsStableCategory(RewardModifierCategory category)
        {
            return category != RewardModifierCategory.None &&
                   Enum.IsDefined(typeof(RewardModifierCategory), category);
        }

        private static void AppendMissingCategoryErrors(
            HashSet<RewardModifierCategory> configuredCategories,
            string context,
            string configName,
            ICollection<string> errors)
        {
            Array categories = Enum.GetValues(typeof(RewardModifierCategory));

            for (int index = 0; index < categories.Length; index++)
            {
                RewardModifierCategory category = (RewardModifierCategory)categories.GetValue(index);

                if (category == RewardModifierCategory.None || configuredCategories.Contains(category))
                    continue;

                errors.Add($"{context}: {configName} is missing required category '{category}'.");
            }
        }
    }
}
