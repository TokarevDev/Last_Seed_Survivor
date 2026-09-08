using System;

namespace Game.Gameplay.Input
{
    public interface IGameplayInputLock
    {
        bool IsLocked { get; }

        IDisposable Acquire();
    }
}
