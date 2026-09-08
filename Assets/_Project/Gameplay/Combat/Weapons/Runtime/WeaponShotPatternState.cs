
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Modifiers;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Modifiers.Parallel;

using Game.Gameplay.Combat.Weapons.ProjectileWeapon;

namespace Game.Gameplay.Combat.Weapons.Runtime
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class WeaponShotPatternState
    {
        private const int DefaultMaxParallelProjectiles = 5;
        public const int MaxParallelProjectiles = 8;

        private readonly List<ShotModifierData> _modifiers = new();
        private int _maxParallelProjectiles = DefaultMaxParallelProjectiles;

        public int ParallelProjectileCount { get; private set; } = 1;
        public float ParallelSpacing { get; private set; } = 0.5f;
        public IReadOnlyList<ShotModifierData> Modifiers => _modifiers;

        public bool CanAddParallelProjectiles => ParallelProjectileCount < _maxParallelProjectiles;

        public void Reset()
        {
            _modifiers.Clear();
            ParallelProjectileCount = 1;
            ParallelSpacing = 0.5f;
        }

        public void SetParallelLimit(int maxParallelProjectiles)
        {
            _maxParallelProjectiles = Mathf.Clamp(maxParallelProjectiles, 1, MaxParallelProjectiles);
            ParallelProjectileCount = Mathf.Min(ParallelProjectileCount, _maxParallelProjectiles);
        }

        public bool CanApplyParallelProjectiles(int bonus, int limitAfterApply)
        {
            if (bonus <= 0)
                return false;

            int limit = Mathf.Clamp(
                Mathf.Max(_maxParallelProjectiles, limitAfterApply),
                1,
                MaxParallelProjectiles);
            return ParallelProjectileCount + bonus <= limit;
        }

        public bool CanApplyParallelProjectiles(int bonus)
        {
            return bonus > 0 && ParallelProjectileCount + bonus <= _maxParallelProjectiles;
        }

        public int AddParallelProjectiles(int bonus, float spacing)
        {
            int accepted = Mathf.Min(Mathf.Max(0, bonus), _maxParallelProjectiles - ParallelProjectileCount);
            ParallelProjectileCount += Mathf.Max(0, accepted);

            if (accepted > 0)
                ParallelSpacing = Mathf.Max(0.1f, spacing);

            return accepted;
        }

        public void ExpandParallelLimit(int limit)
        {
            _maxParallelProjectiles = Mathf.Clamp(
                Mathf.Max(_maxParallelProjectiles, limit),
                1,
                MaxParallelProjectiles);
        }

        public bool AddModifier(ShotModifierData modifier)
        {
            if (modifier == null)
                return false;

            if (modifier is ParallelModifierData parallel)
                return AddParallelProjectiles(Mathf.Max(0, parallel.Count - 1), parallel.Spacing) > 0;

            _modifiers.Add(modifier);
            return true;
        }

        public bool CanAddModifier(ShotModifierData modifier)
        {
            if (modifier == null)
                return false;

            return modifier is not ParallelModifierData parallel
                || CanApplyParallelProjectiles(Mathf.Max(0, parallel.Count - 1));
        }

        public WeaponShotPatternState Clone()
        {
            WeaponShotPatternState clone = new()
            {
                _maxParallelProjectiles = _maxParallelProjectiles,
                ParallelProjectileCount = ParallelProjectileCount,
                ParallelSpacing = ParallelSpacing
            };

            for (int i = 0; i < _modifiers.Count; i++)
                clone._modifiers.Add(_modifiers[i]);

            return clone;
        }
    }

}
