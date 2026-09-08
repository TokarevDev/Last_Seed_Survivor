using Game.Gameplay.Combat.Weapons.Runtime;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IShotPatternBuilder
    {
        void Build(
            Vector3 origin,
            Quaternion rotation,
            WeaponRuntimeState runtimeState,
            List<ShotSpawnData> shots);
    }
}
