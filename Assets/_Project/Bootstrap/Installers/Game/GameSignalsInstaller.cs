using Zenject;

using Game.Gameplay.Signals;
using Game.Presentation.Signals;
using Game.Presentation.UI.Common.Popups;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<WormDiedSignal>();
            Container.DeclareSignal<WormRewardRequestedSignal>();
            Container.DeclareSignal<WormReviveGrantedSignal>();
            Container.DeclareSignal<WormReviveRollbackCompletedSignal>();
            Container.DeclareSignal<WormCombatBurstStateChangedSignal>();
            Container.DeclareSignal<WormDamageDealtSignal>();
            Container.DeclareSignal<WormDestructionProgressChangedSignal>();
            Container.DeclareSignal<WormPathCompletedSignal>();
            Container.DeclareSignal<WeaponRuntimeStatsChangedSignal>();
            Container.DeclareSignal<WeaponAttackCycleStartedSignal>();
            Container.DeclareSignal<ShowPopupRequestedSignal>();
            Container.DeclareSignal<VictoryPopupIntentSignal>();

            Container.BindInterfacesAndSelfTo<WeaponRuntimeStatsSignalPublisher>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponAttackCycleSignalPublisher>().AsSingle();
            Container.BindInterfacesTo<WormCombatSignalPublisher>().AsSingle();
        }
    }
}
