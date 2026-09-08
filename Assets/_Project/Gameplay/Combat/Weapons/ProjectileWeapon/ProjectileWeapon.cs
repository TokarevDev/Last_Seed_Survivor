
using Game.Core.Pooling;
using Game.Core.Timing;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Pooling;
using Game.Gameplay.Signals;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [DisallowMultipleComponent]
    public sealed class ProjectileWeapon : MonoBehaviour
    {
        [Header("Debug / Safety")]
        [SerializeField][Min(1)] private int _maxShots = 200;

        private WeaponConfig _config;
        private IPooledSpawnService<ProjectileSpawnRequest> _pool;
        private Transform _firePoint;

        private float _currentShotCooldown;

        private readonly List<ShotSpawnData> _shots = new();
        private readonly ProjectileShotPatternBuilder _shotPatternBuilder = new();
        private readonly PreparedActionTimer _preparedAttack = new();
        private readonly CooldownBurstCycle _fireCycle = new();

        private WeaponRuntimeState _runtimeState;
        private IWeaponAttackCyclePublisher _attackCyclePublisher;
        private IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;

        public WeaponConfig Config => _config;
        public WeaponRuntimeState RuntimeState => _runtimeState;
        public int CurrentProjectileDamage => BuildProjectileDamage();

        public void Init(
            IPooledSpawnService<ProjectileSpawnRequest> pool,
            Transform firePoint,
            IWeaponAttackCyclePublisher attackCyclePublisher,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _firePoint = firePoint != null
                ? firePoint
                : throw new ArgumentNullException(nameof(firePoint));
            _attackCyclePublisher = attackCyclePublisher ??
                throw new ArgumentNullException(nameof(attackCyclePublisher));
            _runtimeStatsPublisher = runtimeStatsPublisher ??
                throw new ArgumentNullException(nameof(runtimeStatsPublisher));
        }

        public void ApplyConfig(WeaponConfig config)
        {
            _config = config;

            if (_runtimeState == null)
                _runtimeState = new WeaponRuntimeState();

            ApplyRuntimeLimits();

            RebuildModifiers(resetFiringCycle: true);
            PublishRuntimeStatsChanged();
        }

        public void Tick(float deltaTime)
        {
            if (_pool == null || !_pool.IsInitialized || _firePoint == null || _config == null)
                return;

            if (_fireCycle.IsBurstActive)
            {
                TickFireCycle(deltaTime);
                return;
            }

            if (_preparedAttack.IsActive)
            {
                TickPreparedAttack(deltaTime);
                return;
            }

            if (_fireCycle.Advance(deltaTime) == CooldownBurstCycleStep.CycleReady)
                StartAttackCycle();
        }

        private void RebuildModifiers(bool resetFiringCycle)
        {
            if (_config == null) return;

            float cappedFireRateBonus = Mathf.Min(
                _runtimeState.FireRateBonus,
                _config.MaxFireRateBonus);

            _currentShotCooldown = Mathf.Max(
                _config.MinShotCooldown,
                _config.FireRate / (1f + cappedFireRateBonus));

            if (resetFiringCycle)
            {
                ResetFiringCycle();
                return;
            }

            if (!_preparedAttack.IsActive)
                _fireCycle.LimitCooldown(_currentShotCooldown);
        }

        public void ForceRebuild()
        {
            RebuildModifiers(resetFiringCycle: false);
            PublishRuntimeStatsChanged();
        }

        public void ClearTransientState()
        {
            _preparedAttack.Reset();
            _fireCycle.CancelBurst();
        }

        public void ResetRuntimeState()
        {
            if (_runtimeState == null)
                _runtimeState = new WeaponRuntimeState();
            else
                _runtimeState.ResetProgression();

            if (_config != null)
            {
                ApplyRuntimeLimits();
                RebuildModifiers(resetFiringCycle: true);
            }
            else
            {
                ClearTransientState();
                _fireCycle.Reset();
            }

            PublishRuntimeStatsChanged();
        }

        private void ApplyRuntimeLimits()
        {
            if (_config == null || _runtimeState == null)
                return;

            _runtimeState.SetProgressionLimits(
                _config.MaxDamageMultiplier,
                _config.MaxFireRateBonus,
                _config.MaxProjectileSpeedBonus,
                _config.MaxCriticalChance,
                _config.MaxCriticalDamageMultiplier,
                _config.MaxPenetrationBonus,
                _config.MaxParallelProjectiles,
                _config.MaxSalvoExtraShots);
        }

        private void ResetFiringCycle()
        {
            _preparedAttack.Reset();
            _fireCycle.Reset();
        }

        private void StartAttackCycle()
        {
            _preparedAttack.Begin();

            if (_attackCyclePublisher == null)
            {
                ReleasePreparedAttack();
                return;
            }

            _attackCyclePublisher.Publish(
                _currentShotCooldown,
                GetBaseShotCooldown());
        }

        private float GetBaseShotCooldown()
        {
            if (_config == null)
                return _currentShotCooldown;

            return Mathf.Max(_config.MinShotCooldown, _config.FireRate);
        }

        public void ReleasePreparedAttack()
        {
            ReleasePreparedAttack(_preparedAttack.Elapsed);
        }

        public void ReleasePreparedAttack(float preparedAttackElapsed)
        {
            if (!_preparedAttack.IsActive)
                return;

            if (_pool == null || _firePoint == null || _config == null || _runtimeState == null)
                return;

            _preparedAttack.TryComplete(preparedAttackElapsed, _currentShotCooldown);
            _fireCycle.BeginBurst(1 + Mathf.Max(0, _runtimeState.SalvoExtraShots));

            FireSalvoShot();
        }

        private void TickPreparedAttack(float deltaTime)
        {
            _preparedAttack.Advance(deltaTime);

            if (_preparedAttack.HasReached(_currentShotCooldown))
                ReleasePreparedAttack(_currentShotCooldown);
        }

        private void TickFireCycle(float deltaTime)
        {
            if (_fireCycle.Advance(deltaTime) == CooldownBurstCycleStep.BurstActionReady)
                FireSalvoShot();
        }

        private void FireSalvoShot()
        {
            Fire();
            _fireCycle.CommitBurstAction(
                GetSalvoInterval(),
                _currentShotCooldown - _preparedAttack.LastCompletionDelay);
        }

        private float GetSalvoInterval()
        {
            return Mathf.Max(
                0.01f,
                _runtimeState.SalvoInterval / GetProjectileSpeedMultiplier());
        }

        private void Fire()
        {
            _shots.Clear();
            _shotPatternBuilder.Build(_firePoint.position, _firePoint.rotation, _runtimeState, _shots);

            if (_shots.Count > _maxShots)
            {
                Debug.LogWarning($"Shot limit exceeded: {_shots.Count} → clamped to {_maxShots}");
                _shots.RemoveRange(_maxShots, _shots.Count - _maxShots);
            }

            foreach (var shot in _shots)
            {
                Spawn(shot);
            }
        }

        private void Spawn(ShotSpawnData shot)
        {
            ProjectileRuntimeStats stats = BuildProjectileStats();
            ProjectileSpawnRequest request = new(
                _config.Projectile,
                stats,
                shot.Position,
                shot.Rotation);
            _pool.Spawn(in request);
        }

        private ProjectileRuntimeStats BuildProjectileStats()
        {
            int finalDamage = BuildProjectileDamage();

            return new ProjectileRuntimeStats(
                finalDamage,
                _runtimeState.PenetrationBonus,
                _runtimeState.CriticalChance,
                _runtimeState.CriticalDamageMultiplier,
                GetProjectileSpeedMultiplier()
            );
        }

        private float GetProjectileSpeedMultiplier()
        {
            return Mathf.Max(0.1f, 1f + _runtimeState.ProjectileSpeedBonus);
        }

        private int BuildProjectileDamage()
        {
            if (_config == null || _config.Projectile == null || _runtimeState == null)
                return 0;

            return WeaponRuntimeState.ClampDamage(
                _config.Projectile.Damage * (double)_runtimeState.DamageMultiplier);
        }

        private void PublishRuntimeStatsChanged()
        {
            _runtimeStatsPublisher?.Publish(WeaponRuntimeStatsSource.MainProjectile);
        }
    }

}
