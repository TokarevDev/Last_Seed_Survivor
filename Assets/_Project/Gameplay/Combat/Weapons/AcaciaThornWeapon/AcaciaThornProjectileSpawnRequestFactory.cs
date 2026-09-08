using Game.Core.Combat;
using Game.Core.Random;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;

namespace Game.Gameplay.Combat.Weapons.AcaciaThornWeapon
{
    using System;
    using UnityEngine;

    public sealed class AcaciaThornProjectileSpawnRequestFactory
    {
        private readonly IRandomSource _randomSource;

        public AcaciaThornProjectileSpawnRequestFactory(IRandomSource randomSource)
        {
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        public AcaciaThornProjectileSpawnRequest Create(
            AcaciaThornWeaponConfig config,
            AcaciaThornRuntimeState runtimeState,
            Vector3 origin,
            Vector2 direction)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (runtimeState == null)
                throw new ArgumentNullException(nameof(runtimeState));

            Vector3 position = origin +
                (Vector3)(direction * Mathf.Max(0f, config.SpawnOffset));
            double rawDamage = WeaponDerivedStatsCalculator.CalculateRawDamage(
                runtimeState.BaseDamage,
                runtimeState.DamageMultiplier);
            CriticalDamageRoll damageRoll = CriticalDamageResolver.Roll(
                rawDamage,
                runtimeState.CriticalChance,
                runtimeState.CriticalDamageMultiplier,
                _randomSource);
            float speedMultiplier =
                WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(
                    runtimeState.ProjectileSpeedBonus);

            return new AcaciaThornProjectileSpawnRequest(
                position,
                direction,
                damageRoll.Damage,
                damageRoll.DamageKind,
                damageRoll.IsCritical,
                Mathf.Max(
                    WeaponDerivedStatsCalculator.MinimumProjectileSpeedMultiplier,
                    config.Speed * speedMultiplier),
                config.LifeTime,
                config.BounceCount,
                Mathf.Max(0, config.BaseSplitCount),
                true);
        }
    }
}
