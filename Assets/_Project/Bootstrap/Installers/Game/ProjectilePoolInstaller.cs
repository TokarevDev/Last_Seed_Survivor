using UnityEngine;
using Zenject;

using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Pooling;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class ProjectilePoolInstaller : MonoInstaller
    {
        [SerializeField] private PoolRegistry _projectilePoolRegistry;

        public override void InstallBindings()
        {
            Container.Bind<PoolRegistry>().FromInstance(_projectilePoolRegistry).AsSingle();
            Container
                .BindInterfacesAndSelfTo<AcaciaThornProjectilePool>()
                .AsSingle();
        }
    }
}
