using UnityEngine;
using Zenject;

using Game.Gameplay.Input;
using Game.Infrastructure.Input;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameplayInputInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputSnapshotProvider _playerInputSnapshotProvider;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<PlayerInputSnapshotProvider>()
                .FromInstance(_playerInputSnapshotProvider)
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<GameplayInputLock>()
                .AsSingle();
        }
    }
}
