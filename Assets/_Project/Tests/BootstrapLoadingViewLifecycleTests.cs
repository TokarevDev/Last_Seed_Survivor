using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Presentation.UI.Loading;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class BootstrapLoadingViewLifecycleTests
    {
        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        private GameObject _viewObject;
        private BootstrapLoadingView _view;

        [SetUp]
        public void SetUp()
        {
            _viewObject = new GameObject("BootstrapLoadingView");
            _view = _viewObject.AddComponent<BootstrapLoadingView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_viewObject);
        }

        [Test]
        public void OnDisable_DuringTransition_CancelsTaskAndReleasesAnimationState()
        {
            using CancellationTokenSource cancellationSource = new();
            UniTask transitionTask = _view.PlayAsync(cancellationSource.Token);

            Assert.That(transitionTask.Status, Is.EqualTo(UniTaskStatus.Pending));
            Assert.That(GetPrivateField<object>("_sequence"), Is.Not.Null);

            InvokeOnDisable();

            Assert.That(transitionTask.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(GetPrivateField<object>("_sequence"), Is.Null);
            Assert.That(GetPrivateField<UniTaskCompletionSource>("_completionSource"), Is.Null);
            Assert.That(
                GetPrivateField<CancellationTokenRegistration>("_cancellationRegistration"),
                Is.EqualTo(default(CancellationTokenRegistration)));
        }

        private void InvokeOnDisable()
        {
            MethodInfo method = typeof(BootstrapLoadingView).GetMethod(
                "OnDisable",
                PrivateInstance);

            Assert.That(method, Is.Not.Null);
            method.Invoke(_view, null);
        }

        private T GetPrivateField<T>(string fieldName)
        {
            FieldInfo field = typeof(BootstrapLoadingView).GetField(
                fieldName,
                PrivateInstance);

            Assert.That(field, Is.Not.Null, $"Missing test field '{fieldName}'.");
            return (T)field.GetValue(_view);
        }
    }
}
