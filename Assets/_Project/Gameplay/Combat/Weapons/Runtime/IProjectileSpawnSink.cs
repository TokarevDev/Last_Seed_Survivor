namespace Game.Gameplay.Combat.Weapons.Runtime
{
    public interface IProjectileSpawnSink<TRequest>
        where TRequest : struct
    {
        bool IsReady { get; }

        void Spawn(in TRequest request);

        void Clear();
    }
}
