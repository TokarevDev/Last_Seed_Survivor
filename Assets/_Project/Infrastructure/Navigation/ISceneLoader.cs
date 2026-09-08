namespace Game.Infrastructure.Navigation
{
    public interface ISceneLoader
    {
        ISceneLoadOperation BeginLoad(string sceneName);
    }
}
