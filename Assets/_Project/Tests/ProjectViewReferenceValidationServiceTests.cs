using System.Collections.Generic;
using Game.EditorTools.Validation;
using Game.Presentation.Validation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ProjectViewReferenceValidationServiceTests
    {
        private GameObject _root;
        private RequiredViewReferenceTestComponent _component;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("RequiredReferenceTestRoot");
            _component = _root.AddComponent<RequiredViewReferenceTestComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void AppendHierarchyErrors_ReportsMissingReferenceAndEmptyCollection()
        {
            List<string> errors = new();

            int validatedCount = ProjectViewReferenceValidationService.AppendHierarchyErrors(
                _root,
                "test prefab",
                errors);

            Assert.That(validatedCount, Is.EqualTo(2));
            Assert.That(errors, Has.Some.Contains("_target is not assigned"));
            Assert.That(errors, Has.Some.Contains("_items collection is empty"));
        }

        [Test]
        public void AppendHierarchyErrors_ReportsNullCollectionElement()
        {
            _component.Configure(_root.transform, new List<GameObject> { _root, null });
            List<string> errors = new();

            ProjectViewReferenceValidationService.AppendHierarchyErrors(
                _root,
                "test prefab",
                errors);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors[0], Does.Contain("_items[1] is not assigned"));
        }

        [Test]
        public void AppendHierarchyErrors_AcceptsCompleteReferences()
        {
            _component.Configure(_root.transform, new List<GameObject> { _root });
            List<string> errors = new();

            int validatedCount = ProjectViewReferenceValidationService.AppendHierarchyErrors(
                _root,
                "test prefab",
                errors);

            Assert.That(validatedCount, Is.EqualTo(2));
            Assert.That(errors, Is.Empty);
        }
    }

    public sealed class RequiredViewReferenceTestComponent : MonoBehaviour
    {
        [SerializeField, RequiredViewReference] private Transform _target;
        [SerializeField, RequiredViewReference] private List<GameObject> _items = new();

        public void Configure(Transform target, List<GameObject> items)
        {
            _target = target;
            _items = items;
        }
    }
}
