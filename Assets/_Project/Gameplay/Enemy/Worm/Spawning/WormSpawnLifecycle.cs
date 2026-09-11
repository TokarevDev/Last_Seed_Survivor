using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Randomization;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Gameplay.Rewards.Data;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Spawning
{
    public sealed class WormSpawnLifecycle
    {
        private readonly WormSegmentPool _segmentPool;
        private readonly WormFactory _wormFactory;
        private readonly WormSpawnSettings _settings;
        private readonly WormAdaptiveHpController _adaptiveHpController;
        private readonly WormController _wormController;
        private readonly WormCombatController _wormCombat;
        private readonly IWormSectionHealthPresentation _hpPresentation;
        private readonly IWormFaceBurstPresentation _faceBurstPresenter;
        private readonly IRandomSource _randomSource;
        private readonly List<WormSegment> _activeSegments = new();
        private readonly List<WormSection> _activeSections = new();

        private WormSegment _head;

        private enum SpawnStage
        {
            None,
            SegmentViews,
            SectionModels,
            DamageReceivers,
            WormController,
            FacePresentation,
            Combat,
            HealthPresentation
        }

        public WormSpawnLifecycle(
            WormSegmentPool segmentPool,
            WormFactory wormFactory,
            WormSpawnSettings settings,
            WormAdaptiveHpController adaptiveHpController,
            WormController wormController,
            WormCombatController wormCombat,
            IWormSectionHealthPresentation hpPresentation,
            IWormFaceBurstPresentation faceBurstPresenter,
            IRandomSource randomSource)
        {
            _segmentPool = segmentPool ?? throw new ArgumentNullException(nameof(segmentPool));
            _wormFactory = wormFactory ?? throw new ArgumentNullException(nameof(wormFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _adaptiveHpController = adaptiveHpController ??
                throw new ArgumentNullException(nameof(adaptiveHpController));
            _wormController = wormController ?? throw new ArgumentNullException(nameof(wormController));
            _wormCombat = wormCombat ?? throw new ArgumentNullException(nameof(wormCombat));
            _hpPresentation = hpPresentation ??
                throw new ArgumentNullException(nameof(hpPresentation));
            _faceBurstPresenter = faceBurstPresenter ??
                throw new ArgumentNullException(nameof(faceBurstPresenter));
            _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
        }

        public bool IsSpawned { get; private set; }

        public UniTask PrewarmAsync(CancellationToken cancellationToken)
        {
            return _segmentPool.PrewarmAsync(
                _settings.BodyPoolCapacity,
                _settings.PrewarmBatchSize,
                cancellationToken);
        }

        public void Spawn(
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles,
            float currentTime)
        {
            if (IsSpawned)
                return;

            List<WormSegment> segments = null;
            List<WormSection> sections = null;
            WormSegment head = null;
            WormSegment tail = null;
            SpawnStage stage = SpawnStage.None;

            try
            {
                // Mark a stage before entry so rollback covers partial side effects.
                stage = SpawnStage.SegmentViews;
                segments = CreateSegmentViews(out head, out tail);

                stage = SpawnStage.SectionModels;
                sections = BuildSectionModels(segments, cocoonProfiles, currentTime);

                stage = SpawnStage.DamageReceivers;
                _wormFactory.AttachDamageReceivers(segments, _wormCombat);

                stage = SpawnStage.WormController;
                _wormController.Init(segments);

                stage = SpawnStage.FacePresentation;
                _faceBurstPresenter.Bind(head.FaceVisual);

                stage = SpawnStage.Combat;
                _wormCombat.Init(head, tail, sections);

                stage = SpawnStage.HealthPresentation;
                _hpPresentation.BindSections(sections);

                CommitSpawn(segments, sections, head);
            }
            catch (Exception spawnException)
            {
                List<Exception> rollbackFailures =
                    RollbackFailedSpawn(segments, currentTime, stage);

                if (rollbackFailures != null)
                {
                    rollbackFailures.Insert(0, spawnException);
                    throw new AggregateException(
                        "Worm spawn and rollback both failed.",
                        rollbackFailures);
                }

                throw;
            }
        }

        public void Despawn(float currentTime)
        {
            List<Exception> failures = null;

            RollbackGameplayAndPresentation(
                SpawnStage.HealthPresentation,
                ref failures);
            ReleaseSegmentsForRollback(_activeSegments, ref failures);
            _activeSegments.Clear();
            _activeSections.Clear();
            ResetAdaptiveHpForRollback(currentTime, ref failures);
            _head = null;
            IsSpawned = false;

            if (failures != null)
                throw new AggregateException("Worm despawn cleanup failed.", failures);
        }

        public void RebindFacePresentation()
        {
            if (IsSpawned)
                _faceBurstPresenter.Bind(_head?.FaceVisual);
        }

        public void UnbindFacePresentation()
        {
            _faceBurstPresenter.Unbind();
        }

        private List<WormSegment> CreateSegmentViews(
            out WormSegment head,
            out WormSegment tail)
        {
            List<WormPatternEntry> pattern =
                WormPatternBuilder.BuildPattern(_settings.SectionCount, _randomSource);

            return _wormFactory.CreateSegments(pattern, out head, out tail);
        }

        private List<WormSection> BuildSectionModels(
            List<WormSegment> segments,
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles,
            float currentTime)
        {
            List<WormSection> sections =
                WormSectionBuilder.BuildSections(segments, _randomSource, cocoonProfiles);

            _adaptiveHpController.InitializeSections(sections, currentTime);
            return sections;
        }

        private void CommitSpawn(
            List<WormSegment> segments,
            List<WormSection> sections,
            WormSegment head)
        {
            _activeSegments.AddRange(segments);
            _activeSections.AddRange(sections);
            _head = head;
            IsSpawned = true;
        }

        private List<Exception> RollbackFailedSpawn(
            List<WormSegment> rentedSegments,
            float currentTime,
            SpawnStage stage)
        {
            List<Exception> failures = null;

            RollbackGameplayAndPresentation(stage, ref failures);
            ReleaseSegmentsForRollback(rentedSegments, ref failures);

            if (stage >= SpawnStage.SectionModels)
                ResetAdaptiveHpForRollback(currentTime, ref failures);

            _head = null;
            IsSpawned = false;
            return failures;
        }

        private void ReleaseSegmentsForRollback(
            IReadOnlyList<WormSegment> segments,
            ref List<Exception> failures)
        {
            if (segments == null)
                return;

            for (int index = segments.Count - 1; index >= 0; index--)
            {
                try
                {
                    _segmentPool.Release(segments[index]);
                }
                catch (Exception exception)
                {
                    AddRollbackFailure(exception, ref failures);
                }
            }
        }

        private void RollbackGameplayAndPresentation(
            SpawnStage stage,
            ref List<Exception> failures)
        {
            if (stage >= SpawnStage.HealthPresentation)
            {
                try
                {
                    _hpPresentation.Clear();
                }
                catch (Exception exception)
                {
                    AddRollbackFailure(exception, ref failures);
                }
            }

            if (stage >= SpawnStage.Combat)
            {
                try
                {
                    _wormCombat.Clear();
                }
                catch (Exception exception)
                {
                    AddRollbackFailure(exception, ref failures);
                }
            }

            if (stage >= SpawnStage.FacePresentation)
            {
                try
                {
                    _faceBurstPresenter.Unbind();
                }
                catch (Exception exception)
                {
                    AddRollbackFailure(exception, ref failures);
                }
            }

            if (stage < SpawnStage.WormController)
                return;

            try
            {
                _wormController.ClearWorm();
            }
            catch (Exception exception)
            {
                AddRollbackFailure(exception, ref failures);
            }
        }

        private void ResetAdaptiveHpForRollback(
            float currentTime,
            ref List<Exception> failures)
        {
            try
            {
                _adaptiveHpController.Reset(currentTime);
            }
            catch (Exception exception)
            {
                AddRollbackFailure(exception, ref failures);
            }
        }

        private static void AddRollbackFailure(
            Exception exception,
            ref List<Exception> failures)
        {
            failures ??= new List<Exception>();
            failures.Add(exception);
        }
    }

}
