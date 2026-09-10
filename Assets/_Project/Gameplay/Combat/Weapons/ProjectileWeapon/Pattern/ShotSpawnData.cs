using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using UnityEngine;

namespace Game.Gameplay.Combat.Weapons.ProjectileWeapon.Pattern
{
    public struct ShotSpawnData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public ShotSpawnData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

}
