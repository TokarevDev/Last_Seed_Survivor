
using Game.Core.Combat;
using Game.Core.Pooling;
using Game.Core.World;

namespace Game.Gameplay.Combat.Weapons.AcaciaThornWeapon
{
    using System;
    using UnityEngine;

    public sealed class AcaciaThornProjectilePool :
        IConfigurablePooledSpawnService<
            AcaciaThornProjectilePoolSetup,
            AcaciaThornProjectileSpawnRequest>
    {
        private ObjectPool<AcaciaThornProjectile> _pool;

        private AcaciaThornProjectile _prefab;
        private Transform _parent;
        private IScreenBounds _screenBounds;
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public void Initialize(in AcaciaThornProjectilePoolSetup setup)
        {
            InitializePool(
                setup.Prefab,
                setup.Parent,
                setup.ScreenBounds,
                setup.PrewarmCount);
        }

        private void InitializePool(
            AcaciaThornProjectile prefab,
            Transform parent,
            IScreenBounds screenBounds,
            int prewarmCount)
        {
            if (_initialized)
                return;

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (screenBounds == null)
                throw new ArgumentNullException(nameof(screenBounds));

            _prefab = prefab;
            _parent = parent;
            _screenBounds = screenBounds;

            _pool = new ObjectPool<AcaciaThornProjectile>(CreateNew, Deactivate);
            _pool.Prewarm(Math.Max(0, prewarmCount));

            _initialized = true;
        }

        public AcaciaThornProjectile Spawn(
            Vector3 position,
            Vector2 direction,
            int damage,
            DamageKind damageKind,
            bool isCritical,
            float speed,
            float lifeTime,
            int bounces,
            int splitCount,
            bool canSplit)
        {
            AcaciaThornProjectileSpawnRequest request = new(
                position,
                direction,
                damage,
                damageKind,
                isCritical,
                speed,
                lifeTime,
                bounces,
                splitCount,
                canSplit);

            return _pool.Rent(request, InitializeProjectile);
        }

        public void Spawn(in AcaciaThornProjectileSpawnRequest request)
        {
            _pool.Rent(request, InitializeProjectile);
        }

        public void Release(AcaciaThornProjectile projectile)
        {
            _pool?.Return(projectile);
        }

        public void ReleaseAllActive()
        {
            _pool?.ReturnAll();
        }

        public void ReleaseAll()
        {
            ReleaseAllActive();
        }

        private AcaciaThornProjectile CreateNew()
        {
            AcaciaThornProjectile projectile = UnityEngine.Object.Instantiate(_prefab, _parent);
            projectile.Init(this, _screenBounds);
            return projectile;
        }

        private static void Deactivate(AcaciaThornProjectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private static void InitializeProjectile(
            AcaciaThornProjectile projectile,
            in AcaciaThornProjectileSpawnRequest request)
        {
            projectile.Activate(
                request.Position,
                request.Direction,
                request.Damage,
                request.DamageKind,
                request.IsCritical,
                request.Speed,
                request.LifeTime,
                request.Bounces,
                request.SplitCount,
                request.CanSplit);
        }
    }

}
