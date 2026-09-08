using System;

namespace Game.Gameplay.Combat
{
    public sealed class CombatSessionState : ICombatSessionState
    {
        public event Action<bool> ShootingEnabledChanged;

        public bool IsShootingEnabled { get; private set; }

        public void SetShootingEnabled(bool isEnabled)
        {
            if (IsShootingEnabled == isEnabled)
                return;

            IsShootingEnabled = isEnabled;
            ShootingEnabledChanged?.Invoke(isEnabled);
        }

        public void Reset()
        {
            SetShootingEnabled(false);
        }
    }
}
