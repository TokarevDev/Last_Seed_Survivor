using System;
using Game.Gameplay.Enemy.Worm;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Spawning
{
    public sealed class WormSegmentPoolSettings
    {
        public WormSegmentPoolSettings(
            Transform parent,
            WormSegment headPrefab,
            WormSegment bodyPrefab,
            WormSegment tailPrefab)
        {
            Parent = parent != null ? parent : throw new ArgumentNullException(nameof(parent));
            HeadPrefab = ValidatePrefab(headPrefab, WormSegmentType.Head, nameof(headPrefab));
            BodyPrefab = ValidatePrefab(bodyPrefab, WormSegmentType.Body, nameof(bodyPrefab));
            TailPrefab = ValidatePrefab(tailPrefab, WormSegmentType.Tail, nameof(tailPrefab));
        }

        public Transform Parent { get; }
        public WormSegment HeadPrefab { get; }
        public WormSegment BodyPrefab { get; }
        public WormSegment TailPrefab { get; }

        private static WormSegment ValidatePrefab(
            WormSegment prefab,
            WormSegmentType expectedType,
            string parameterName)
        {
            if (prefab == null)
                throw new ArgumentNullException(parameterName);

            if (prefab.Type != expectedType)
            {
                throw new ArgumentException(
                    $"Worm segment prefab '{prefab.name}' must have type {expectedType}, " +
                    $"but has {prefab.Type}.",
                    parameterName);
            }

            return prefab;
        }
    }

}
