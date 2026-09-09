using System;
using System.Collections.Generic;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards.Visuals;

namespace Game.Editor.Validation
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

        public static void ValidateWeaponConfig(
            WeaponConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config != null && config.Projectile == null)
                errors.Add($"{context}: weapon config has no projectile config.");
        }

        public static void ValidateProjectileConfig(
            ProjectileConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config != null && config.Prefab == null)
                errors.Add($"{context}: projectile config has no projectile prefab.");
        }

        public static void ValidateAcaciaThornConfig(
            AcaciaThornWeaponConfig config,
            string context,
            ICollection<string> errors)
        {
            if (config != null && config.ProjectilePrefab == null)
                errors.Add($"{context}: Acacia Thorn config has no projectile prefab.");
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
