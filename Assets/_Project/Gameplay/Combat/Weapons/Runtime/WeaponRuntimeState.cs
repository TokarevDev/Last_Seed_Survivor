using System.Collections.Generic;
using Game.Core.Combat;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Modifiers;
using UnityEngine;

namespace Game.Gameplay.Combat.Weapons.Runtime
{
    public sealed class WeaponRuntimeState
    {
        private const float DefaultSalvoInterval = 0.2f;

        public const int DefaultMaxParallelProjectiles = 5;
        public const int MaxParallelProjectiles = 8;
        public const int DefaultMaxSalvoShots = 4;
        public const int MaxSalvoShots = 6;
        public const int DefaultMaxSalvoExtraShots = DefaultMaxSalvoShots - 1;
        public const int MaxSalvoExtraShots = MaxSalvoShots - 1;
        public const int MaxProjectileDamage = WeaponDamageClamp.MaximumDamage;
        public const float DefaultMaxFireRateBonus = 3f;
        public const float DefaultMaxProjectileSpeedBonus = 2f;
        public const float MaxDamageMultiplier = 100000f;
        public const float MaxCriticalDamageMultiplier = 100f;
        public const int MaxPenetrationBonus = 5;
        public const float MaxCriticalChance = 1f;

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

        private WeaponShotPatternState _shotPattern = new();
        private WeaponProgressionState _progression = new(
            DefaultProgressionLimits,
            HardProgressionLimits);
        private int _maxPenetrationBonus = MaxPenetrationBonus;
        private float _salvoInterval = DefaultSalvoInterval;

        public float DamageMultiplier => _progression.DamageMultiplier;
        public float FireRateBonus => _progression.FireRateBonus;
        public float CriticalChance => _progression.CriticalChance;
        public float CriticalDamageMultiplier => _progression.CriticalDamageMultiplier;
        public int PenetrationBonus { get; private set; }
        public int ParallelProjectileCount => _shotPattern.ParallelProjectileCount;
        public float ParallelSpacing => _shotPattern.ParallelSpacing;
        public int SalvoExtraShots => _progression.SalvoExtraShots;
        public float SalvoInterval => _salvoInterval;
        public float ProjectileSpeedBonus => _progression.ProjectileSpeedBonus;
        public float MaxFireRateBonus => _progression.MaxFireRateBonus;
        public float MaxProjectileSpeedBonus => _progression.MaxProjectileSpeedBonus;
        public IReadOnlyList<ShotModifierData> ShotModifiers => _shotPattern.Modifiers;

        public bool CanAddDamageMultiplier => _progression.CanAddDamageMultiplier;
        public bool CanAddFireRateBonus => _progression.CanAddFireRateBonus;
        public bool CanAddCriticalChance => _progression.CanAddCriticalChance;
        public bool CanAddCriticalDamage => _progression.CanAddCriticalDamage;
        public bool CanAddPenetration => PenetrationBonus < _maxPenetrationBonus;
        public bool CanAddParallelProjectiles => _shotPattern.CanAddParallelProjectiles;
        public bool CanAddSalvoShots => _progression.CanAddSalvoShots;
        public bool CanAddProjectileSpeedBonus => _progression.CanAddProjectileSpeedBonus;

        public void ResetProgression()
        {
            _shotPattern.Reset();
            _progression.Reset();
            PenetrationBonus = 0;
            _salvoInterval = DefaultSalvoInterval;
        }

        public bool CanApplyDamageMultiplier(float multiplier) =>
            _progression.CanApplyDamageMultiplier(multiplier);

        public bool CanApplyFireRateBonus(float bonus) =>
            _progression.CanApplyFireRateBonus(bonus);

        public bool CanApplyProjectileSpeedBonus(float bonus) =>
            _progression.CanApplyProjectileSpeedBonus(bonus);

        public bool CanApplyCriticalChance(float chanceBonus) =>
            _progression.CanApplyCriticalChance(chanceBonus);

        public bool CanApplyCriticalDamageBonus(float damageBonus) =>
            _progression.CanApplyCriticalDamageBonus(damageBonus);

        public bool CanApplyPenetrationBonus(int bonus)
        {
            return bonus > 0 && PenetrationBonus + bonus <= _maxPenetrationBonus;
        }

