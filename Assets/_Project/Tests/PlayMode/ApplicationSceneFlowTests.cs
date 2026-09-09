using System.Collections;
using Game.Infrastructure.Navigation;
using Game.Presentation.UI.Common;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests.PlayMode
{
    public sealed class ApplicationSceneFlowTests
    {
        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator BootstrapToLobbyToGameplayToLobby_CompletesThroughUiRoutes()
        {
            yield return _flow.LoadSceneAndWaitFor(
                GameSceneNames.Bootstrap,
                GameSceneNames.Lobby,
                "waiting for Bootstrap to route to Lobby");

            LobbyStartBattleButton startBattle =
                _flow.FindInActiveScene<LobbyStartBattleButton>();
            Assert.That(startBattle, Is.Not.Null);
            Assert.That(startBattle.TryGetComponent(out Button lobbyButton), Is.True);

            lobbyButton.onClick.Invoke();
            yield return _flow.WaitForActiveScene(
                GameSceneNames.Gameplay,
                "waiting for the Lobby battle button to route to Game");

            GameplayBackToLobbyButton backToLobby =
                _flow.FindInActiveScene<GameplayBackToLobbyButton>();
            Assert.That(backToLobby, Is.Not.Null);
            Assert.That(backToLobby.TryGetComponent(out Button gameplayButton), Is.True);

            gameplayButton.onClick.Invoke();
            yield return _flow.WaitForActiveScene(
                GameSceneNames.Lobby,
                "waiting for the Game back button to route to Lobby");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameSceneNames.Lobby));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }
    }
}
