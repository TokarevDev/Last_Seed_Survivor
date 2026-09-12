using System;
using System.Collections.Generic;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards;
using Game.Presentation.UI.Rewards.Visuals;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Validation
{
    public static class ProjectAssetValidationService
    {
        private const string ProjectAssetRoot = "Assets/_Project";

        public static void ValidateAllProjectConfigs()
        {
            List<string> errors = new();
            int validatedAssetCount = 0;

            validatedAssetCount += ValidateRequiredAssets<RewardDatabase>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardDatabase(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<RewardVisualCatalog>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardVisualCatalog(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<RewardIconProfile>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardIconProfile(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<WeaponConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateWeaponConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<ProjectileConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateProjectileConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<AcaciaThornWeaponConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateAcaciaThornConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<WormHpScalingConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateWormHpScalingConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<WormPressureConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateWormPressureConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<WormMovementConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateWormMovementConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<RewardPopupAnimationConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardPopupAnimationConfig(
                    asset,
                    path,
                    errors),
                errors);
            validatedAssetCount += ValidateRequiredAssets<RewardPopupActionPresentationConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardPopupActionPresentationConfig(
                    asset,
                    path,
                    errors),
                errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Last Seed project configuration validation failed:\n- " +
                    string.Join("\n- ", errors));
            }

            Debug.Log(
                $"Last Seed project configuration validation succeeded for " +
                $"{validatedAssetCount} config assets.");
        }

        private static int ValidateRequiredAssets<TAsset>(
            Action<TAsset, string> validate,
            ICollection<string> errors)
            where TAsset : ScriptableObject
        {
            string[] assetGuids = AssetDatabase.FindAssets(
                $"t:{typeof(TAsset).Name}",
                new[] { ProjectAssetRoot });

            if (assetGuids.Length == 0)
            {
                errors.Add(
                    $"{ProjectAssetRoot}: required config family " +
                    $"'{typeof(TAsset).Name}' has no assets.");
                return 0;
            }

            for (int index = 0; index < assetGuids.Length; index++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(assetPath);

                if (asset == null)
                    throw new InvalidOperationException(
                        $"Could not load {typeof(TAsset).Name} at '{assetPath}'.");

                validate(asset, assetPath);
            }

            return assetGuids.Length;
        }
    }
}
