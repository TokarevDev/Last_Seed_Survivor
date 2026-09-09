
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;
using Game.Gameplay.Rewards.Runtime;
using Game.Gameplay.Signals;

namespace Game.Gameplay.Rewards.Services
{
    public sealed class RewardApplyService : IRewardChoiceApplier, IRewardRuntimeContextProvider
    {
        private readonly ProjectileWeapon _weapon;
        private readonly AcaciaThornWeapon _acaciaThornWeapon;
        private readonly RewardRuntimeContext _context;

        public RewardApplyService(
            ProjectileWeapon weapon,
            AcaciaThornWeapon acaciaThornWeapon)
        {
            _weapon = weapon;
            _acaciaThornWeapon = acaciaThornWeapon;
            _context = new RewardRuntimeContext(
                weapon != null ? () => weapon.RuntimeState : null,
                acaciaThornWeapon != null ? () => acaciaThornWeapon.RuntimeState : null,
                weapon != null ? () => weapon.CurrentProjectileDamage : null,
                weapon != null ? () => weapon.Config : null,
                acaciaThornWeapon != null ? () => acaciaThornWeapon.Config : null);
        }

        public WeaponRuntimeState RuntimeState => _weapon != null ? _weapon.RuntimeState : null;
        public RewardRuntimeContext RuntimeContext => _context;

        public void Apply(RewardChoiceData choice)
        {
            if (choice == null || choice.Effect == null)
                return;

            if (_context == null)
                return;

            if (!choice.Effect.CanApply(_context))
                return;

            choice.Effect.Apply(_context);

            if (choice.Effect.AffectedWeapon == WeaponRuntimeStatsSource.AcaciaThorn)
                _acaciaThornWeapon?.ForceRebuild();
            else
                _weapon?.ForceRebuild();
        }
    }

}
