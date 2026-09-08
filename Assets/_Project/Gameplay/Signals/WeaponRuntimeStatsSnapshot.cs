namespace Game.Gameplay.Signals
{
    public readonly struct WeaponRuntimeStatsSnapshot
    {
        public WeaponRuntimeStatsSnapshot(
            WeaponRuntimeStatsSource source,
            bool isActive,
            int projectileDamage,
            float shotCooldown,
            float projectileSpeedMultiplier,
            int salvoShots,
            int projectilesPerShot,
            int penetrationBonus,
            float criticalChance,
            float criticalDamageMultiplier)
        {
            Source = source;
            IsActive = isActive;
            ProjectileDamage = projectileDamage;
            ShotCooldown = shotCooldown;
            ProjectileSpeedMultiplier = projectileSpeedMultiplier;
            SalvoShots = salvoShots;
            ProjectilesPerShot = projectilesPerShot;
            PenetrationBonus = penetrationBonus;
            CriticalChance = criticalChance;
            CriticalDamageMultiplier = criticalDamageMultiplier;
        }

        public WeaponRuntimeStatsSource Source { get; }
        public bool IsActive { get; }
        public int ProjectileDamage { get; }
        public float ShotCooldown { get; }
        public float ProjectileSpeedMultiplier { get; }
        public int SalvoShots { get; }
        public int ProjectilesPerShot { get; }
        public int PenetrationBonus { get; }
        public float CriticalChance { get; }
        public float CriticalDamageMultiplier { get; }
    }
}