        public bool CanApplyParallelProjectiles(int bonusProjectiles) =>
            _shotPattern.CanApplyParallelProjectiles(bonusProjectiles);

        public bool CanApplySalvoShots(int extraShots) =>
            _progression.CanApplySalvoShots(extraShots);

        public bool CanApplyParallelProjectiles(
            int bonusProjectiles,
            int maxParallelProjectilesAfterApply)
        {
            return _shotPattern.CanApplyParallelProjectiles(
                bonusProjectiles,
                maxParallelProjectilesAfterApply);
        }

        public bool CanApplySalvoShots(
            int extraShots,
            int maxSalvoExtraShotsAfterApply)
        {
            return _progression.CanApplySalvoShots(
                extraShots,
                maxSalvoExtraShotsAfterApply);
        }

        public float ApplyDamageMultiplier(float multiplier) =>
            _progression.ApplyDamageMultiplier(multiplier);

        public void SetProgressionLimits(
            float maxDamageMultiplier,
            float maxFireRateBonus,
            float maxProjectileSpeedBonus,
            float maxCriticalChance,
            float maxCriticalDamageMultiplier,
            int maxPenetrationBonus,
            int maxParallelProjectiles,
            int maxSalvoExtraShots)
        {
            _progression.SetLimits(new WeaponProgressionLimits(
                maxDamageMultiplier,
                maxFireRateBonus,
                maxSalvoExtraShots,
                maxProjectileSpeedBonus,
                maxCriticalChance,
                maxCriticalDamageMultiplier));

            _maxPenetrationBonus = Mathf.Clamp(
                maxPenetrationBonus,
                0,
                MaxPenetrationBonus);
            _shotPattern.SetParallelLimit(maxParallelProjectiles);
            PenetrationBonus = Mathf.Min(PenetrationBonus, _maxPenetrationBonus);
        }

        public float AddFireRateBonus(float bonus) =>
            _progression.AddFireRateBonus(bonus);

        public float AddProjectileSpeedBonus(float bonus) =>
            _progression.AddProjectileSpeedBonus(bonus);

        public float AddCriticalChance(
            float chanceBonus,
            float minimumCriticalDamageMultiplier)
        {
            return _progression.AddCriticalChance(
                chanceBonus,
                minimumCriticalDamageMultiplier);
        }

        public float AddCriticalDamageBonus(float damageBonus) =>
            _progression.AddCriticalDamageBonus(damageBonus);

        public int AddPenetration(int bonus)
        {
            int accepted = Mathf.Min(
                Mathf.Max(0, bonus),
                _maxPenetrationBonus - PenetrationBonus);

            PenetrationBonus += Mathf.Max(0, accepted);
            return accepted;
        }

        public int AddSalvoShots(int extraShots, float interval)
        {
            int accepted = _progression.AddSalvoShots(extraShots);

            if (accepted > 0)
                _salvoInterval = Mathf.Max(0.01f, interval);

            return accepted;
        }

        public void ExpandParallelProjectileLimit(int maxParallelProjectiles)
        {
            _shotPattern.ExpandParallelLimit(maxParallelProjectiles);
        }

        public void ExpandSalvoExtraShotLimit(int maxSalvoExtraShots)
        {
            _progression.ExpandSalvoExtraShotLimit(maxSalvoExtraShots);
        }

        public bool AddShotModifier(ShotModifierData modifier) =>
            _shotPattern.AddModifier(modifier);

        public bool CanAddShotModifier(ShotModifierData modifier) =>
            _shotPattern.CanAddModifier(modifier);

        public int AddParallelProjectiles(int bonusProjectiles, float spacing) =>
            _shotPattern.AddParallelProjectiles(bonusProjectiles, spacing);

        public static int ClampDamage(double rawDamage) =>
            WeaponDamageClamp.Clamp(rawDamage);

        public WeaponRuntimeState Clone()
        {
            return new WeaponRuntimeState
            {
                _shotPattern = _shotPattern.Clone(),
                _progression = _progression.Clone(),
                _maxPenetrationBonus = _maxPenetrationBonus,
                _salvoInterval = _salvoInterval,
                PenetrationBonus = PenetrationBonus
            };
        }
    }
}
