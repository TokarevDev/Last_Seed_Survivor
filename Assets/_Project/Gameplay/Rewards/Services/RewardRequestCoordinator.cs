
using Game.Gameplay.Rewards.Runtime;

namespace Game.Gameplay.Rewards.Services
{
    using System;

    public sealed class RewardRequestCoordinator
    {
        private readonly RewardRequestQueue _queue;
        private readonly RewardRequestLifecycle _lifecycle;

        public RewardRequestCoordinator(
            RewardRequestQueue queue,
            RewardRequestLifecycle lifecycle)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _lifecycle = lifecycle ?? throw new ArgumentNullException(nameof(lifecycle));
        }

        public int PendingCount => _queue.Count;

        public RewardRequestSubmission Submit(in RewardOpenRequest request)
        {
            if (_lifecycle.IsActive)
            {
                _queue.Enqueue(request);
                return RewardRequestSubmission.Queued;
            }

            _lifecycle.Begin(request);
            return RewardRequestSubmission.Begun;
        }

        public bool CompleteActive()
        {
            return _lifecycle.Complete();
        }

        public bool TryBeginNext(out RewardOpenRequest request)
        {
            request = default;

            if (_lifecycle.IsActive || !_queue.TryDequeue(out request))
                return false;

            _lifecycle.Begin(request);
            return true;
        }

        public void ClearPending()
        {
            _queue.Clear();
        }

        public void Reset()
        {
            _queue.Clear();
            _lifecycle.Reset();
        }
    }

}
