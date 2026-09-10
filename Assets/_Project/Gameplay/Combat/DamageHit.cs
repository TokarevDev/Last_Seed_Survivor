using Game.Core.Combat;
using UnityEngine;

namespace Game.Gameplay.Combat
{
    public readonly struct DamageHit
    {
        public DamageHit(in DamageInfo damage, Vector3 hitPosition)
        {
            Damage = damage;
            HitPosition = hitPosition;
        }

        public DamageInfo Damage { get; }
        public Vector3 HitPosition { get; }
    }

}
