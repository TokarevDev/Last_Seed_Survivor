
using Game.Core.Combat;
using Game.Core.Timing;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Signals;

namespace Game.Gameplay.Combat.Weapons.AcaciaThornWeapon
{
    using System;
    using UnityEngine;

    [DisallowMultipleComponent]
    public sealed class AcaciaThornWeapon : MonoBehaviour
    {
        [SerializeField] private AcaciaThornWeaponConfig _config;

        private readonly AcaciaThornRuntimeState _runtimeState = new();

        private Transform _firePoint;
        private float _currentCooldown;
        private bool _initialized;
        private IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;
        private AcaciaThornProjectileSpawnRequestFactory _spawnRequestFactory;
        private IProjectileSpawnSink<AcaciaThornProjectileSpawnRequest> _spawnSink;
        private readonly WeaponFireCycle _fireCycle = new();

        public AcaciaThornWeaponConfig Config => _config;
        public AcaciaThornRuntimeState RuntimeState => _runtimeState;

        public void Init(
            Transform firePoint,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher,
            AcaciaThornProjectileSpawnRequestFactory spawnRequestFactory,
            IProjectileSpawnSink<AcaciaThornProjectileSpawnRequest> spawnSink)
        {
            if (_initialized)
                return;

            if (_config == null)
                throw new InvalidOperationException("Acacia Thorn weapon config is missing.");

            if (spawnSink == null)
                throw new ArgumentNullException(nameof(spawnSink));

            if (!spawnSink.IsReady)
                throw new InvalidOperationException("Acacia Thorn projectile pool is not initialized.");

            if (firePoint == null)
                throw new ArgumentNullException(nameof(firePoint));

            _runtimeStatsPublisher = runtimeStatsPublisher ??
                throw new ArgumentNullException(nameof(runtimeStatsPublisher));
            _spawnRequestFactory = spawnRequestFactory ??
                throw new ArgumentNullException(nameof(spawnRequestFactory));
            _spawnSink = spawnSink;
            _firePoint = firePoint;
            ApplyRuntimeLimits();
            _runtimeState.SetBaseDamage(_config.Damage);

            RebuildCooldown(resetTimer: true);
            _initialized = true;
            PublishRuntimeStatsChanged();
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized || !_runtimeState.IsUnlocked || !_spawnSink.IsReady)
                return;

            WeaponFireCycleStep step = _fireCycle.Advance(deltaTime);

            if (step == WeaponFireCycleStep.BurstActionReady)
            {
                FireSalvoShot();
                return;
            }

            if (step == WeaponFireCycleStep.CycleReady)
                StartSalvo();
        }

        public void Unlock(int baseDamage)
        {
            if (!_runtimeState.CanUnlock)
                return;

            int fallbackBaseDamage = _config != null ? _config.Damage : 1;
            _runtimeState.Unlock(Mathf.Max(fallbackBaseDamage, baseDamage));
            _fireCycle.Reset();
            PublishRuntimeStatsChanged();
        }

        public void AddDamageMultiplier(float multiplier)
        {
            if (!_runtimeState.CanApplyDamageMultiplier(multiplier))
                return;

            _runtimeState.ApplyDamageMultiplier(multiplier);
            PublishRuntimeStatsChanged();
        }

        public void AddFireRateBonus(float bonus)
        {
            if (!_runtimeState.CanApplyFireRateBonus(bonus))
                return;

            _runtimeState.AddFireRateBonus(bonus);
            RebuildCooldown(resetTimer: false);
            PublishRuntimeStatsChanged();
        }

        public void AddSalvoShots(int extraShots)
        {
            if (!_runtimeState.CanApplySalvoShots(extraShots))
                return;

            _runtimeState.AddSalvoShots(extraShots);
            PublishRuntimeStatsChanged();
        }

        public void AddProjectileSpeedBonus(float bonus)
        {
            if (!_runtimeState.CanApplyProjectileSpeedBonus(bonus))
                return;

            _runtimeState.AddProjectileSpeedBonus(bonus);
            PublishRuntimeStatsChanged();
        }

