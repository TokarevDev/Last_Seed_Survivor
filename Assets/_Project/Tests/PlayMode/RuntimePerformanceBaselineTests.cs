using System;
using System.Collections;
using System.Collections.Generic;
using Game.Bootstrap.GameplayLoop;
using Game.Core.Combat;
using Game.Gameplay.Combat.Weapons.ProjectileWeapon;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Movement;
using Game.Gameplay.Signals;
using Game.Infrastructure.Navigation;
using Game.Presentation.Worm;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace Game.Tests.PlayMode
{
    public sealed class RuntimePerformanceBaselineTests
    {
        private const int WarmupFrameCount = 30;
        private const int SampleFrameCount = 120;
        private const int ForcedWeaponFireInterval = 30;
        private const int PopupReuseIterationCount = 100;
        private const string GcAllocatedInFrameCounterName = "GC Allocated In Frame";

        private readonly PlayModeFlowFixture _flow = new();

        [UnityTest]
        public IEnumerator GameplayHotPaths_EmitMeasuredProfilerBaseline()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);
            yield return WaitFrames(WarmupFrameCount);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            ProjectileWeapon weapon = sceneContext.Container.Resolve<ProjectileWeapon>();
            ProfilerRecorder gameplayFrameRecorder = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts,
                GameplayFrameCoordinator.ProfilerMarkerName,
                SampleFrameCount);
            ProfilerRecorder weaponFireRecorder = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts,
                ProjectileWeapon.FireProfilerMarkerName,
                SampleFrameCount);
            ProfilerRecorder wormFrameRecorder = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts,
                WormFrameSimulation.ProfilerMarkerName,
                SampleFrameCount);
            ProfilerRecorder gcRecorder = ProfilerRecorder.StartNew(
                ProfilerCategory.Memory,
                GcAllocatedInFrameCounterName,
                SampleFrameCount);
            try
            {
                Assert.That(gameplayFrameRecorder.Valid, Is.True);
                Assert.That(weaponFireRecorder.Valid, Is.True);
                Assert.That(wormFrameRecorder.Valid, Is.True);
                Assert.That(gcRecorder.Valid, Is.True);

                for (int frameIndex = 0; frameIndex < SampleFrameCount; frameIndex++)
                {
                    if (frameIndex % ForcedWeaponFireInterval == 0)
                    {
                        weapon.Tick(10f);
                        weapon.ReleasePreparedAttack();
                    }

                    yield return null;
                }

                gameplayFrameRecorder.Stop();
                weaponFireRecorder.Stop();
                wormFrameRecorder.Stop();
                gcRecorder.Stop();

                ProfilerSampleSummary gameplaySummary = Summarize(gameplayFrameRecorder);
                ProfilerSampleSummary weaponSummary = Summarize(weaponFireRecorder);
                ProfilerSampleSummary wormSummary = Summarize(wormFrameRecorder);
                ProfilerSampleSummary gcSummary = Summarize(gcRecorder);

                Assert.That(gameplaySummary.InvocationCount, Is.GreaterThan(0));
                Assert.That(weaponSummary.InvocationCount, Is.GreaterThan(0));
                Assert.That(wormSummary.InvocationCount, Is.GreaterThan(0));

                string baselineMessage =
                    "[PerformanceBaseline] " +
                    $"frames={SampleFrameCount}; " +
                    $"gameplay={gameplaySummary.FormatNanoseconds()}; " +
                    $"weaponFire={weaponSummary.FormatNanoseconds()}; " +
                    $"wormFrame={wormSummary.FormatNanoseconds()}; " +
                    $"editorGcAllocatedPerFrame={gcSummary.FormatBytes()}";
                LogAssert.Expect(LogType.Log, baselineMessage);
                Debug.Log(baselineMessage);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                gameplayFrameRecorder.Dispose();
                weaponFireRecorder.Dispose();
                wormFrameRecorder.Dispose();
                gcRecorder.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DamagePopupReuseAndSceneUnload_DoNotGrowPoolOrRetainSceneObjects()
        {
            yield return _flow.LoadScene(GameSceneNames.Gameplay);

            SceneContext sceneContext = _flow.GetActiveSceneContext();
            SignalBus signalBus = sceneContext.Container.Resolve<SignalBus>();
            WormDamagePopupPresenter presenter =
                _flow.FindInActiveScene<WormDamagePopupPresenter>();
            WormDamagePopupView[] initialPopupViews = FindLoadedSceneObjects<WormDamagePopupView>();
            WormSegment[] initialWormSegments = FindLoadedSceneObjects<WormSegment>();
            Assert.That(presenter, Is.Not.Null);
            Assert.That(initialPopupViews, Is.Not.Empty);
            Assert.That(initialWormSegments, Is.Not.Empty);

            for (int iteration = 0; iteration < PopupReuseIterationCount; iteration++)
            {
                DamageViewRequest request = new(
                    amount: iteration + 1,
                    worldPosition: Vector3.zero,
                    kind: DamageKind.Critical,
                    isCritical: true);
                signalBus.Fire(new WormDamageDealtSignal(request));
                presenter.ClearActivePopups();
            }

            WormDamagePopupView[] popupViewsAfterReuse =
                FindLoadedSceneObjects<WormDamagePopupView>();
            Assert.That(popupViewsAfterReuse.Length, Is.EqualTo(initialPopupViews.Length));

            yield return _flow.LoadScene(GameSceneNames.Lobby);
            yield return Resources.UnloadUnusedAssets();

            int retainedPopupCount = CountAlive(initialPopupViews);
            int retainedSegmentCount = CountAlive(initialWormSegments);

            Assert.That(retainedPopupCount, Is.Zero);
            Assert.That(retainedSegmentCount, Is.Zero);
            Assert.That(FindLoadedSceneObjects<WormDamagePopupView>(), Is.Empty);
            Assert.That(FindLoadedSceneObjects<WormSegment>(), Is.Empty);

            string baselineMessage =
                "[PerformanceBaseline] " +
                $"popupReuseIterations={PopupReuseIterationCount}; " +
                $"popupPoolSize={initialPopupViews.Length}; " +
                $"retainedPopupsAfterUnload={retainedPopupCount}; " +
                $"retainedWormSegmentsAfterUnload={retainedSegmentCount}";
            LogAssert.Expect(LogType.Log, baselineMessage);
            Debug.Log(baselineMessage);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return _flow.TearDown();
        }

        private static IEnumerator WaitFrames(int frameCount)
        {
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                yield return null;
        }

        private static TComponent[] FindLoadedSceneObjects<TComponent>()
            where TComponent : Component
        {
            return UnityEngine.Object.FindObjectsByType<TComponent>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        }

        private static int CountAlive<TComponent>(TComponent[] components)
            where TComponent : Component
        {
            int count = 0;

            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] != null)
                    count++;
            }

            return count;
        }

        private static ProfilerSampleSummary Summarize(ProfilerRecorder recorder)
        {
            List<ProfilerRecorderSample> samples = new(recorder.Capacity);
            recorder.CopyTo(samples);
            long totalValue = 0L;
            long maximumValue = 0L;
            long invocationCount = 0L;

            for (int index = 0; index < samples.Count; index++)
            {
                ProfilerRecorderSample sample = samples[index];
                totalValue += sample.Value;
                maximumValue = Math.Max(maximumValue, sample.Value);
                invocationCount += sample.Count;
            }

            return new ProfilerSampleSummary(
                samples.Count,
                invocationCount,
                totalValue,
                maximumValue);
        }

        private readonly struct ProfilerSampleSummary
        {
            public ProfilerSampleSummary(
                int sampleCount,
                long invocationCount,
                long totalValue,
                long maximumValue)
            {
                SampleCount = sampleCount;
                InvocationCount = invocationCount;
                TotalValue = totalValue;
                MaximumValue = maximumValue;
            }

            public int SampleCount { get; }
            public long InvocationCount { get; }
            public long TotalValue { get; }
            public long MaximumValue { get; }

            public string FormatNanoseconds()
            {
                double averageMicroseconds = InvocationCount > 0
                    ? TotalValue / (double)InvocationCount / 1_000d
                    : 0d;
                double maximumMicroseconds = MaximumValue / 1_000d;
                return $"avg {averageMicroseconds:F3} us, max {maximumMicroseconds:F3} us, " +
                       $"calls {InvocationCount}, samples {SampleCount}";
            }

            public string FormatBytes()
            {
                double averageBytes = SampleCount > 0
                    ? TotalValue / (double)SampleCount
                    : 0d;
                return $"avg {averageBytes:F0} B, max {MaximumValue} B, samples {SampleCount}";
            }
        }
    }
}
