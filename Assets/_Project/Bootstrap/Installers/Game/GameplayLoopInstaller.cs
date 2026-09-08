using Zenject;

using Game.Bootstrap.GameplayLoop;
using Game.Bootstrap.Scenes.Game;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameplayLoopInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameplayFrameCoordinator>().AsSingle();

            Container
                .Bind<GameplayUpdateDriver>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName(nameof(GameplayUpdateDriver))
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesTo<GameSceneInitializer>()
                .AsSingle();
        }
    }
}
