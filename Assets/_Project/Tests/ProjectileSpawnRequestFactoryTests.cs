using Game.Gameplay.Combat.Projectiles.Configs;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ProjectileSpawnRequestFactoryTests
    {
        private ProjectileConfig _projectileConfig;
        private WeaponConfig _weaponConfig;
        private AcaciaThornWeaponConfig _acaciaConfig;

        [SetUp]
        public void SetUp()
        {
            _projectileConfig = ScriptableObject.CreateInstance<ProjectileConfig>();
            _weaponConfig = ScriptableObject.CreateInstance<WeaponConfig>();
            _weaponConfig.Projectile = _projectileConfig;
            _acaciaConfig = ScriptableObject.CreateInstance<AcaciaThornWeaponConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_projectileConfig);
            Object.DestroyImmediate(_weaponConfig);
            Object.DestroyImmediate(_acaciaConfig);
        }

        [Test]
        public void MainFactory_BuildsTypedPayloadFromRuntimeState()
        {
            WeaponRuntimeState state = new();
            state.ApplyDamageMultiplier(2f);
            ShotSpawnData shot = new(new Vector3(2f, 3f, 0f), Quaternion.Euler(0f, 0f, 45f));

            ProjectileSpawnRequest request = new ProjectileSpawnRequestFactory().Create(
                _weaponConfig,
                state,
                in shot);

            Assert.That(request.Config, Is.SameAs(_projectileConfig));
            Assert.That(request.Stats.Damage, Is.EqualTo(10));
            Assert.That(request.Position, Is.EqualTo(shot.Position));
            Assert.That(request.Rotation, Is.EqualTo(shot.Rotation));
        }

        [Test]
        public void AcaciaFactory_BuildsTypeSpecificPayloadAndOffset()
        {
            AcaciaThornRuntimeState state = new();
            state.Unlock(_acaciaConfig.Damage);
            AcaciaThornProjectileSpawnRequestFactory factory = new(new TestRandomSource());

            AcaciaThornProjectileSpawnRequest request = factory.Create(
                _acaciaConfig,
                state,
                new Vector3(1f, 2f, 0f),
                Vector2.up);

            Assert.That(request.Position, Is.EqualTo(new Vector3(1f, 2.3f, 0f)));
            Assert.That(request.Direction, Is.EqualTo(Vector2.up));
            Assert.That(request.Damage, Is.EqualTo(_acaciaConfig.Damage));
            Assert.That(request.Speed, Is.EqualTo(_acaciaConfig.Speed));
            Assert.That(request.SplitCount, Is.EqualTo(_acaciaConfig.BaseSplitCount));
            Assert.That(request.CanSplit, Is.True);
        }
    }
}
