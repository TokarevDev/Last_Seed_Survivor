using Game.Core.World;
using UnityEngine;

namespace Game.Gameplay.Combat.Weapons.AcaciaThornWeapon
{
    public readonly struct AcaciaThornProjectilePoolSetup
    {
        public AcaciaThornProjectilePoolSetup(
            AcaciaThornProjectile prefab,
            Transform parent,
            IScreenBounds screenBounds,
            int prewarmCount)
        {
            Prefab = prefab;
            Parent = parent;
            ScreenBounds = screenBounds;
            PrewarmCount = prewarmCount;
        }

        public AcaciaThornProjectile Prefab { get; }
        public Transform Parent { get; }
        public IScreenBounds ScreenBounds { get; }
        public int PrewarmCount { get; }
    }

}
