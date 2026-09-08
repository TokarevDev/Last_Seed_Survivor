using UnityEngine;
using Zenject;

using Game.Core.Random;
using Game.Core.Timing;
using Game.Gameplay.World;
using Game.Infrastructure.Random;
using Game.Infrastructure.Timing;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameWorldInstaller : MonoInstaller
    {
        [Header("World")]
        [SerializeField] private Camera _worldCamera;

        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromInstance(_worldCamera).AsSingle();
            Container.Bind<IRandomSource>().To<UnityRandomSource>().AsSingle();
            Container
                .Bind<ITimeScaleController>()
                .To<UnityTimeScaleController>()
                .AsSingle();
            Container
                .Bind<IGameTimeProvider>()
                .To<UnityScaledGameTimeProvider>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<ScreenBoundsService>()
                .AsSingle();
        }
    }
}
