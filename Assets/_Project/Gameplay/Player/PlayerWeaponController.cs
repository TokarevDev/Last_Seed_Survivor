using System;
using Game.Gameplay.Combat;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern;
using Game.Gameplay.Pooling;
using Game.Gameplay.Signals;

namespace Game.Gameplay.Player
{
    public sealed class PlayerWeaponController
    {
        private readonly ProjectileWeapon _mainWeapon;
        private readonly AcaciaThornWeapon _acaciaThornWeapon;
        private readonly PoolRegistry _poolRegistry;
        private readonly PlayerWeaponLoadout _loadout;
        private readonly ICombatSessionState _combatSessionState;
        private readonly IShotPatternBuilder _shotPatternBuilder;
        private readonly ProjectileSpawnRequestFactory _spawnRequestFactory;
        private readonly IWeaponAttackCyclePublisher _attackCyclePublisher;
        private readonly IWeaponRuntimeStatsPublisher _runtimeStatsPublisher;
        private bool _initialized;

        public PlayerWeaponController(
            ProjectileWeapon mainWeapon,
            AcaciaThornWeapon acaciaThornWeapon,
            PoolRegistry poolRegistry,
            PlayerWeaponLoadout loadout,
            ICombatSessionState combatSessionState,
            IShotPatternBuilder shotPatternBuilder,
            ProjectileSpawnRequestFactory spawnRequestFactory,
            IWeaponAttackCyclePublisher attackCyclePublisher,
            IWeaponRuntimeStatsPublisher runtimeStatsPublisher)
        {
            _mainWeapon = mainWeapon ?? throw new ArgumentNullException(nameof(mainWeapon));
            _acaciaThornWeapon = acaciaThornWeapon ??
                throw new ArgumentNullException(nameof(acaciaThornWeapon));
            _poolRegistry = poolRegistry ?? throw new ArgumentNullException(nameof(poolRegistry));
            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));
            _combatSessionState = combatSessionState ?? throw new ArgumentNullException(nameof(combatSessionState));
            _shotPatternBuilder = shotPatternBuilder ??
                throw new ArgumentNullException(nameof(shotPatternBuilder));
            _spawnRequestFactory = spawnRequestFactory ??
                throw new ArgumentNullException(nameof(spawnRequestFactory));
            _attackCyclePublisher = attackCyclePublisher ??
                throw new ArgumentNullException(nameof(attackCyclePublisher));
            _runtimeStatsPublisher = runtimeStatsPublisher ??
                throw new ArgumentNullException(nameof(runtimeStatsPublisher));
        }

        public WeaponConfig StartConfig => _loadout.StartConfig;

        public void Initialize()
        {
            if (_initialized)
                return;

            WeaponConfig config = _loadout.StartConfig;

            if (config.Projectile == null)
                throw new InvalidOperationException("Player start weapon projectile config is missing.");

            ProjectilePool pool = _poolRegistry.GetPool(config.Projectile.Prefab);

            if (pool == null)
                throw new InvalidOperationException("Player start weapon projectile pool could not be created.");

            _mainWeapon.Init(
                pool,
                _loadout.FirePoint,
                _shotPatternBuilder,
                _spawnRequestFactory,
                _attackCyclePublisher,
                _runtimeStatsPublisher);
            _mainWeapon.ApplyConfig(config);
            _initialized = true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized || !_combatSessionState.IsShootingEnabled)
                return;

            _mainWeapon.Tick(deltaTime);
            _acaciaThornWeapon.Tick(deltaTime);
        }

        public void ClearTransientState()
        {
            _poolRegistry.ReleaseAllActiveProjectiles();
            _mainWeapon.ClearTransientState();
            _acaciaThornWeapon.ClearTransientState();
        }

        public void ResetRuntimeState()
        {
            _mainWeapon.ResetRuntimeState();
            _acaciaThornWeapon.ResetRuntimeState();
        }
    }

}
