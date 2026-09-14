using System;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public sealed class AcaciaThornWeaponPowerSource : IWeaponPowerSource
    {
        private readonly AcaciaThornWeapon _weapon;

        public AcaciaThornWeaponPowerSource(AcaciaThornWeapon weapon)
        {
            _weapon = weapon != null
                ? weapon
                : throw new ArgumentNullException(nameof(weapon));
        }

        public WeaponPowerSnapshot GetCurrentPower()
        {
            return AcaciaThornWeaponPowerEstimator.Estimate(
                _weapon.Config,
                _weapon.RuntimeState);
        }
    }
}
