using System;
using System.Collections;
using System.Reflection;
using Game.Core.Pooling;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Presentation.Worm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests.PlayMode
{
    public sealed class LifecycleOwnershipRegressionTests
    {
        [UnityTest]
        public IEnumerator WormProgress_WhenReenabled_RendersLatestSnapshot()
        {
            GameObject owner = new("WormProgress", typeof(RectTransform));
            owner.SetActive(false);
            Type textType = Type.GetType(
                "TMPro.TextMeshProUGUI, Unity.TextMeshPro",
                throwOnError: true);
            Component text = owner.AddComponent(textType);
            WormProgressPresenter presenter = owner.AddComponent<WormProgressPresenter>();
            MutableProgressSnapshotProvider snapshotProvider = new(5, 10);
            presenter.Construct(null, snapshotProvider);

            owner.SetActive(true);
            yield return null;
            Assert.That(GetText(text), Is.EqualTo("Progress: 50%"));

            owner.SetActive(false);
            snapshotProvider.Set(7, 10);
            owner.SetActive(true);
            yield return null;

            Assert.That(GetText(text), Is.EqualTo("Progress: 70%"));
            UnityEngine.Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SegmentReturn_WhenCleanupFails_LeavesNoActiveOrReusableOrphan()
        {
            GameObject owner = new("Segment");
            GameObject cocoon = new("Cocoon");
            cocoon.transform.SetParent(owner.transform);
            WormSegment segment = owner.AddComponent<WormSegment>();
            ThrowOnceShakeClock shakeClock = new();
            WormSegmentCocoonPresenter presenter = new(
                owner.transform,
                cocoon,
                3f,
                10f);
            presenter.BindShakeClock(shakeClock, true);
            SetField(segment, "_cocoonPresenter", presenter);
            SetField(
                segment,
                "_pooledViewLifecycle",
                new WormSegmentPooledViewLifecycle(owner, null, null));
            Action<WormSegment> cleanup =
                (Action<WormSegment>)Delegate.CreateDelegate(
                    typeof(Action<WormSegment>),
                    typeof(WormSegmentPool).GetMethod(
                        "PrepareForPool",
                        BindingFlags.Static | BindingFlags.NonPublic));
            ObjectPool<WormSegment> pool = new(() => segment, cleanup);

            pool.Rent();
            presenter.Show(null, true);

            Assert.Throws<InvalidOperationException>(() => pool.Return(segment));
            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.That(pool.AvailableCount, Is.Zero);
            Assert.That(owner.activeSelf, Is.False);

            presenter.Hide();
            UnityEngine.Object.Destroy(owner);
            yield return null;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }

        private static string GetText(Component text)
        {
            return (string)text.GetType().GetProperty("text")?.GetValue(text);
        }

        private sealed class MutableProgressSnapshotProvider :
            IWormDestructionProgressSnapshotProvider
        {
            public MutableProgressSnapshotProvider(int destroyed, int total)
            {
                Set(destroyed, total);
            }

            public WormDestructionProgressSnapshot CurrentProgress { get; private set; }

            public void Set(int destroyed, int total)
            {
                CurrentProgress = new WormDestructionProgressSnapshot(destroyed, total);
            }
        }

        private sealed class ThrowOnceShakeClock : IWormCocoonShakeClock
        {
            private bool _hasThrown;

            public float RotationOffset => 0f;

            public void Register(float interval, float angle)
            {
            }

            public void Unregister()
            {
                if (_hasThrown)
                    return;

                _hasThrown = true;
                throw new InvalidOperationException("Cocoon cleanup failed.");
            }
        }
    }
}
