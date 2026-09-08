
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Gameplay.Signals;

namespace Game.Presentation.Worm
{
    using System;
    using Zenject;

    public sealed class WormFaceBurstPresenter :
        IWormFaceBurstPresentation,
        IInitializable,
        IDisposable
    {
        private readonly SignalBus _signalBus;
        private IWormFaceBurstView _faceVisual;
        private bool _isBurstActive;

        public WormFaceBurstPresenter(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WormCombatBurstStateChangedSignal>(HandleStateChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<WormCombatBurstStateChangedSignal>(HandleStateChanged);
            Unbind();
        }

        public void Bind(IWormFaceBurstView faceVisual)
        {
            _faceVisual = faceVisual;
            _faceVisual?.SetBoostActive(_isBurstActive);
        }

        public void Unbind()
        {
            _faceVisual?.SetBoostActive(false);
            _faceVisual = null;
        }

        private void HandleStateChanged(WormCombatBurstStateChangedSignal signal)
        {
            _isBurstActive = signal.IsActive;
            _faceVisual?.SetBoostActive(_isBurstActive);
        }
    }

}
