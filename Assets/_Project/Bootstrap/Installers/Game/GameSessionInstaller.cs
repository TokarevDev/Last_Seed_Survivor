using Zenject;

using Game.Gameplay.Combat;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameSessionInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<CombatSessionState>()
                .AsSingle();
        }
    }
}
