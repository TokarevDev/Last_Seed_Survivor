
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public sealed class WeaponPowerProvider : IWeaponPowerProvider
    {
        private readonly ProjectileWeapon _mainWeapon;
        private readonly AcaciaThornWeapon _acaciaThornWeapon;

        public WeaponPowerProvider(
            ProjectileWeapon mainWeapon,
            AcaciaThornWeapon acaciaThornWeapon)
        {
            _mainWeapon = mainWeapon;
            _acaciaThornWeapon = acaciaThornWeapon;
        }

        public WeaponPowerSnapshot GetCurrentPower()
        {
            return WeaponPowerEstimator.Estimate(_mainWeapon, _acaciaThornWeapon);
        }
    }

}
