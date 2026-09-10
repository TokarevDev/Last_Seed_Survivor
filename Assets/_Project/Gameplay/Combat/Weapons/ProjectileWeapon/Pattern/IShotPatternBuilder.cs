using System.Collections.Generic;
using Game.Gameplay.Combat.Weapons.Runtime;
using UnityEngine;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern
{
    public interface IShotPatternBuilder
    {
        void Build(
            Vector3 origin,
            Quaternion rotation,
            WeaponRuntimeState runtimeState,
            List<ShotSpawnData> shots);
    }
}
