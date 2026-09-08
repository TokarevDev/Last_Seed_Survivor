using System;
using LastSeed.Core.Pooling;
using LastSeed.Gameplay.Signals;

namespace LastSeed.Bootstrap.Gameplay
{
    public sealed class AcaciaThornRuntimeInitializer
    {
        private readonly AcaciaThornWeapon _weapon;
        private readonly PoolRegistry _projectilePoolRegistry;
        private readonly PlayerWeaponLoadout _loadout;
        private readonly IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;
        private readonly IRandomSource _randomSource;
        private readonly IConfigurablePooledSpawnService<
            AcaciaThornProjectilePoolSetup,
            AcaciaThornProjectileSpawnRequest> _pool;

        public AcaciaThornRuntimeInitializer(
            AcaciaThornWeapon weapon,
            PoolRegistry projectilePoolRegistry,
            PlayerWeaponLoadout loadout,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher,
            IRandomSource randomSource,
            IConfigurablePooledSpawnService<
                AcaciaThornProjectilePoolSetup,
                AcaciaThornProjectileSpawnRequest> pool)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            _projectilePoolRegistry = projectilePoolRegistry ??
                throw new ArgumentNullException(nameof(projectilePoolRegistry));
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _runtimeStatsPublisher = runtimeStatsPublisher ??
                throw new ArgumentNullException(nameof(runtimeStatsPublisher));
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
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
                _randomSource,
                _pool);
        }
    }
}
