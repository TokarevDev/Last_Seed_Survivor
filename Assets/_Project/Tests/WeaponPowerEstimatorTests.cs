using System.Collections.Generic;
using System.Reflection;
using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Enemy.Worm.Balance;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class WeaponPowerEstimatorTests
    {
        private ProjectileConfig _projectileConfig;
        private WeaponConfig _mainConfig;
        private AcaciaThornWeaponConfig _acaciaConfig;

        [SetUp]
        public void SetUp()
        {
            _projectileConfig = ScriptableObject.CreateInstance<ProjectileConfig>();
            SetPrivateField(_projectileConfig, "_damage", 10);
            SetPrivateField(_projectileConfig, "_penetration", 2);

            _mainConfig = ScriptableObject.CreateInstance<WeaponConfig>();
            _mainConfig.FireRate = 2f;
            _mainConfig.MinShotCooldown = 0.5f;
            _mainConfig.Projectile = _projectileConfig;

            _acaciaConfig = ScriptableObject.CreateInstance<AcaciaThornWeaponConfig>();
            _acaciaConfig.Damage = 6;
            _acaciaConfig.Cooldown = 3f;
            _acaciaConfig.MinCooldown = 0.75f;
            _acaciaConfig.BaseSplitCount = 2;
            _acaciaConfig.EstimatedSplitHitChance = 0.55f;
            _acaciaConfig.BounceCount = 2;
            _acaciaConfig.EstimatedBounceHitChance = 0.25f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_projectileConfig);
            Object.DestroyImmediate(_mainConfig);
            Object.DestroyImmediate(_acaciaConfig);
        }

        [Test]
        public void Estimate_MainWeapon_PreservesCurrentDamageAndCycleFormula()
        {
            WeaponPowerSnapshot power = ProjectileWeaponPowerEstimator.Estimate(
                _mainConfig,
                new WeaponRuntimeState());

            Assert.That(power.IsValid, Is.True);
            Assert.That(power.EstimatedDps, Is.EqualTo(15f).Within(0.001f));
            Assert.That(power.DamagePerProjectile, Is.EqualTo(10));
            Assert.That(power.ProjectilesPerShot, Is.EqualTo(1));
            Assert.That(power.SalvoShots, Is.EqualTo(1));
            Assert.That(power.ShotCycleTime, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void Estimate_AcaciaThorn_PreservesCurrentExpectedHitFormula()
        {
            AcaciaThornRuntimeState state = new();
            state.Unlock(_acaciaConfig.Damage);

            WeaponPowerSnapshot power = AcaciaThornWeaponPowerEstimator.Estimate(
                _acaciaConfig,
                state);

            Assert.That(power.IsValid, Is.True);
            Assert.That(power.EstimatedDps, Is.EqualTo(5.2f).Within(0.001f));
            Assert.That(power.DamagePerProjectile, Is.EqualTo(6));
            Assert.That(power.ProjectilesPerShot, Is.EqualTo(3));
            Assert.That(power.SalvoShots, Is.EqualTo(1));
            Assert.That(power.ShotCycleTime, Is.EqualTo(3f).Within(0.001f));
        }

        [Test]
        public void Estimate_CombinedWeapons_PreservesCurrentAggregationRules()
        {
            AcaciaThornRuntimeState acaciaState = new();
            acaciaState.Unlock(_acaciaConfig.Damage);

            WeaponPowerSnapshot power = WeaponPowerAggregator.Combine(
                ProjectileWeaponPowerEstimator.Estimate(
                    _mainConfig,
                    new WeaponRuntimeState()),
                AcaciaThornWeaponPowerEstimator.Estimate(
                    _acaciaConfig,
                    acaciaState));

            Assert.That(power.IsValid, Is.True);
            Assert.That(power.EstimatedDps, Is.EqualTo(20.2f).Within(0.001f));
            Assert.That(power.DamagePerProjectile, Is.EqualTo(10));
            Assert.That(power.ProjectilesPerShot, Is.EqualTo(4));
            Assert.That(power.SalvoShots, Is.EqualTo(1));
            Assert.That(power.ShotCycleTime, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void Provider_AggregatesAnyRegisteredWeaponPowerSources()
        {
            WeaponPowerProvider provider = new(new List<IWeaponPowerSource>
            {
                new StubPowerSource(new WeaponPowerSnapshot(true, 10f, 4, 1, 1, 2f)),
                new StubPowerSource(WeaponPowerSnapshot.Invalid),
                new StubPowerSource(new WeaponPowerSnapshot(true, 7f, 9, 3, 2, 4f))
            });

            WeaponPowerSnapshot power = provider.GetCurrentPower();

            Assert.That(power.IsValid, Is.True);
            Assert.That(power.EstimatedDps, Is.EqualTo(17f).Within(0.001f));
            Assert.That(power.DamagePerProjectile, Is.EqualTo(9));
            Assert.That(power.ProjectilesPerShot, Is.EqualTo(4));
            Assert.That(power.SalvoShots, Is.EqualTo(2));
            Assert.That(power.ShotCycleTime, Is.EqualTo(2f).Within(0.001f));
        }

        private static void SetPrivateField<T>(object target, string name, T value)
        {
            typeof(ProjectileConfig)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        private sealed class StubPowerSource : IWeaponPowerSource
        {
            private readonly WeaponPowerSnapshot _power;

            public StubPowerSource(WeaponPowerSnapshot power)
            {
                _power = power;
            }

            public WeaponPowerSnapshot GetCurrentPower()
            {
                return _power;
            }
        }
    }
}
