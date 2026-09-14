using Zenject;
using UnityEngine;

using Game.Gameplay.Signals;
using Game.Presentation.Signals;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.Worm;

namespace Game.Bootstrap.Installers.Game
{
    public sealed class GameSignalsInstaller : MonoInstaller
    {
        [SerializeField] private WormDamagePopupPresenter _damagePopupPresenter;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<WormDiedSignal>();
            Container.DeclareSignal<WormRewardRequestedSignal>();
            Container.DeclareSignal<WormReviveGrantedSignal>();
            Container.DeclareSignal<WormReviveRollbackCompletedSignal>();
            Container.DeclareSignal<WormCombatBurstStateChangedSignal>();
            Container.DeclareSignal<WormDestructionProgressChangedSignal>();
            Container.DeclareSignal<WormPathCompletedSignal>();
            Container.DeclareSignal<WeaponRuntimeStatsChangedSignal>();
            Container.DeclareSignal<WeaponAttackCycleStartedSignal>();
            Container.DeclareSignal<ShowPopupRequestedSignal>();
            Container.DeclareSignal<VictoryPopupIntentSignal>();

            Container.BindInterfacesAndSelfTo<WeaponRuntimeStatsSignalPublisher>().AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponAttackCycleSignalPublisher>().AsSingle();
            Container.Bind<IDamageViewRequestSink>()
                .FromInstance(_damagePopupPresenter)
                .AsSingle();
            Container.BindInterfacesTo<WormCombatSignalPublisher>().AsSingle();
        }
    }
}
