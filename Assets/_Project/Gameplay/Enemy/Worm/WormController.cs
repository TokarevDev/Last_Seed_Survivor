using System;
using System.Collections.Generic;
using Game.Core.Collections;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Enemy.Worm.Presentation;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public sealed class WormController : MonoBehaviour, IWormPathProgressProvider
    {
        [Header("Rail")]
        [SerializeField] private RailPath _rail;
        [SerializeField] private WormMovementConfig _movementConfig;

        private WormLifecycleController _lifecycleController;
        private WormFrameSimulation _frameSimulation;
        private WormRailTargetResolver _railTargetResolver;
        private WormPathProgressState _pathProgress;
        private WormSegmentChainPresenter _segmentChainPresenter;
        private WormReviveSequence _reviveSequence;
        private OrderedReferenceSet<WormSegment> _segmentChain;
        private WormSectionRollbackState<WormSegment> _sectionRollbackState;
        private WormMovementRuntimeConfig _runtimeMovementConfig;
        private WormPresentationConfig _presentationConfig;
        private WormReviveConfig _reviveConfig;
        private float _waveTime;

        public bool HasWorm => _segmentChain != null && _segmentChain.Count > 0;
        public bool IsCatchingUpToCombatStart =>
            _pathProgress != null && _pathProgress.IsCatchingUp;
        public bool IsCombatBurstActive =>
            _lifecycleController != null && _lifecycleController.IsCombatBurstActive;

        public void Configure(
            WormLifecycleController lifecycleController,
            WormFrameSimulation frameSimulation,
            WormRailTargetResolver railTargetResolver,
            WormPathProgressState pathProgress,
            WormSegmentChainPresenter segmentChainPresenter,
            WormReviveSequence reviveSequence,
            OrderedReferenceSet<WormSegment> segmentChain,
            WormSectionRollbackState<WormSegment> sectionRollbackState)
        {
            _lifecycleController = lifecycleController;
            _frameSimulation = frameSimulation;
            _railTargetResolver = railTargetResolver;
            _pathProgress = pathProgress;
            _segmentChainPresenter = segmentChainPresenter;
            _reviveSequence = reviveSequence;
            _segmentChain = segmentChain;
            _sectionRollbackState = sectionRollbackState;

            if (_rail == null)
                throw new InvalidOperationException($"{nameof(WormController)} on '{name}' requires a rail path.");

            if (_movementConfig == null)
                throw new InvalidOperationException($"{nameof(WormController)} on '{name}' requires a movement config.");

            _runtimeMovementConfig = _movementConfig.CreateRuntimeMovementConfig();
            _presentationConfig = _movementConfig.CreatePresentationConfig();
            _reviveConfig = _movementConfig.CreateReviveConfig();
        }

        public float HeadPathProgressNormalized
        {
            get
            {
                if (_rail == null || _rail.TotalLength <= 0f)
                    return 0f;

                return Mathf.Clamp01(_pathProgress.HeadDistance / _rail.TotalLength);
            }
        }

        public float HeadControlPointProgressNormalized
        {
            get
            {
                if (_rail == null || _rail.PointCount <= 1)
                    return HeadPathProgressNormalized;

                return _rail.GetControlPointProgressNormalized(_pathProgress.HeadDistance);
            }
        }

        private void OnValidate()
        {
            if (_rail == null)
                Debug.LogError($"{nameof(WormController)} requires a rail path.", this);

            if (_movementConfig == null)
                Debug.LogError($"{nameof(WormController)} requires a movement config.", this);

            ClearTargetDistanceCaches();
        }

        private void OnDestroy()
        {
            _lifecycleController?.CancelPendingOperations();
        }

        public void Init(List<WormSegment> segments)
        {
            _lifecycleController.Initialize(
                segments,
                _runtimeMovementConfig.BaseSpeed,
                TryGetCatchUpTargetDistance(out _));

            UpdateSegments();
        }

        public void ClearWorm()
        {
            _lifecycleController.Clear(_runtimeMovementConfig.BaseSpeed);
        }

        public WormFrameResult Tick(
            float deltaTime,
            float unscaledDeltaTime,
            float time,
            float unscaledTime)
        {
            _waveTime = (_sectionRollbackState.IsActive || _reviveSequence.IsActive
                ? unscaledTime
                : time) * _presentationConfig.WaveSpeed;
            WormFrameContext context = new(
                _rail,
                _runtimeMovementConfig.ForwardMotion,
                BuildSegmentLayout(),
                _runtimeMovementConfig.BaseSpeed,
                _runtimeMovementConfig.SectionRollbackForwardSpeedMultiplier,
                _runtimeMovementConfig.RollbackSpeed,
                deltaTime,
                unscaledDeltaTime);

            bool pathCompleted = _frameSimulation.Tick(context);
            WormPathCompletion? completion = pathCompleted
                ? new WormPathCompletion(
                    _pathProgress.HeadDistance,
                    HeadPathProgressNormalized,
                    WormPathCompletionReason.ReachedRailEnd)
                : null;
            return new WormFrameResult(completion);
        }

        private bool TryGetCatchUpTargetDistance(out float targetDistance)
        {
            return _railTargetResolver.TryGetCatchUpDistance(
                _rail,
                _runtimeMovementConfig.ForwardMotion.CatchUpRailPointIndex,
                out targetDistance);
        }

        private bool TryGetReviveRollbackTargetDistance(out float targetDistance)
        {
            return _railTargetResolver.TryGetReviveDistance(
                _rail,
                _reviveConfig.RollbackRailPointIndex,
                _runtimeMovementConfig.ForwardMotion.CatchUpRailPointIndex,
                out targetDistance);
        }

        private void ClearTargetDistanceCaches()
        {
            _railTargetResolver?.Clear();
        }

        private void UpdateSegments()
        {
            _frameSimulation.Render(_rail, BuildSegmentLayout());
        }

        private WormSegmentChainLayout BuildSegmentLayout()
        {
            return _presentationConfig.CreateSegmentLayout(
                _pathProgress.HeadDistance,
                _waveTime,
                _reviveSequence.VisualYOffset,
                _sectionRollbackState.IsActive,
                _reviveSequence.IsActive);
        }

        public int RemoveDestroyedSectionSegments(List<WormSegment> destroyed, out int firstRemovedIndex)
        {
            firstRemovedIndex = -1;

            if (destroyed == null || destroyed.Count == 0)
                return 0;

            int removed = _segmentChain.RemoveAll(destroyed, out firstRemovedIndex);
            _sectionRollbackState.Forget(destroyed);
            return removed;
        }

        public void RollbackDestroyedGap(int destroyedCount, int splitIndex)
        {
            if (destroyedCount <= 0)
                return;

            if (splitIndex < 0)
                return;

            if (_reviveSequence.IsActive)
                return;

            _sectionRollbackState.BeginOrExtend(
                _segmentChain.Items,
                splitIndex,
                destroyedCount,
                _pathProgress.HeadDistance,
                _presentationConfig.SegmentSpacing);
            _segmentChainPresenter.Reset();
        }

        public bool RollbackToReviveStart(Action onComplete)
        {
            if (_segmentChain.Count == 0 || _rail == null)
                return false;

            float target = GetReviveRollbackTargetDistance();

            _reviveSequence.Cancel();

            ClearSectionRollbackState();
            _pathProgress.ReopenPath();

            if (_pathProgress.HeadDistance <= target)
            {
                _pathProgress.SetHeadDistance(target);
                UpdateSegments();
                onComplete?.Invoke();
                return true;
            }

            _segmentChainPresenter.Reset();
            _reviveSequence.Begin(
                _pathProgress.HeadDistance,
                target,
                _reviveConfig.AnimationSettings,
                _segmentChain.Items,
                onComplete);
            return true;
        }

        private float GetReviveRollbackTargetDistance()
        {
            if (_rail == null)
                return 0f;

            return TryGetReviveRollbackTargetDistance(out float targetDistance)
                ? Mathf.Clamp(targetDistance, 0f, _rail.TotalLength)
                : 0f;
        }

        private void ClearSectionRollbackState()
        {
            _segmentChainPresenter.Reset();
            _sectionRollbackState.Complete();
        }
    }

}
