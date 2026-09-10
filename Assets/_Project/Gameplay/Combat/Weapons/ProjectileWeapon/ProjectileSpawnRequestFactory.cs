using System;
using Game.Core.Combat;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Pooling;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon
{
    public sealed class ProjectileSpawnRequestFactory
    {
        public ProjectileSpawnRequest Create(
            WeaponConfig config,
            WeaponRuntimeState runtimeState,
            in ShotSpawnData shot)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.Projectile == null)
                throw new InvalidOperationException("Projectile config is missing.");

            if (runtimeState == null)
                throw new ArgumentNullException(nameof(runtimeState));

            ProjectileRuntimeStats stats = new(
                WeaponDerivedStatsCalculator.CalculateDamage(
                    config.Projectile.Damage,
                    runtimeState.DamageMultiplier),
                runtimeState.PenetrationBonus,
                runtimeState.CriticalChance,
                runtimeState.CriticalDamageMultiplier,
                WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(
                    runtimeState.ProjectileSpeedBonus));

            return new ProjectileSpawnRequest(
                config.Projectile,
                stats,
                shot.Position,
                shot.Rotation);
        }
    }
}
