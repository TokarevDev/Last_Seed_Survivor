using Game.Core.Combat;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public static class ProjectileWeaponPowerEstimator
    {
        private const float MinimumShotCycleTime = 0.01f;

        public static WeaponPowerSnapshot Estimate(
            WeaponConfig config,
            WeaponRuntimeState runtimeState)
        {
            if (config == null || config.Projectile == null || runtimeState == null)
                return WeaponPowerSnapshot.Invalid;

            int damagePerProjectile = WeaponExpectedDamageEstimator.Estimate(
                config.Projectile.Damage,
                runtimeState.DamageMultiplier,
                runtimeState.CriticalChance,
                runtimeState.CriticalDamageMultiplier);
            int projectilesPerShot = Mathf.Max(1, runtimeState.ParallelProjectileCount);
            float damageEventsPerProjectile = EstimateDamageEventsPerProjectile(
                config.Projectile,
                runtimeState);
            int salvoShots = Mathf.Max(1, 1 + runtimeState.SalvoExtraShots);
            float shotCycleTime = EstimateShotCycleTime(config, runtimeState, salvoShots);
            float estimatedDps =
                damagePerProjectile *
                projectilesPerShot *
                damageEventsPerProjectile *
                salvoShots /
                shotCycleTime;

            return new WeaponPowerSnapshot(
                true,
                estimatedDps,
                damagePerProjectile,
                projectilesPerShot,
                salvoShots,
                shotCycleTime);
        }

        private static float EstimateDamageEventsPerProjectile(
            ProjectileConfig projectileConfig,
            WeaponRuntimeState runtimeState)
        {
            return Mathf.Max(
                1,
                1 + projectileConfig.Penetration + runtimeState.PenetrationBonus);
        }

        private static float EstimateShotCycleTime(
            WeaponConfig config,
            WeaponRuntimeState runtimeState,
            int salvoShots)
        {
            float fireRateBonus = Mathf.Min(
                runtimeState.FireRateBonus,
                config.MaxFireRateBonus);
            float shotCooldown = WeaponDerivedStatsCalculator.CalculateCooldown(
                config.FireRate,
                config.MinShotCooldown,
                fireRateBonus);
            float salvoTime = Mathf.Max(0, salvoShots - 1) *
                Mathf.Max(MinimumShotCycleTime, runtimeState.SalvoInterval);

            return Mathf.Max(MinimumShotCycleTime, shotCooldown + salvoTime);
        }
    }
}
