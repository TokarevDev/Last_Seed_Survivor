using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Infrastructure.Navigation
{
    public interface ISceneTransition
    {
        UniTask PlayAsync(CancellationToken cancellationToken);
    }
}
