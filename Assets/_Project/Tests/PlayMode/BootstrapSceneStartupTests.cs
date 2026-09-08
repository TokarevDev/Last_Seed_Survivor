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
        private const float LobbyLoadTimeoutSeconds = 10f;

        [UnityTest]
        public IEnumerator BootstrapScene_LoadsLobbyThroughApplicationEntryPoint()
        {
            AsyncOperation bootstrapLoad = SceneManager.LoadSceneAsync(
                GameSceneNames.Bootstrap,
                LoadSceneMode.Single);

            Assert.That(bootstrapLoad, Is.Not.Null);
            yield return bootstrapLoad;

            float deadline = Time.realtimeSinceStartup + LobbyLoadTimeoutSeconds;

            yield return new WaitUntil(() =>
                SceneManager.GetActiveScene().name == GameSceneNames.Lobby
                || Time.realtimeSinceStartup >= deadline);

            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo(GameSceneNames.Lobby));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (ProjectContext.HasInstance)
                Object.Destroy(ProjectContext.Instance.gameObject);

            yield return null;
        }
    }
}
