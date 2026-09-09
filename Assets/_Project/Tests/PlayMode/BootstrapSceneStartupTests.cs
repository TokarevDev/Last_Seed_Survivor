using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

using Game.Infrastructure.Navigation;

namespace Game.Tests.PlayMode
{
    public sealed class BootstrapSceneStartupTests
    {
        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator BootstrapScene_LoadsLobbyThroughApplicationEntryPoint()
        {
            yield return _flow.LoadSceneAndWaitFor(
                GameSceneNames.Bootstrap,
                GameSceneNames.Lobby,
                "waiting for the application entry point");

            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo(GameSceneNames.Lobby));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }
    }
}
