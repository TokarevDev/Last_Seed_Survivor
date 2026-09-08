using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Navigation
{
    public interface ISceneLoadOperation
    {
        UniTask WaitUntilReadyAsync(CancellationToken cancellationToken);
        void Activate();
        UniTask WaitUntilCompletedAsync(CancellationToken cancellationToken);
    }
}
