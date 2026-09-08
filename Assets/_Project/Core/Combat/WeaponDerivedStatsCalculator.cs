namespace Game.Core.Combat
{
    using System;

    public static class WeaponDerivedStatsCalculator
    {
        public const float MinimumProjectileSpeedMultiplier = 0.1f;
        public const float MinimumSalvoInterval = 0.01f;

        public static double CalculateRawDamage(int baseDamage, float damageMultiplier)
        {
            return Math.Max(1, baseDamage) *
                (double)Math.Max(1f, damageMultiplier);
        }

        public static int CalculateDamage(int baseDamage, float damageMultiplier)
        {
            return WeaponDamageClamp.Clamp(
                CalculateRawDamage(baseDamage, damageMultiplier));
        }

        public static float CalculateCooldown(
            float baseCooldown,
            float minimumCooldown,
            float fireRateBonus)
        {
            float normalizedBaseCooldown = Math.Max(0f, baseCooldown);
            float normalizedMinimumCooldown = Math.Max(0f, minimumCooldown);
            float normalizedBonus = Math.Max(0f, fireRateBonus);

            return Math.Max(
                normalizedMinimumCooldown,
                normalizedBaseCooldown / (1f + normalizedBonus));
        }

        public static float CalculateProjectileSpeedMultiplier(float speedBonus)
        {
            return Math.Max(
                MinimumProjectileSpeedMultiplier,
                1f + speedBonus);
        }

        public static float CalculateSalvoInterval(
            float baseInterval,
            float projectileSpeedMultiplier)
        {
            float normalizedSpeedMultiplier = Math.Max(
                MinimumProjectileSpeedMultiplier,
                projectileSpeedMultiplier);

            return Math.Max(
                MinimumSalvoInterval,
                Math.Max(0f, baseInterval) / normalizedSpeedMultiplier);
        }
    }
}
