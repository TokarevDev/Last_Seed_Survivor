using System;
using Game.Core.Combat;

namespace Game.Gameplay.Combat.Weapons.AcaciaThornWeapon
{
    public sealed class AcaciaThornRuntimeState
    {
        public const float DefaultMaxFireRateBonus = 3f;
        public const float DefaultMaxProjectileSpeedBonus = 2f;
        public const float MaxDamageMultiplier = 100000f;
        public const int DefaultMaxSalvoShots = 4;
        public const int MaxSalvoShots = 6;
        public const int DefaultMaxSalvoExtraShots = DefaultMaxSalvoShots - 1;
        public const int MaxSalvoExtraShots = MaxSalvoShots - 1;
        public const float MaxCriticalChance = 1f;
        public const float MaxCriticalDamageMultiplier = 100f;

        private static readonly WeaponProgressionLimits DefaultProgressionLimits = new(
            MaxDamageMultiplier,
            DefaultMaxFireRateBonus,
            DefaultMaxSalvoExtraShots,
            DefaultMaxProjectileSpeedBonus,
            MaxCriticalChance,
            MaxCriticalDamageMultiplier);

        private static readonly WeaponProgressionLimits HardProgressionLimits = new(
            MaxDamageMultiplier,
            DefaultMaxFireRateBonus,
            MaxSalvoExtraShots,
            DefaultMaxProjectileSpeedBonus,
            MaxCriticalChance,
            MaxCriticalDamageMultiplier);

        private WeaponProgressionState _progression = new(
            DefaultProgressionLimits,
            HardProgressionLimits);

        public bool IsUnlocked { get; private set; }
        public int BaseDamage { get; private set; } = 1;
        public float DamageMultiplier => _progression.DamageMultiplier;
        public float FireRateBonus => _progression.FireRateBonus;
        public int SalvoExtraShots => _progression.SalvoExtraShots;
        public float ProjectileSpeedBonus => _progression.ProjectileSpeedBonus;
        public float CriticalChance => _progression.CriticalChance;
        public float CriticalDamageMultiplier => _progression.CriticalDamageMultiplier;
        public float MaxFireRateBonus => _progression.MaxFireRateBonus;
        public float MaxProjectileSpeedBonus => _progression.MaxProjectileSpeedBonus;

        public bool CanUnlock => !IsUnlocked;

        public void ResetProgression(int baseDamage)
        {
            IsUnlocked = false;
            BaseDamage = Math.Max(1, baseDamage);
            _progression.Reset();
        }

        public void SetProgressionLimits(
            float maxDamageMultiplier,
            float maxFireRateBonus,
            int maxSalvoExtraShots,
            float maxProjectileSpeedBonus,
            float maxCriticalChance,
            float criticalDamageMultiplier,
            float maxCriticalDamageMultiplier)
        {
            _progression.SetLimits(new WeaponProgressionLimits(
                maxDamageMultiplier,
                maxFireRateBonus,
                maxSalvoExtraShots,
                maxProjectileSpeedBonus,
                maxCriticalChance,
                maxCriticalDamageMultiplier));
            _progression.SetCriticalDamageMultiplier(criticalDamageMultiplier);
        }

        public bool CanApplyDamageMultiplier(float multiplier) =>
            IsUnlocked && _progression.CanApplyDamageMultiplier(multiplier);

        public bool CanApplyFireRateBonus(float bonus) =>
            IsUnlocked && _progression.CanApplyFireRateBonus(bonus);

        public bool CanApplySalvoShots(int extraShots) =>
            IsUnlocked && _progression.CanApplySalvoShots(extraShots);

        public bool CanApplySalvoShots(
            int extraShots,
            int maxSalvoExtraShotsAfterApply)
        {
            return IsUnlocked && _progression.CanApplySalvoShots(
                extraShots,
                maxSalvoExtraShotsAfterApply);
        }

        public bool CanApplyProjectileSpeedBonus(float bonus) =>
            IsUnlocked && _progression.CanApplyProjectileSpeedBonus(bonus);

        public bool CanApplyCriticalChance(float chanceBonus) =>
            IsUnlocked && _progression.CanApplyCriticalChance(chanceBonus);

        public bool CanApplyCriticalDamageBonus(float damageBonus)
        {
            return IsUnlocked
                && CriticalChance > 0f
                && _progression.CanApplyCriticalDamageBonus(damageBonus);
        }

        public void Unlock(int baseDamage)
        {
            SetBaseDamage(baseDamage);
            IsUnlocked = true;
        }

        public void SetBaseDamage(int baseDamage)
        {
            BaseDamage = Math.Max(BaseDamage, Math.Max(1, baseDamage));
        }

        public float ApplyDamageMultiplier(float multiplier) =>
            _progression.ApplyDamageMultiplier(multiplier);

        public float AddFireRateBonus(float bonus) =>
            _progression.AddFireRateBonus(bonus);

        public int AddSalvoShots(int extraShots) =>
            _progression.AddSalvoShots(extraShots);

        public void ExpandSalvoExtraShotLimit(int maxSalvoExtraShots)
        {
            _progression.ExpandSalvoExtraShotLimit(maxSalvoExtraShots);
        }

        public float AddProjectileSpeedBonus(float bonus) =>
            _progression.AddProjectileSpeedBonus(bonus);

        public float AddCriticalChance(float chanceBonus) =>
            _progression.AddCriticalChance(chanceBonus);

        public float AddCriticalDamageBonus(float damageBonus) =>
            _progression.AddCriticalDamageBonus(damageBonus);

        public static int ClampDamage(double rawDamage) =>
            WeaponDamageClamp.Clamp(rawDamage);

        public AcaciaThornRuntimeState Clone()
        {
            return new AcaciaThornRuntimeState
            {
                _progression = _progression.Clone(),
                IsUnlocked = IsUnlocked,
                BaseDamage = BaseDamage
            };
        }
    }
}
