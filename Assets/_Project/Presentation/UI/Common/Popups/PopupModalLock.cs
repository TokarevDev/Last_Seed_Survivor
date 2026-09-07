using System;
using LastSeed.Core.Timing;
using LastSeed.Gameplay.Input;

namespace LastSeed.Presentation.UI.Popups
{
    public sealed class PopupModalLock : IDisposable
    {
        private readonly IGameplayInputLock _gameplayInputLock;
        private readonly ITimeScaleController _timeScaleController;
        private readonly bool _pauseTime;

        private IDisposable _gameplayInputLockHandle;
        private float _timeScaleBeforeLock;
        private bool _hasTimeScaleLock;

        public PopupModalLock(
            IGameplayInputLock gameplayInputLock,
            ITimeScaleController timeScaleController,
            bool pauseTime)
        {
            _gameplayInputLock = gameplayInputLock ??
                throw new ArgumentNullException(nameof(gameplayInputLock));
            _timeScaleController = timeScaleController ??
                throw new ArgumentNullException(nameof(timeScaleController));
            _pauseTime = pauseTime;
        }

        public bool IsAcquired => _gameplayInputLockHandle != null;

        public void Acquire()
        {
            if (_gameplayInputLockHandle != null)
                return;

            IDisposable inputLockHandle = _gameplayInputLock.Acquire() ??
                throw new InvalidOperationException(
                    "Gameplay input lock returned no ownership handle.");

            try
            {
                if (_pauseTime)
                {
                    _timeScaleBeforeLock = _timeScaleController.TimeScale;
                    _timeScaleController.TimeScale = 0f;
                    _hasTimeScaleLock = true;
                }

                _gameplayInputLockHandle = inputLockHandle;
            }
            catch
            {
                inputLockHandle?.Dispose();
                throw;
            }
        }

        public void Release()
        {
            IDisposable inputLockHandle = _gameplayInputLockHandle;
            _gameplayInputLockHandle = null;

            try
            {
                inputLockHandle?.Dispose();
            }
            finally
            {
                if (_hasTimeScaleLock)
                {
                    _timeScaleController.TimeScale = _timeScaleBeforeLock;
                    _hasTimeScaleLock = false;
                }
            }
        }

        public void Dispose()
        {
            Release();
        }
    }
}
