
using Game.Core.Combat;
using Game.Core.Timing;
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
        private IProjectileSpawnSink<ProjectileSpawnRequest> _spawnSink;
        private Transform _firePoint;

        private float _currentShotCooldown;

        private readonly List<ShotSpawnData> _shots = new();
        private IShotPatternBuilder _shotPatternBuilder;
        private ProjectileSpawnRequestFactory _spawnRequestFactory;
        private readonly WeaponFireCycle _fireCycle = new();

        private WeaponRuntimeState _runtimeState;
        private IWeaponAttackCyclePublisher _attackCyclePublisher;
        private IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;

        public WeaponConfig Config => _config;
        public WeaponRuntimeState RuntimeState => _runtimeState;
        public int CurrentProjectileDamage => BuildProjectileDamage();

        public void Init(
            IProjectileSpawnSink<ProjectileSpawnRequest> spawnSink,
            Transform firePoint,
            IShotPatternBuilder shotPatternBuilder,
            ProjectileSpawnRequestFactory spawnRequestFactory,
            IWeaponAttackCyclePublisher attackCyclePublisher,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher)
        {
            _spawnSink = spawnSink ?? throw new ArgumentNullException(nameof(spawnSink));
            _firePoint = firePoint != null
                ? firePoint
                : throw new ArgumentNullException(nameof(firePoint));
            _shotPatternBuilder = shotPatternBuilder ??
                throw new ArgumentNullException(nameof(shotPatternBuilder));
            _spawnRequestFactory = spawnRequestFactory ??
                throw new ArgumentNullException(nameof(spawnRequestFactory));
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
            if (_spawnSink == null || !_spawnSink.IsReady || _firePoint == null || _config == null)
                return;

            WeaponFireCycleStep step = _fireCycle.Advance(
                deltaTime,
                _currentShotCooldown);

            if (step == WeaponFireCycleStep.BurstActionReady)
                FireSalvoShot();
            else if (step == WeaponFireCycleStep.PreparationReady)
                ReleasePreparedAttack(_currentShotCooldown);
            else if (step == WeaponFireCycleStep.CycleReady)
                StartAttackCycle();
        }

        private void RebuildModifiers(bool resetFiringCycle)
        {
            if (_config == null) return;

            float cappedFireRateBonus = Math.Min(
                _runtimeState.FireRateBonus,
                _config.MaxFireRateBonus);

            _currentShotCooldown = WeaponDerivedStatsCalculator.CalculateCooldown(
                _config.FireRate,
                _config.MinShotCooldown,
                cappedFireRateBonus);

            if (resetFiringCycle)
            {
                ResetFiringCycle();
                return;
            }

            _fireCycle.LimitCooldown(_currentShotCooldown);
        }

        public void ForceRebuild()
        {
            RebuildModifiers(resetFiringCycle: false);
            PublishRuntimeStatsChanged();
        }

        public void ClearTransientState()
        {
            _fireCycle.CancelTransient();
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
            _fireCycle.Reset();
        }

        private void StartAttackCycle()
        {
            _fireCycle.BeginPreparation();

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
            ReleasePreparedAttack(_fireCycle.PreparationElapsed);
        }

        public void ReleasePreparedAttack(float preparedAttackElapsed)
        {
            if (!_fireCycle.IsPreparationActive)
                return;

            if (_spawnSink == null || _firePoint == null || _config == null || _runtimeState == null)
                return;

            _fireCycle.CompletePreparation(preparedAttackElapsed, _currentShotCooldown);
            _fireCycle.BeginBurst(1 + Mathf.Max(0, _runtimeState.SalvoExtraShots));

            FireSalvoShot();
        }

        private void FireSalvoShot()
        {
            Fire();
            _fireCycle.CommitBurstAction(
                GetSalvoInterval(),
                _currentShotCooldown);
        }

        private float GetSalvoInterval()
        {
            return WeaponDerivedStatsCalculator.CalculateSalvoInterval(
                _runtimeState.SalvoInterval,
                GetProjectileSpeedMultiplier());
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
            ProjectileSpawnRequest request = _spawnRequestFactory.Create(
                _config,
                _runtimeState,
                in shot);
            _spawnSink.Spawn(in request);
        }

        private float GetProjectileSpeedMultiplier()
        {
            return WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(
                _runtimeState.ProjectileSpeedBonus);
        }

        private int BuildProjectileDamage()
        {
            if (_config == null || _config.Projectile == null || _runtimeState == null)
                return 0;

            return WeaponDerivedStatsCalculator.CalculateDamage(
                _config.Projectile.Damage,
                _runtimeState.DamageMultiplier);
        }

        private void PublishRuntimeStatsChanged()
        {
            if (_runtimeStatsPublisher == null)
                return;

            bool isActive = _config != null &&
                _config.Projectile != null &&
                _runtimeState != null;
            WeaponRuntimeStatsSnapshot snapshot = new(
                WeaponRuntimeStatsSource.MainProjectile,
                isActive,
                isActive ? BuildProjectileDamage() : 0,
                isActive ? _currentShotCooldown : 0f,
                isActive ? GetProjectileSpeedMultiplier() : 1f,
                isActive ? 1 + _runtimeState.SalvoExtraShots : 0,
                isActive ? _runtimeState.ParallelProjectileCount : 0,
                isActive ? _runtimeState.PenetrationBonus : 0,
                isActive ? _runtimeState.CriticalChance : 0f,
                isActive ? _runtimeState.CriticalDamageMultiplier : 1f);
            _runtimeStatsPublisher.Publish(in snapshot);
        }
    }

}
