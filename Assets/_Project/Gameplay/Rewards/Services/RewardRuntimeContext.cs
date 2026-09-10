using System;
using Game.Core;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon;
using Game.Gameplay.Combat.Weapons.AcaciaThornWeapon.Configs;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Combat.Weapons.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    public readonly struct RewardRollContext
    {
        public readonly float HeadPathProgressNormalized;
        public readonly float WormDestructionProgressNormalized;
        public readonly bool HasRevivedThisRun;
        public readonly bool IsPaidAssistRoll;

        public RewardRollContext(
            float headPathProgressNormalized,
            float wormDestructionProgressNormalized,
            bool hasRevivedThisRun,
            bool isPaidAssistRoll = false)
        {
            HeadPathProgressNormalized = FloatMath.Clamp01(headPathProgressNormalized);
            WormDestructionProgressNormalized = FloatMath.Clamp01(wormDestructionProgressNormalized);
            HasRevivedThisRun = hasRevivedThisRun;
            IsPaidAssistRoll = isPaidAssistRoll;
        }

        public RewardRollContext WithPaidAssistRoll()
        {
            return new RewardRollContext(
                HeadPathProgressNormalized,
                WormDestructionProgressNormalized,
                HasRevivedThisRun,
                true);
        }
    }

    public sealed class RewardRuntimeContext
    {
        private readonly WeaponRuntimeState _mainWeaponState;
        private readonly AcaciaThornRuntimeState _acaciaThornState;
        private readonly WeaponConfig _mainWeaponConfig;
        private readonly AcaciaThornWeaponConfig _acaciaThornConfig;
        private readonly Func<int> _mainWeaponDamageProvider;
        private readonly Func<WeaponRuntimeState> _mainWeaponStateProvider;
        private readonly Func<AcaciaThornRuntimeState> _acaciaThornStateProvider;
        private readonly Func<WeaponConfig> _mainWeaponConfigProvider;
        private readonly Func<AcaciaThornWeaponConfig> _acaciaThornConfigProvider;

        public RewardRuntimeContext(
            Func<WeaponRuntimeState> mainWeaponStateProvider,
            Func<AcaciaThornRuntimeState> acaciaThornStateProvider,
            Func<int> mainWeaponDamageProvider,
            Func<WeaponConfig> mainWeaponConfigProvider,
            Func<AcaciaThornWeaponConfig> acaciaThornConfigProvider)
        {
            _mainWeaponStateProvider = mainWeaponStateProvider;
            _acaciaThornStateProvider = acaciaThornStateProvider;
            _mainWeaponDamageProvider = mainWeaponDamageProvider;
            _mainWeaponConfigProvider = mainWeaponConfigProvider;
            _acaciaThornConfigProvider = acaciaThornConfigProvider;
        }

        public RewardRuntimeContext(
            WeaponRuntimeState mainWeaponState,
            AcaciaThornRuntimeState acaciaThornState,
            Func<int> mainWeaponDamageProvider = null,
            WeaponConfig mainWeaponConfig = null,
            AcaciaThornWeaponConfig acaciaThornConfig = null)
        {
            _mainWeaponState = mainWeaponState;
            _acaciaThornState = acaciaThornState;
            _mainWeaponDamageProvider = mainWeaponDamageProvider;
            _mainWeaponConfig = mainWeaponConfig;
            _acaciaThornConfig = acaciaThornConfig;
        }

        public WeaponConfig MainWeaponConfig =>
            _mainWeaponConfigProvider != null
                ? _mainWeaponConfigProvider()
                : _mainWeaponConfig;

        public AcaciaThornWeaponConfig AcaciaThornConfig =>
            _acaciaThornConfigProvider != null
                ? _acaciaThornConfigProvider()
                : _acaciaThornConfig;

        public WeaponRuntimeState MainWeaponState =>
            _mainWeaponStateProvider != null
                ? _mainWeaponStateProvider()
                : _mainWeaponState;

        public AcaciaThornRuntimeState AcaciaThornState =>
            _acaciaThornStateProvider != null
                ? _acaciaThornStateProvider()
                : _acaciaThornState;

        public int MainWeaponDamageSnapshot
        {
            get
            {
                if (_mainWeaponDamageProvider != null)
                    return _mainWeaponDamageProvider();

                return 0;
            }
        }
    }

}
