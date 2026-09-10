using System;
using Game.Core.Randomization;

namespace Game.Core.Combat
{
    public readonly struct CriticalDamageRoll
    {
        public CriticalDamageRoll(int damage, bool isCritical)
        {
            Damage = damage;
            IsCritical = isCritical;
        }

        public int Damage { get; }
        public bool IsCritical { get; }
        public DamageKind DamageKind => IsCritical
            ? DamageKind.Critical
            : DamageKind.Normal;
    }

    public static class CriticalDamageResolver
    {
        public static CriticalDamageRoll Roll(
            double rawDamage,
            float criticalChance,
            float criticalDamageMultiplier,
            IRandomSource randomSource)
        {
            if (randomSource == null)
                throw new ArgumentNullException(nameof(randomSource));

            bool isCritical = criticalChance > 0f &&
                randomSource.NextUnitFloat() < criticalChance;
            double resolvedDamage = isCritical
                ? rawDamage * criticalDamageMultiplier
                : rawDamage;

            return new CriticalDamageRoll(
                WeaponDamageClamp.Clamp(resolvedDamage),
                isCritical);
        }
    }

}
