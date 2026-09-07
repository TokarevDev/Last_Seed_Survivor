using LastSeed.Core.Pooling;
using UnityEngine;

public interface IWeapon
{
    void Init(
        IPooledSpawnService<ProjectileSpawnRequest> pool,
        Transform firePoint);

    void Tick(float deltaTime);

    void ApplyConfig(WeaponConfig config);
}
