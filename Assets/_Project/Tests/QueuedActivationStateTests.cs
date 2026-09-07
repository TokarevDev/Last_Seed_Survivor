using System;
using LastSeed.Core.Collections;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class QueuedActivationStateTests
    {
        [Test]
        public void Request_ActivatesFirstItemAndDeduplicatesFollowingRequests()
        {
            object active = new();
            object queued = new();
            QueuedActivationState<object> state = new();

            Assert.That(state.Request(active), Is.EqualTo(ActivationRequestResult.Activated));
            Assert.That(state.Request(active), Is.EqualTo(ActivationRequestResult.AlreadyActive));
            Assert.That(state.Request(queued), Is.EqualTo(ActivationRequestResult.Queued));
            Assert.That(state.Request(queued), Is.EqualTo(ActivationRequestResult.AlreadyQueued));
            Assert.That(state.ActiveItem, Is.SameAs(active));
            Assert.That(state.QueuedCount, Is.EqualTo(1));
        }

        [Test]
        public void Deactivate_WithContinuation_ActivatesQueuedItemsInFifoOrder()
        {
            object first = new();
            object second = new();
            object third = new();
            QueuedActivationState<object> state = new();
            state.Request(first);
            state.Request(second);
            state.Request(third);

            Assert.That(state.Deactivate(out object deactivated), Is.True);
            Assert.That(state.TryActivateNext(out object activated), Is.True);
            Assert.That(deactivated, Is.SameAs(first));
            Assert.That(activated, Is.SameAs(second));
            Assert.That(state.ActiveItem, Is.SameAs(second));

            state.Deactivate(out deactivated);
            state.TryActivateNext(out activated);

            Assert.That(deactivated, Is.SameAs(second));
            Assert.That(activated, Is.SameAs(third));
            Assert.That(state.QueuedCount, Is.Zero);
        }

        [Test]
        public void RemoveQueued_SkipsRemovedItemWithoutChangingRemainingOrder()
        {
            object active = new();
            object removed = new();
            object next = new();
            QueuedActivationState<object> state = new();
            state.Request(active);
            state.Request(removed);
            state.Request(next);

            Assert.That(state.RemoveQueued(removed), Is.True);
            state.Deactivate(out _);
            state.TryActivateNext(out object activated);

            Assert.That(activated, Is.SameAs(next));
            Assert.That(state.QueuedCount, Is.Zero);
        }

        [Test]
        public void Clear_ReturnsActiveAndRemovesAllState()
        {
            object active = new();
            QueuedActivationState<object> state = new();
            state.Request(active);
            state.Request(new object());

            object clearedActive = state.Clear();

            Assert.That(clearedActive, Is.SameAs(active));
            Assert.That(state.ActiveItem, Is.Null);
            Assert.That(state.QueuedCount, Is.Zero);
            Assert.That(state.Deactivate(out _), Is.False);
        }

        [Test]
        public void Request_Null_ThrowsWithoutChangingState()
        {
            QueuedActivationState<object> state = new();

            Assert.Throws<ArgumentNullException>(() => state.Request(null));
            Assert.That(state.ActiveItem, Is.Null);
            Assert.That(state.QueuedCount, Is.Zero);
        }
    }
}
