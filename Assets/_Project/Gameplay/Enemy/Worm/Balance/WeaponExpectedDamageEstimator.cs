using Game.Core.Combat;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public static class WeaponExpectedDamageEstimator
    {
        public static int Estimate(
            int baseDamage,
            float damageMultiplier,
            float criticalChance,
            float criticalDamageMultiplier)
        {
            int damage = WeaponDerivedStatsCalculator.CalculateDamage(
                baseDamage,
                damageMultiplier);
            float normalizedCriticalChance = Mathf.Clamp01(criticalChance);
            float criticalBonus = Mathf.Max(0f, criticalDamageMultiplier - 1f);
            float expectedCriticalMultiplier =
                1f + normalizedCriticalChance * criticalBonus;

            return WeaponDamageClamp.Clamp(
                damage * (double)expectedCriticalMultiplier);
        }
    }
}
