using System;
using System.Collections.Generic;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Enemy.Worm.Balance;
using UnityEngine;

namespace Game.EditorTools.Worm
{
    internal static class WormBalanceWeaponSimulation
    {
        public static WeaponRuntimeState CreateMainWeaponState(WeaponConfig config)
        {
            WeaponRuntimeState state = new();

            if (config == null)
                return state;

            state.SetProgressionLimits(
                config.MaxDamageMultiplier,
                config.MaxFireRateBonus,
                config.MaxProjectileSpeedBonus,
                config.MaxCriticalChance,
                config.MaxCriticalDamageMultiplier,
                config.MaxPenetrationBonus,
                config.MaxParallelProjectiles,
                config.MaxSalvoExtraShots);

            return state;
        }

        public static AcaciaThornRuntimeState CreateAcaciaThornState(
            AcaciaThornWeaponConfig config)
        {
            AcaciaThornRuntimeState state = new();

            if (config == null)
                return state;

            state.SetProgressionLimits(
                config.MaxDamageMultiplier,
                config.MaxFireRateBonus,
                config.MaxSalvoExtraShots,
                config.MaxProjectileSpeedBonus,
                config.MaxCriticalChance,
                config.CriticalDamageMultiplier,
                config.MaxCriticalDamageMultiplier);
            state.SetBaseDamage(config.Damage);

            return state;
        }

        public static WeaponPowerSnapshot EstimatePower(
            WormBalanceSimulationSettings settings,
            WeaponRuntimeState mainState,
            AcaciaThornRuntimeState acaciaState)
        {
            return WeaponPowerEstimator.Estimate(
                settings.MainWeaponConfig,
                mainState,
                settings.AcaciaThornConfig,
                acaciaState);
        }

        public static int BuildMainWeaponDamage(
            WeaponConfig config,
            WeaponRuntimeState state)
        {
            if (config == null || config.Projectile == null || state == null)
                return 0;

            return WeaponRuntimeState.ClampDamage(
                config.Projectile.Damage * (double)state.DamageMultiplier);
        }

    }

}
