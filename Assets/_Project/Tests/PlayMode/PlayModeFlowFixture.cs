using System.Collections;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Tests.PlayMode
{
    internal sealed class PlayModeFlowFixture
    {
        private const float DefaultSceneTimeoutSeconds = 10f;
        private const int InitializationFrameCount = 2;

        public IEnumerator LoadScene(string sceneName)
        {
            yield return LoadSceneAndWaitFor(
                sceneName,
                sceneName,
                $"loading '{sceneName}' directly");
        }

        public IEnumerator LoadSceneAndWaitFor(
            string sceneName,
            string expectedSceneName,
            string operation)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single);

            Assert.That(
                loadOperation,
                Is.Not.Null,
                $"Unity did not create a load operation for scene '{sceneName}'.");

            yield return loadOperation;
            yield return WaitForActiveScene(expectedSceneName, operation);
        }

        public IEnumerator WaitForActiveScene(
            string expectedSceneName,
            string operation,
            float timeoutSeconds = DefaultSceneTimeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;

            while (SceneManager.GetActiveScene().name != expectedSceneName &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            if (SceneManager.GetActiveScene().name != expectedSceneName)
            {
                Assert.Fail(BuildTimeoutDiagnostics(
                    expectedSceneName,
                    operation,
                    timeoutSeconds));
            }

            yield return WaitForInitializationFrames();
        }

        public SceneContext GetActiveSceneContext()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneContext sceneContext = FindInScene<SceneContext>(activeScene);

            Assert.That(
                sceneContext,
                Is.Not.Null,
                $"Scene '{activeScene.name}' has no {nameof(SceneContext)}.");

            return sceneContext;
        }

        public TComponent FindInActiveScene<TComponent>()
            where TComponent : Component
        {
            return FindInScene<TComponent>(SceneManager.GetActiveScene());
        }

        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;

            if (ProjectContext.HasInstance)
                Object.Destroy(ProjectContext.Instance.gameObject);

            yield return null;
        }

        private static IEnumerator WaitForInitializationFrames()
        {
            for (int frameIndex = 0; frameIndex < InitializationFrameCount; frameIndex++)
                yield return null;
        }

        private static TComponent FindInScene<TComponent>(Scene scene)
            where TComponent : Component
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();

            for (int rootIndex = 0; rootIndex < rootObjects.Length; rootIndex++)
            {
                TComponent component = rootObjects[rootIndex]
                    .GetComponentInChildren<TComponent>(true);

                if (component != null)
                    return component;
            }

            return null;
        }

        private static string BuildTimeoutDiagnostics(
            string expectedSceneName,
            string operation,
            float timeoutSeconds)
        {
            StringBuilder message = new();
            message.Append("Timed out after ")
                .Append(timeoutSeconds)
                .Append(" seconds while ")
                .Append(operation)
                .Append(". Expected active scene '")
                .Append(expectedSceneName)
                .Append("', actual '")
                .Append(SceneManager.GetActiveScene().name)
                .Append("'. Loaded scenes: ");

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                if (sceneIndex > 0)
                    message.Append(", ");

                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                message.Append('[')
                    .Append(sceneIndex)
                    .Append("] ")
                    .Append(scene.name)
                    .Append(" (loaded=")
                    .Append(scene.isLoaded)
                    .Append(')');
            }

            message.Append(". ProjectContext.HasInstance=")
                .Append(ProjectContext.HasInstance)
                .Append(", timeScale=")
                .Append(Time.timeScale)
                .Append('.');
            return message.ToString();
        }
    }
}
