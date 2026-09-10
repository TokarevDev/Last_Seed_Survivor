using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Rewards.Services;
using Game.Gameplay.Signals;
using UnityEngine;

namespace Game.Gameplay.Combat.Rewards.Effects
{
    public abstract class RewardEffect : ScriptableObject
    {
        public virtual WeaponRuntimeStatsSource AffectedWeapon =>
            WeaponRuntimeStatsSource.MainProjectile;

        public virtual bool CanApply(RewardRuntimeContext context)
        {
            return context != null && CanApply(context.MainWeaponState);
        }

        public virtual bool CanApply(WeaponRuntimeState state)
        {
            return state != null;
        }

        public virtual void Apply(RewardRuntimeContext context)
        {
            if (context == null)
                return;

            Apply(context.MainWeaponState);
        }

        public abstract void Apply(WeaponRuntimeState state);
    }

}
