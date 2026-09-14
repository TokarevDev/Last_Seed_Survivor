namespace Game.Gameplay.Signals
{
    public interface IDamageViewRequestSink
    {
        void Present(in DamageViewRequest request);
    }
}