        public void AddCriticalChance(float chanceBonus)
        {
            if (!_runtimeState.CanApplyCriticalChance(chanceBonus))
                return;

            _runtimeState.AddCriticalChance(chanceBonus);
            PublishRuntimeStatsChanged();
        }

        public void AddCriticalDamageBonus(float damageBonus)
        {
            if (!_runtimeState.CanApplyCriticalDamageBonus(damageBonus))
                return;

            _runtimeState.AddCriticalDamageBonus(damageBonus);
            PublishRuntimeStatsChanged();
        }

        public void ClearTransientState()
        {
            _spawnSink?.Clear();
            _fireCycle.CancelTransient();
        }

        public void ResetRuntimeState()
        {
            ClearTransientState();

            int baseDamage = _config != null ? _config.Damage : 1;
            _runtimeState.ResetProgression(baseDamage);
            ApplyRuntimeLimits();
            RebuildCooldown(resetTimer: true);
            PublishRuntimeStatsChanged();
        }

        private void ApplyRuntimeLimits()
        {
            if (_config == null)
                return;

            _runtimeState.SetProgressionLimits(
                _config.MaxDamageMultiplier,
                _config.MaxFireRateBonus,
                _config.MaxSalvoExtraShots,
                _config.MaxProjectileSpeedBonus,
                _config.MaxCriticalChance,
                _config.CriticalDamageMultiplier,
                _config.MaxCriticalDamageMultiplier);
        }

        private void StartSalvo()
        {
            _fireCycle.BeginBurst(1 + Mathf.Max(0, _runtimeState.SalvoExtraShots));
            FireSalvoShot();
        }

        private void FireSalvoShot()
        {
            Fire();
            _fireCycle.CommitBurstAction(GetSalvoInterval(), _currentCooldown);
        }

        private void Fire()
        {
            Vector2 direction = _firePoint.rotation * Vector2.up;

            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector2.up;

            direction.Normalize();

            AcaciaThornProjectileSpawnRequest request = _spawnRequestFactory.Create(
                _config,
                _runtimeState,
                _firePoint.position,
                direction);
            _spawnSink.Spawn(in request);
        }

        private float GetSalvoInterval()
        {
            return WeaponDerivedStatsCalculator.CalculateSalvoInterval(
                _config.SalvoInterval,
                GetProjectileSpeedMultiplier());
        }

        private float GetProjectileSpeedMultiplier()
        {
            return WeaponDerivedStatsCalculator.CalculateProjectileSpeedMultiplier(
                _runtimeState.ProjectileSpeedBonus);
        }

        private void RebuildCooldown(bool resetTimer)
        {
            float cappedFireRateBonus = Math.Min(
                _runtimeState.FireRateBonus,
                _config.MaxFireRateBonus);

            _currentCooldown = WeaponDerivedStatsCalculator.CalculateCooldown(
                _config.Cooldown,
                _config.MinCooldown,
                cappedFireRateBonus);

            if (resetTimer)
                _fireCycle.Reset();
            else
                _fireCycle.LimitCooldown(_currentCooldown);
        }

        private void PublishRuntimeStatsChanged()
        {
            _runtimeStatsPublisher?.Publish(WeaponRuntimeStatsSource.AcaciaThorn);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Unlock Acacia Thorn")]
        private void DebugUnlockAcaciaThorn()
        {
            Unlock(_config != null ? _config.Damage : 1);
        }

        [ContextMenu("Debug/Fire Acacia Thorn Once")]
        private void DebugFireAcaciaThornOnce()
        {
            if (!_initialized)
            {
                Debug.LogWarning("AcaciaThornWeapon debug fire skipped: weapon is not initialized.", this);
                return;
            }

            if (!_runtimeState.IsUnlocked)
                Unlock(_config != null ? _config.Damage : 1);

            Fire();
            _fireCycle.StartCooldown(_currentCooldown);
        }
#endif
    }

}
