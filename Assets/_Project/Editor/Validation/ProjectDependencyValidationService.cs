using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject.Internal;

namespace Game.EditorTools.Validation
{
    public static class ProjectDependencyValidationService
    {
        private const string ValidationMenuPath = "Tools/Last Seed/Validate Dependencies";

        [MenuItem(ValidationMenuPath)]
        public static void ValidateAllEnabledBuildScenes()
        {
            SceneSetup[] originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                int validatedSceneCount = ZenUnityEditorUtil.ValidateAllActiveScenes();
                ProjectAssetValidationService.ValidateAllProjectConfigs();
                Debug.Log(
                    $"Last Seed dependency validation succeeded for " +
                    $"{validatedSceneCount} enabled build scenes.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
            finally
            {
                if (ContainsLoadedScene(originalSceneSetup))
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
            }
        }

        public static bool TryValidateCurrentScene()
        {
            SceneSetup[] currentSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            if (!ContainsLoadedScene(currentSceneSetup) && !TryOpenFirstEnabledBuildScene())
                return false;

            bool validationCompleted = false;
            bool validationExecuted = ZenUnityEditorUtil.SaveThenRunPreserveSceneSetup(() =>
            {
                ZenUnityEditorUtil.ValidateCurrentSceneSetup();
                ProjectAssetValidationService.ValidateAllProjectConfigs();
                validationCompleted = true;
            });

            if (validationExecuted && validationCompleted)
                Debug.Log("Last Seed dependency validation succeeded for the current scene setup.");

            return validationExecuted && validationCompleted;
        }

        private static bool TryOpenFirstEnabledBuildScene()
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int sceneIndex = 0; sceneIndex < buildScenes.Length; sceneIndex++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[sceneIndex];

                if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                    continue;

                EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                Debug.LogWarning(
                    $"No scene was loaded. Opened the first enabled build scene " +
                    $"'{buildScene.path}' before dependency validation.");
                return true;
            }

            Debug.LogError(
                "Play Mode dependency validation requires at least one enabled build scene.");
            return false;
        }

        private static bool ContainsLoadedScene(SceneSetup[] sceneSetup)
        {
            for (int sceneIndex = 0; sceneIndex < sceneSetup.Length; sceneIndex++)
            {
                if (sceneSetup[sceneIndex].isLoaded)
                    return true;
            }

            return false;
        }
    }
}
