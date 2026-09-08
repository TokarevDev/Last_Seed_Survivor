using System;

using Game.Core.Pooling;
using Game.Core.World;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Player;
using Game.Gameplay.Pooling;
using Game.Gameplay.Signals;

namespace Game.Bootstrap.Scenes.Game
{
    public sealed class AcaciaThornRuntimeInitializer
    {
        private readonly AcaciaThornWeapon _weapon;
        private readonly PoolRegistry _projectilePoolRegistry;
        private readonly PlayerWeaponLoadout _loadout;
        private readonly IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;
        private readonly AcaciaThornProjectileSpawnRequestFactory _spawnRequestFactory;
        private readonly AcaciaThornProjectilePool _pool;

        public AcaciaThornRuntimeInitializer(
            AcaciaThornWeapon weapon,
            PoolRegistry projectilePoolRegistry,
            PlayerWeaponLoadout loadout,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher,
            AcaciaThornProjectileSpawnRequestFactory spawnRequestFactory,
            AcaciaThornProjectilePool pool)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            _projectilePoolRegistry = projectilePoolRegistry ??
                throw new ArgumentNullException(nameof(projectilePoolRegistry));
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _runtimeStatsPublisher = runtimeStatsPublisher ??
                throw new ArgumentNullException(nameof(runtimeStatsPublisher));
            _spawnRequestFactory = spawnRequestFactory ??
                throw new ArgumentNullException(nameof(spawnRequestFactory));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public void Initialize(IScreenBounds screenBounds)
        {
            if (screenBounds == null)
                throw new ArgumentNullException(nameof(screenBounds));

            AcaciaThornWeaponConfig config = _weapon.Config;

            if (config == null)
                throw new InvalidOperationException("Acacia Thorn weapon config is missing.");

            if (config.ProjectilePrefab == null)
                throw new InvalidOperationException("Acacia Thorn projectile prefab is missing.");

            AcaciaThornProjectilePoolSetup poolSetup = new(
                config.ProjectilePrefab,
                _projectilePoolRegistry.transform,
                screenBounds,
                config.PrewarmCount);
            _pool.Initialize(in poolSetup);
            _weapon.Init(
                _loadout.FirePoint,
                _runtimeStatsPublisher,
                _spawnRequestFactory,
                _pool);
        }
    }
}
