namespace LastSeed.Core.Pooling
{
    public interface IPooledSpawnService<TRequest>
        where TRequest : struct
    {
        bool IsInitialized { get; }

        void Spawn(in TRequest request);

        void ReleaseAll();
    }

    public interface IConfigurablePooledSpawnService<TSetup, TRequest> :
        IPooledSpawnService<TRequest>
        where TSetup : struct
        where TRequest : struct
    {
        void Initialize(in TSetup setup);
    }
}
