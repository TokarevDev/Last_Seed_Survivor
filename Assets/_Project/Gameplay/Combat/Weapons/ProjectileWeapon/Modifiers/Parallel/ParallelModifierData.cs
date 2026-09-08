
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon.Modifiers;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon.Modifiers.Parallel
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Combat/Modifiers/Parallel")]
    public sealed class ParallelModifierData : ShotModifierData
    {
        [Min(2)]
        public int Count = 2;

        [Min(0.1f)]
        public float Spacing = 0.5f;
    }

}
