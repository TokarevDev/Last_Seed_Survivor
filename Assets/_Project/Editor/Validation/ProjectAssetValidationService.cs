using System;
using System.Collections.Generic;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Rewards.Data;
using Game.Presentation.UI.Rewards.Visuals;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Validation
{
    public static class ProjectAssetValidationService
    {
        private const string ProjectAssetRoot = "Assets/_Project";

        public static void ValidateAllProjectConfigs()
        {
            List<string> errors = new();
            int validatedAssetCount = 0;

            validatedAssetCount += ValidateAssets<RewardDatabase>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardDatabase(
                    asset,
                    path,
                    errors));
            validatedAssetCount += ValidateAssets<RewardVisualCatalog>(
                (asset, path) => ProjectConfigurationValidator.ValidateRewardVisualCatalog(
                    asset,
                    path,
                    errors));
            validatedAssetCount += ValidateAssets<WeaponConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateWeaponConfig(
                    asset,
                    path,
                    errors));
            validatedAssetCount += ValidateAssets<ProjectileConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateProjectileConfig(
                    asset,
                    path,
                    errors));
            validatedAssetCount += ValidateAssets<AcaciaThornWeaponConfig>(
                (asset, path) => ProjectConfigurationValidator.ValidateAcaciaThornConfig(
                    asset,
                    path,
                    errors));

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

        private static int ValidateAssets<TAsset>(Action<TAsset, string> validate)
            where TAsset : ScriptableObject
        {
            string[] assetGuids = AssetDatabase.FindAssets(
                $"t:{typeof(TAsset).Name}",
                new[] { ProjectAssetRoot });

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
