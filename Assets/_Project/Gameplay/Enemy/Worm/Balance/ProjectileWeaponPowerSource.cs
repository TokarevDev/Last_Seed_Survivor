using System;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public sealed class ProjectileWeaponPowerSource : IWeaponPowerSource
    {
        private readonly ProjectileWeapon _weapon;

        public ProjectileWeaponPowerSource(ProjectileWeapon weapon)
        {
            _weapon = weapon != null
                ? weapon
                : throw new ArgumentNullException(nameof(weapon));
        }

        public WeaponPowerSnapshot GetCurrentPower()
        {
            return ProjectileWeaponPowerEstimator.Estimate(
                _weapon.Config,
                _weapon.RuntimeState);
        }
    }
}
