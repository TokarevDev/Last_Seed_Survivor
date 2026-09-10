using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using Game.EditorTools.Worm;
using Game.Gameplay.Enemy.Worm;

namespace Game.Tests
{
    public sealed class RailPathSerializedDataTests
    {
        [Test]
        public void PointEditing_UsesWorldSpaceBoundaryAndPersistsSerializedValues()
        {
            GameObject pathObject = new("RailPath");

            try
            {
                pathObject.transform.position = new Vector3(5f, 2f, 0f);
                RailPath path = pathObject.AddComponent<RailPath>();
                SerializedObject serializedPath = new(path);
                RailPathSerializedData data = new(path, serializedPath);

                data.ClearPoints();
                data.AddWorldPoint(new Vector3(6f, 3f, 4f));
                data.InsertWorldPoint(0, new Vector3(5f, 2f, 2f));
                data.FlattenLocalZ();

                Assert.That(data.PointCount, Is.EqualTo(2));
                Assert.That(data.GetWorldPoint(0), Is.EqualTo(new Vector3(5f, 2f, 0f)));
                Assert.That(data.GetWorldPoint(1), Is.EqualTo(new Vector3(6f, 3f, 0f)));

                data.ReversePoints();

                Assert.That(data.GetWorldPoint(0), Is.EqualTo(new Vector3(6f, 3f, 0f)));
                Assert.That(path.PointCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(pathObject);
            }
        }

        [Test]
        public void ImportChildTransforms_ReplacesPointsAndBuildsPreview()
        {
            GameObject pathObject = new("RailPath");

            try
            {
                RailPath path = pathObject.AddComponent<RailPath>();
                CreateChild(pathObject.transform, "Start", new Vector3(-2f, 0f, 0f));
                CreateChild(pathObject.transform, "End", new Vector3(3f, 1f, 0f));
                RailPathSerializedData data = new(path, new SerializedObject(path));

                int importedCount = data.ImportChildTransforms();
                Vector3[] preview = data.BuildPreviewWorldPoints();

                Assert.That(importedCount, Is.EqualTo(2));
                Assert.That(data.PointCount, Is.EqualTo(2));
                Assert.That(preview, Has.Length.EqualTo(2));
                Assert.That(preview[0], Is.EqualTo(new Vector3(-2f, 0f, 0f)));
                Assert.That(preview[1], Is.EqualTo(new Vector3(3f, 1f, 0f)));
            }
            finally
            {
                Object.DestroyImmediate(pathObject);
            }
        }

        private static void CreateChild(
            Transform parent,
            string name,
            Vector3 worldPosition)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent);
            child.transform.position = worldPosition;
        }
    }
}
