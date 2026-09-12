using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Presentation.Validation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.EditorTools.Validation
{
    public static class ProjectViewReferenceValidationService
    {
        private const string ProjectAssetRoot = "Assets/_Project";

        public static void ValidateAllEnabledBuildScenesAndPrefabs()
        {
            List<string> errors = new();
            int validatedReferenceCount = 0;
            int validatedSceneCount = 0;

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int index = 0; index < buildScenes.Length; index++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[index];

                if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                    continue;

                ValidateScene(
                    buildScene.path,
                    errors,
                    ref validatedReferenceCount);
                validatedSceneCount++;
            }

            int validatedPrefabCount = ValidateProjectPrefabs(
                errors,
                ref validatedReferenceCount);
            ThrowIfInvalid(errors);
            Debug.Log(
                $"Last Seed required View reference validation succeeded for " +
                $"{validatedReferenceCount} fields in {validatedSceneCount} enabled scenes " +
                $"and {validatedPrefabCount} prefabs.");
        }

        public static void ValidateCurrentSceneAndProjectPrefabs()
        {
            List<string> errors = new();
            int validatedReferenceCount = 0;
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.IsValid() && activeScene.isLoaded)
            {
                AppendSceneErrors(
                    activeScene,
                    activeScene.path,
                    errors,
                    ref validatedReferenceCount);
            }

            ValidateProjectPrefabs(errors, ref validatedReferenceCount);
            ThrowIfInvalid(errors);
        }

        public static int AppendHierarchyErrors(
            GameObject root,
            string context,
            ICollection<string> errors)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            int validatedReferenceCount = 0;
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            for (int index = 0; index < behaviours.Length; index++)
            {
                MonoBehaviour behaviour = behaviours[index];

                if (behaviour == null)
                    continue;

                AppendComponentErrors(
                    behaviour,
                    context,
                    errors,
                    ref validatedReferenceCount);
            }

            return validatedReferenceCount;
        }

        private static void ValidateScene(
            string scenePath,
            ICollection<string> errors,
            ref int validatedReferenceCount)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;

            if (openedForValidation)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                AppendSceneErrors(
                    scene,
                    scenePath,
                    errors,
                    ref validatedReferenceCount);
            }
            finally
            {
                if (openedForValidation && scene.IsValid())
                    EditorSceneManager.CloseScene(scene, removeScene: true);
            }
        }

        private static void AppendSceneErrors(
            Scene scene,
            string context,
            ICollection<string> errors,
            ref int validatedReferenceCount)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int index = 0; index < roots.Length; index++)
            {
                validatedReferenceCount += AppendHierarchyErrors(
                    roots[index],
                    context,
                    errors);
            }
        }

        private static int ValidateProjectPrefabs(
            ICollection<string> errors,
            ref int validatedReferenceCount)
        {
            string[] prefabGuids = AssetDatabase.FindAssets(
                "t:Prefab",
                new[] { ProjectAssetRoot });

            for (int index = 0; index < prefabGuids.Length; index++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[index]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null)
                {
                    errors.Add($"Could not load prefab at '{prefabPath}'.");
                    continue;
                }

                validatedReferenceCount += AppendHierarchyErrors(
                    prefab,
                    prefabPath,
                    errors);
            }

            return prefabGuids.Length;
        }

        private static void AppendComponentErrors(
            MonoBehaviour behaviour,
            string context,
            ICollection<string> errors,
            ref int validatedReferenceCount)
        {
            Type type = behaviour.GetType();

            while (type != null && type != typeof(MonoBehaviour))
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);

                for (int index = 0; index < fields.Length; index++)
                {
                    FieldInfo field = fields[index];

                    if (!field.IsDefined(typeof(RequiredViewReferenceAttribute), inherit: true))
                        continue;

                    validatedReferenceCount++;
                    ValidateRequiredField(
                        behaviour,
                        field,
                        BuildFieldContext(behaviour, field, context),
                        errors);
                }

                type = type.BaseType;
            }
        }

        private static void ValidateRequiredField(
            MonoBehaviour behaviour,
            FieldInfo field,
            string context,
            ICollection<string> errors)
        {
            bool isSerialized = field.IsPublic ||
                                field.IsDefined(typeof(SerializeField), inherit: true);

            if (!isSerialized)
            {
                errors.Add($"{context} is marked required but is not serialized.");
                return;
            }

            object value = field.GetValue(behaviour);

            if (typeof(Object).IsAssignableFrom(field.FieldType))
            {
                if ((Object)value == null)
                    errors.Add($"{context} is not assigned.");

                return;
            }

            if (value is IList list)
            {
                if (list.Count == 0)
                {
                    errors.Add($"{context} collection is empty.");
                    return;
                }

                for (int index = 0; index < list.Count; index++)
                {
                    if (list[index] is Object item && item != null)
                        continue;

                    errors.Add($"{context}[{index}] is not assigned.");
                }

                return;
            }

            if (value == null && typeof(IList).IsAssignableFrom(field.FieldType))
            {
                errors.Add($"{context} collection is null.");
                return;
            }

            errors.Add($"{context} has unsupported required-reference type '{field.FieldType.Name}'.");
        }

        private static string BuildFieldContext(
            Component component,
            FieldInfo field,
            string assetContext)
        {
            return $"{assetContext}: {BuildHierarchyPath(component.transform)}/" +
                   $"{component.GetType().Name}.{field.Name}";
        }

        private static string BuildHierarchyPath(Transform target)
        {
            List<string> names = new();

            while (target != null)
            {
                names.Add(target.name);
                target = target.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static void ThrowIfInvalid(IReadOnlyCollection<string> errors)
        {
            if (errors.Count == 0)
                return;

            throw new InvalidOperationException(
                "Last Seed required View reference validation failed:\n- " +
                string.Join("\n- ", errors));
        }
    }
}
