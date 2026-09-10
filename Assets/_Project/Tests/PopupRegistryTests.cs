using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class PopupRegistryTests
    {
        [Test]
        public void Register_DuplicateId_DoesNotRetainItemOrSubscribeToEvents()
        {
            PopupView first = CreatePopup("First");
            PopupView duplicateId = CreatePopup("Duplicate");
            int closeRequestCount = 0;

            try
            {
                PopupRegistry registry = new(_ => closeRequestCount++);

                Assert.That(registry.Register(first), Is.EqualTo(PopupRegistrationResult.Registered));
                Assert.That(registry.Register(first), Is.EqualTo(PopupRegistrationResult.AlreadyRegistered));
                Assert.That(registry.Register(duplicateId), Is.EqualTo(PopupRegistrationResult.DuplicateId));
                duplicateId.RequestClose();

                Assert.That(closeRequestCount, Is.Zero);
                Assert.That(registry.Count, Is.EqualTo(1));
                Assert.That(registry.TryGet(first.PopupId, out PopupView resolved), Is.True);
                Assert.That(resolved, Is.SameAs(first));
                Assert.That(registry.Items[0], Is.SameAs(first));
            }
            finally
            {
                Object.DestroyImmediate(first.gameObject);
                Object.DestroyImmediate(duplicateId.gameObject);
            }
        }

        [Test]
        public void Clear_UnsubscribesAllRegisteredPopupsAndClearsLookup()
        {
            PopupView popup = CreatePopup("Popup");
            int closeRequestCount = 0;

            try
            {
                PopupRegistry registry = new(_ => closeRequestCount++);
                registry.Register(popup);
                popup.RequestClose();

                registry.Clear();
                popup.RequestClose();

                Assert.That(closeRequestCount, Is.EqualTo(1));
                Assert.That(registry.Count, Is.Zero);
                Assert.That(registry.TryGet(popup.PopupId, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(popup.gameObject);
            }
        }

        private static PopupView CreatePopup(string name)
        {
            return new GameObject(name).AddComponent<PopupView>();
        }
    }
}
