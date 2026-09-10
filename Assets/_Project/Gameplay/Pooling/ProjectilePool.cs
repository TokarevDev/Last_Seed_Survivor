using Game.Core.Pooling;
using Game.Core.Randomization;
using Game.Core.World;
using Game.Gameplay.Combat.Projectiles;
using Game.Gameplay.Combat.Weapons.Runtime;
using UnityEngine;

namespace Game.Gameplay.Pooling
{
    [DisallowMultipleComponent]
    public sealed class ProjectilePool : MonoBehaviour,
        IPooledSpawnService<ProjectileSpawnRequest>,
        IProjectileSpawnSink<ProjectileSpawnRequest>
    {
        [SerializeField] private int _prewarmCount = 40;

        private Projectile _prefab;
        private IScreenBounds _screenBounds;
        private IRandomSource _randomSource;
        private ObjectPool<Projectile> _pool;
        private bool _initialized;

        public bool IsInitialized => _initialized;
        public bool IsReady => _initialized;

        public void SetPrefab(
            Projectile prefab,
            IScreenBounds screenBounds,
            IRandomSource randomSource)
        {
            if (_initialized) return;

            _prefab = prefab;
            _screenBounds = screenBounds;
            _randomSource = randomSource;
            _pool = new ObjectPool<Projectile>(CreateNew, Deactivate);
            _pool.Prewarm(_prewarmCount);
            _initialized = true;
        }

        public void Spawn(in ProjectileSpawnRequest request)
        {
            _pool.Rent(request, InitializeProjectile);
        }

        public void Release(Projectile projectile)
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

        public void Clear()
        {
            ReleaseAllActive();
        }

        private Projectile CreateNew()
        {
            var projectile = Instantiate(_prefab, transform);
            projectile.Init(this, _screenBounds, _randomSource);
            return projectile;
        }

        private static void Deactivate(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private static void InitializeProjectile(
            Projectile projectile,
            in ProjectileSpawnRequest request)
        {
            projectile.ApplyConfig(request.Config, request.Stats);
            projectile.Activate(request.Position, request.Rotation);
        }
    }

}
