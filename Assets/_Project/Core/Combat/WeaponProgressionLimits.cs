using System;

namespace Game.Core.Combat
{
    public readonly struct WeaponProgressionLimits
    {
        public WeaponProgressionLimits(
            float damageMultiplier,
            float fireRateBonus,
            int salvoExtraShots,
            float projectileSpeedBonus,
            float criticalChance,
            float criticalDamageMultiplier)
        {
            DamageMultiplier = Math.Max(1f, damageMultiplier);
            FireRateBonus = Math.Max(0f, fireRateBonus);
            SalvoExtraShots = Math.Max(0, salvoExtraShots);
            ProjectileSpeedBonus = Math.Max(0f, projectileSpeedBonus);
            CriticalChance = FloatMath.Clamp(criticalChance, 0f, 1f);
            CriticalDamageMultiplier = Math.Max(1f, criticalDamageMultiplier);
        }

        public float DamageMultiplier { get; }
        public float FireRateBonus { get; }
        public int SalvoExtraShots { get; }
        public float ProjectileSpeedBonus { get; }
        public float CriticalChance { get; }
        public float CriticalDamageMultiplier { get; }

        public WeaponProgressionLimits ClampTo(in WeaponProgressionLimits maximum)
        {
            return new WeaponProgressionLimits(
                Math.Min(DamageMultiplier, maximum.DamageMultiplier),
                Math.Min(FireRateBonus, maximum.FireRateBonus),
                Math.Min(SalvoExtraShots, maximum.SalvoExtraShots),
                Math.Min(ProjectileSpeedBonus, maximum.ProjectileSpeedBonus),
                Math.Min(CriticalChance, maximum.CriticalChance),
                Math.Min(CriticalDamageMultiplier, maximum.CriticalDamageMultiplier));
        }
    }
}
