using Zenject;

using Game.Bootstrap.Application;

namespace Game.Bootstrap.Installers.Project
{
    public sealed class ApplicationBootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InitialSceneBootstrapper>().AsSingle();
        }
    }
}
