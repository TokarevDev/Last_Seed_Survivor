using System;
using System.Collections.Generic;
using Game.Gameplay.Enemy.Worm.Combat;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public sealed class WormFactory
    {
        private readonly WormSegmentPool _pool;

        private const int BaseSortOrder = 2500;

        public WormFactory(WormSegmentPool pool)
        {
            _pool = pool;
        }

        public List<WormSegment> CreateSegments(
            List<WormPatternEntry> pattern,
            out WormSegment head,
            out WormSegment tail)
        {
            if (pattern == null)
                throw new System.ArgumentNullException(nameof(pattern));

            List<WormSegment> segments = new(pattern.Count);

            head = null;
            tail = null;

            try
            {
                for (int i = 0; i < pattern.Count; i++)
                {
                    WormPatternEntry entry = pattern[i];

                    WormSegment segment = _pool.Get(entry.Type);

                    if (segment == null)
                        throw new InvalidOperationException(
                            $"Failed to rent worm segment of type {entry.Type}.");

                    segments.Add(segment);
                    segment.PrepareForWorm();
                    segment.Index = i;

                    int order = entry.Type == WormSegmentType.Head
                        ? BaseSortOrder
                        : Mathf.Max(1, BaseSortOrder - i);

                    segment.SetSortingOrder(order);

                    if (entry.Type == WormSegmentType.Head)
                        head = segment;

                    if (entry.Type == WormSegmentType.Tail)
                        tail = segment;

                }

                if (head == null || tail == null)
                    throw new InvalidOperationException(
                        "Worm pattern must create both a head and a tail.");

                return segments;
            }
            catch (Exception creationException)
            {
                List<Exception> rollbackFailures = null;

                for (int segmentIndex = segments.Count - 1; segmentIndex >= 0; segmentIndex--)
                {
                    try
                    {
                        _pool.Release(segments[segmentIndex]);
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackFailures ??= new List<Exception>();
                        rollbackFailures.Add(rollbackException);
                    }
                }

                head = null;
                tail = null;

                if (rollbackFailures != null)
                {
                    rollbackFailures.Insert(0, creationException);
                    throw new AggregateException(
                        "Worm segment creation and rollback both failed.",
                        rollbackFailures);
                }

                throw;
            }
        }

        public void AttachDamageReceivers(
            List<WormSegment> segments,
            WormCombatController combat)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                WormSegment segment = segments[i];
                segment.BindDamageReceivers(combat);
            }
        }
    }

}
