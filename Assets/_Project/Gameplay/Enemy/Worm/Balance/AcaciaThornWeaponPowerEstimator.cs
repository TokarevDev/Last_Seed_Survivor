using Game.Core.Combat;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public static class AcaciaThornWeaponPowerEstimator
    {
        private const float MinimumShotCycleTime = 0.01f;

        public static WeaponPowerSnapshot Estimate(
            AcaciaThornWeaponConfig config,
            AcaciaThornRuntimeState runtimeState)
        {
            if (config == null || runtimeState == null || !runtimeState.IsUnlocked)
                return WeaponPowerSnapshot.Invalid;

            int damagePerProjectile = WeaponExpectedDamageEstimator.Estimate(
                runtimeState.BaseDamage,
                runtimeState.DamageMultiplier,
                runtimeState.CriticalChance,
                runtimeState.CriticalDamageMultiplier);
            int splitCount = Mathf.Max(0, config.BaseSplitCount);
            int salvoShots = Mathf.Max(1, 1 + runtimeState.SalvoExtraShots);
            float estimatedHitsPerShot = 1f +
                splitCount * config.EstimatedSplitHitChance +
                config.BounceCount * config.EstimatedBounceHitChance;
            float shotCycleTime = EstimateShotCycleTime(config, runtimeState, salvoShots);
            float estimatedDps = damagePerProjectile *
                Mathf.Max(1f, estimatedHitsPerShot) /
                shotCycleTime *
                salvoShots;

            return new WeaponPowerSnapshot(
                true,
                estimatedDps,
                damagePerProjectile,
                1 + splitCount,
                salvoShots,
                shotCycleTime);
        }

        private static float EstimateShotCycleTime(
            AcaciaThornWeaponConfig config,
            AcaciaThornRuntimeState runtimeState,
            int salvoShots)
        {
            float fireRateBonus = Mathf.Min(
                runtimeState.FireRateBonus,
                config.MaxFireRateBonus);
            float cooldown = WeaponDerivedStatsCalculator.CalculateCooldown(
                config.Cooldown,
                config.MinCooldown,
                fireRateBonus);
            float salvoTime = Mathf.Max(0, salvoShots - 1) *
                Mathf.Max(MinimumShotCycleTime, config.SalvoInterval);

            return Mathf.Max(MinimumShotCycleTime, cooldown + salvoTime);
        }
    }
}
