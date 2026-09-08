namespace Game.Gameplay.Signals
{
    public enum WeaponRuntimeStatsSource
    {
        MainProjectile,
        AcaciaThorn
    }

    public sealed class WeaponRuntimeStatsChangedSignal
    {
        public WeaponRuntimeStatsChangedSignal(
            in WeaponRuntimeStatsSnapshot snapshot,
            float occurredAt)
        {
            Snapshot = snapshot;
            OccurredAt = occurredAt;
        }

        public WeaponRuntimeStatsSnapshot Snapshot { get; }
        public WeaponRuntimeStatsSource Source => Snapshot.Source;
        public float OccurredAt { get; }
    }
}
