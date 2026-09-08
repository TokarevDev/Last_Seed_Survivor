namespace Game.Core.Combat
{
    public sealed class WeaponProgressionState
    {
        private readonly WeaponProgressionLimits _hardLimits;

        private DamageMultiplierProgressionState _damageMultiplier;
        private CappedBonusState _fireRateBonus;
        private SalvoProgressionState _salvo;
        private CappedBonusState _projectileSpeedBonus;
        private CriticalHitProgressionState _criticalHit;

        public WeaponProgressionState(
            in WeaponProgressionLimits initialLimits,
            in WeaponProgressionLimits hardLimits,
            float initialCriticalDamageMultiplier = 2f)
        {
            _hardLimits = hardLimits;
            WeaponProgressionLimits limits = initialLimits.ClampTo(hardLimits);

            _damageMultiplier = new DamageMultiplierProgressionState(limits.DamageMultiplier);
            _fireRateBonus = new CappedBonusState(limits.FireRateBonus);
            _salvo = new SalvoProgressionState(
                limits.SalvoExtraShots,
                hardLimits.SalvoExtraShots);
            _projectileSpeedBonus = new CappedBonusState(limits.ProjectileSpeedBonus);
            _criticalHit = new CriticalHitProgressionState(
                limits.CriticalChance,
                limits.CriticalDamageMultiplier,
                initialCriticalDamageMultiplier);
        }

        private WeaponProgressionState(
            in WeaponProgressionLimits hardLimits,
            DamageMultiplierProgressionState damageMultiplier,
            CappedBonusState fireRateBonus,
            SalvoProgressionState salvo,
            CappedBonusState projectileSpeedBonus,
            CriticalHitProgressionState criticalHit)
        {
            _hardLimits = hardLimits;
            _damageMultiplier = damageMultiplier;
            _fireRateBonus = fireRateBonus;
            _salvo = salvo;
            _projectileSpeedBonus = projectileSpeedBonus;
            _criticalHit = criticalHit;
        }

        public float DamageMultiplier => _damageMultiplier.Value;
        public float FireRateBonus => _fireRateBonus.Value;
        public int SalvoExtraShots => _salvo.ExtraShots;
        public float ProjectileSpeedBonus => _projectileSpeedBonus.Value;
        public float CriticalChance => _criticalHit.Chance;
        public float CriticalDamageMultiplier => _criticalHit.DamageMultiplier;

        public float MaxFireRateBonus => _fireRateBonus.Limit;
        public float MaxProjectileSpeedBonus => _projectileSpeedBonus.Limit;

        public bool CanAddDamageMultiplier => _damageMultiplier.CanAdd;
        public bool CanAddFireRateBonus => _fireRateBonus.CanAdd;
        public bool CanAddSalvoShots => _salvo.CanAdd;
        public bool CanAddProjectileSpeedBonus => _projectileSpeedBonus.CanAdd;
        public bool CanAddCriticalChance => _criticalHit.CanAddChance;
        public bool CanAddCriticalDamage => _criticalHit.CanAddDamage;

        public void Reset(float criticalDamageMultiplier = 2f)
        {
            _damageMultiplier.Reset();
            _fireRateBonus.Reset();
            _salvo.Reset();
            _projectileSpeedBonus.Reset();
            _criticalHit.Reset(criticalDamageMultiplier);
        }

        public void SetLimits(in WeaponProgressionLimits requestedLimits)
        {
            WeaponProgressionLimits limits = requestedLimits.ClampTo(_hardLimits);

            _damageMultiplier.SetLimit(limits.DamageMultiplier);
            _fireRateBonus.SetLimit(limits.FireRateBonus);
            _salvo.SetLimit(limits.SalvoExtraShots);
            _projectileSpeedBonus.SetLimit(limits.ProjectileSpeedBonus);
            _criticalHit.SetLimits(
                limits.CriticalChance,
                limits.CriticalDamageMultiplier);
        }

        public void SetCriticalDamageMultiplier(float multiplier)
        {
            _criticalHit.SetDamageMultiplier(multiplier);
        }

        public bool CanApplyDamageMultiplier(float multiplier) =>
            _damageMultiplier.CanApply(multiplier);

        public bool CanApplyFireRateBonus(float bonus) =>
            _fireRateBonus.CanApply(bonus);

        public bool CanApplySalvoShots(int extraShots) =>
            _salvo.CanApply(extraShots);

        public bool CanApplySalvoShots(int extraShots, int limitAfterApply) =>
            _salvo.CanApply(extraShots, limitAfterApply);

        public bool CanApplyProjectileSpeedBonus(float bonus) =>
            _projectileSpeedBonus.CanApply(bonus);

        public bool CanApplyCriticalChance(float bonus) =>
            _criticalHit.CanApplyChance(bonus);

        public bool CanApplyCriticalDamageBonus(float bonus) =>
            _criticalHit.CanApplyDamage(bonus);

        public float ApplyDamageMultiplier(float multiplier) =>
            _damageMultiplier.Apply(multiplier);

        public float AddFireRateBonus(float bonus) => _fireRateBonus.Add(bonus);

        public int AddSalvoShots(int extraShots) => _salvo.Add(extraShots);

        public float AddProjectileSpeedBonus(float bonus) =>
            _projectileSpeedBonus.Add(bonus);

        public float AddCriticalChance(
            float bonus,
            float minimumCriticalDamageMultiplier = 1f) =>
            _criticalHit.AddChance(bonus, minimumCriticalDamageMultiplier);

        public float AddCriticalDamageBonus(float bonus) =>
            _criticalHit.AddDamage(bonus);

        public void ExpandSalvoExtraShotLimit(int limit)
        {
            _salvo.ExpandLimit(limit);
        }

        public WeaponProgressionState Clone()
        {
            return new WeaponProgressionState(
                _hardLimits,
                _damageMultiplier.Clone(),
                _fireRateBonus.Clone(),
                _salvo.Clone(),
                _projectileSpeedBonus.Clone(),
                _criticalHit.Clone());
        }
    }
}
