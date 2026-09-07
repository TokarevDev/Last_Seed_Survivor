using System;

namespace LastSeed.Core.Collections
{
    public enum ActivationRequestResult
    {
        Activated,
        Queued,
        AlreadyActive,
        AlreadyQueued
    }

    public sealed class QueuedActivationState<T>
        where T : class
    {
        private readonly UniqueReferenceQueue<T> _queuedItems = new();

        public T ActiveItem { get; private set; }
        public int QueuedCount => _queuedItems.Count;

        public ActivationRequestResult Request(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (ReferenceEquals(ActiveItem, item))
                return ActivationRequestResult.AlreadyActive;

            if (ActiveItem != null)
            {
                return _queuedItems.Enqueue(item)
                    ? ActivationRequestResult.Queued
                    : ActivationRequestResult.AlreadyQueued;
            }

            _queuedItems.Remove(item);
            ActiveItem = item;
            return ActivationRequestResult.Activated;
        }

        public bool Deactivate(out T deactivated)
        {
            deactivated = ActiveItem;

            if (deactivated == null)
                return false;

            ActiveItem = null;
            return true;
        }

        public bool TryActivateNext(out T activated)
        {
            activated = null;

            if (ActiveItem != null || !_queuedItems.TryDequeue(out T next))
                return false;

            ActiveItem = next;
            activated = next;
            return true;
        }

        public bool RemoveQueued(T item)
        {
            return _queuedItems.Remove(item);
        }

        public void ClearQueued()
        {
            _queuedItems.Clear();
        }

        public T Clear()
        {
            T active = ActiveItem;
            ActiveItem = null;
            _queuedItems.Clear();
            return active;
        }
    }
}
