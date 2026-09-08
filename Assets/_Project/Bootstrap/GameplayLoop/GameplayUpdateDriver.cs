using UnityEngine;
using Zenject;

namespace Game.Bootstrap.GameplayLoop
{
    [DisallowMultipleComponent]
    public sealed class GameplayUpdateDriver : MonoBehaviour
    {
        private GameplayFrameCoordinator _gameplayFrameCoordinator;

        [Inject]
        public void Construct(GameplayFrameCoordinator gameplayFrameCoordinator)
        {
            _gameplayFrameCoordinator = gameplayFrameCoordinator;
        }

        private void Update()
        {
            _gameplayFrameCoordinator.Tick(
                Time.deltaTime,
                Time.unscaledDeltaTime,
                Time.time,
                Time.unscaledTime);
        }
    }
}
