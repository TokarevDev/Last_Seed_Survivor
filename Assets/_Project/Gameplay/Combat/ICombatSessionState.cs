using System;

namespace Game.Gameplay.Combat
{
    public interface ICombatSessionState
    {
        event Action<bool> ShootingEnabledChanged;

        bool IsShootingEnabled { get; }

        void SetShootingEnabled(bool isEnabled);
        void Reset();
    }
}
