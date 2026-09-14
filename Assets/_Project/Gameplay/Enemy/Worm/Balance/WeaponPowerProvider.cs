using System;
using System.Collections.Generic;

namespace Game.Gameplay.Enemy.Worm.Balance
{
    public sealed class WeaponPowerProvider : IWeaponPowerProvider
    {
        private readonly IReadOnlyList<IWeaponPowerSource> _sources;

        public WeaponPowerProvider(List<IWeaponPowerSource> sources)
        {
            _sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        public WeaponPowerSnapshot GetCurrentPower()
        {
            WeaponPowerSnapshot result = WeaponPowerSnapshot.Invalid;

            for (int i = 0; i < _sources.Count; i++)
            {
                IWeaponPowerSource source = _sources[i];

                if (source != null)
                {
                    result = WeaponPowerAggregator.Combine(
                        result,
                        source.GetCurrentPower());
                }
            }

            return result;
        }
    }
}
