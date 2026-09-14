using System;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public static class WeaponPowerAggregator
    {
        public static WeaponPowerSnapshot Combine(
            WeaponPowerSnapshot current,
            WeaponPowerSnapshot next)
        {
            if (!current.IsValid)
                return next;

            if (!next.IsValid)
                return current;

            return new WeaponPowerSnapshot(
                true,
                current.EstimatedDps + next.EstimatedDps,
                Math.Max(current.DamagePerProjectile, next.DamagePerProjectile),
                current.ProjectilesPerShot + next.ProjectilesPerShot,
                Math.Max(current.SalvoShots, next.SalvoShots),
                Math.Min(current.ShotCycleTime, next.ShotCycleTime));
        }
    }
}
