using System;
using System.Collections;
using System.Reflection;
using Game.Core.Pooling;
using Game.Gameplay.Enemy.Worm;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Enemy.Worm.Presentation;
using Game.Presentation.UI.Common;
using Game.Presentation.UI.Common.Popups;
using Game.Presentation.UI.Rewards;
using Game.Presentation.Worm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

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
            WormDestructionProgressState progressState = new();
            progressState.Reset(10);
            progressState.RecordDestroyed(5);
            presenter.Construct(null, progressState);

            owner.SetActive(true);
            yield return null;
            Assert.That(GetText(text), Is.EqualTo("Progress: 50%"));

            owner.SetActive(false);
            progressState.RecordDestroyed(2);
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

        [UnityTest]
        public IEnumerator NavigationButtons_WhenReenabled_ReturnToIdleInteraction()
        {
            GameObject lobbyOwner = new("LobbyButton", typeof(RectTransform), typeof(Button));
            LobbyStartBattleButton lobby = lobbyOwner.AddComponent<LobbyStartBattleButton>();
            Button lobbyButton = lobbyOwner.GetComponent<Button>();
            lobbyButton.interactable = false;

            lobby.enabled = false;
            lobby.enabled = true;

            GameObject gameplayOwner = new(
                "GameplayButton",
                typeof(RectTransform),
                typeof(Button));
            GameplayBackToLobbyButton gameplay =
                gameplayOwner.AddComponent<GameplayBackToLobbyButton>();
            Button gameplayButton = gameplayOwner.GetComponent<Button>();
            gameplayButton.interactable = false;

            gameplay.enabled = false;
            gameplay.enabled = true;
            yield return null;

            Assert.That(lobbyButton.interactable, Is.True);
            Assert.That(gameplayButton.interactable, Is.True);
            UnityEngine.Object.Destroy(lobbyOwner);
            UnityEngine.Object.Destroy(gameplayOwner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WinPopup_WhenReenabled_ClearsStaleCompletionState()
        {
            GameObject owner = new("WinPopup", typeof(RectTransform));
            owner.SetActive(false);
            Button accept = new GameObject("Accept", typeof(RectTransform), typeof(Button))
                .GetComponent<Button>();
            Button doubleReward = new GameObject(
                "DoubleReward",
                typeof(RectTransform),
                typeof(Button)).GetComponent<Button>();
            accept.transform.SetParent(owner.transform);
            doubleReward.transform.SetParent(owner.transform);
            WinPopupView popup = owner.AddComponent<WinPopupView>();
            SetField(popup, "_acceptButton", accept);
            SetField(popup, "_doubleRewardButton", doubleReward);
            owner.SetActive(true);
            SetField(popup, "_restartRequested", true);
            accept.interactable = false;
            doubleReward.interactable = false;

            popup.enabled = false;
            popup.enabled = true;
            yield return null;

            Assert.That(GetField<bool>(popup, "_restartRequested"), Is.False);
            Assert.That(accept.interactable, Is.True);
            Assert.That(doubleReward.interactable, Is.True);
            UnityEngine.Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RevivalPopup_WhenReenabled_ReappliesLatestRenderState()
        {
            GameObject owner = new("RevivalPopup", typeof(RectTransform));
            owner.SetActive(false);
            Button revive = new GameObject("Revive", typeof(RectTransform), typeof(Button))
                .GetComponent<Button>();
            Button giveUp = new GameObject("GiveUp", typeof(RectTransform), typeof(Button))
                .GetComponent<Button>();
            revive.transform.SetParent(owner.transform);
            giveUp.transform.SetParent(owner.transform);
            RevivalPopupView popup = owner.AddComponent<RevivalPopupView>();
            SetField(popup, "_reviveButton", revive);
            SetField(popup, "_giveUpButton", giveUp);
            popup.Render(new RevivalPopupViewModel(1, 0.4f, 0.6f, true, false));
            owner.SetActive(true);
            revive.interactable = false;
            giveUp.interactable = false;

            popup.enabled = false;
            popup.enabled = true;
            yield return null;

            Assert.That(revive.interactable, Is.True);
            Assert.That(giveUp.interactable, Is.True);
            UnityEngine.Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RewardPopup_WhenReenabled_RestartsInteractionGate()
        {
            GameObject owner = new("RewardPopup", typeof(RectTransform));
            owner.SetActive(false);
            LogAssert.Expect(LogType.Error, "RewardPopupView requires an animation config.");
            LogAssert.Expect(
                LogType.Error,
                "RewardPopupView requires an action presentation config.");
            RewardPopupView popup = owner.AddComponent<RewardPopupView>();
            RewardPopupAnimationConfig animationConfig =
                ScriptableObject.CreateInstance<RewardPopupAnimationConfig>();
            RewardPopupActionPresentationConfig actionConfig =
                ScriptableObject.CreateInstance<RewardPopupActionPresentationConfig>();
            SetField(popup, "_animationConfig", animationConfig);
            SetField(popup, "_actionPresentationConfig", actionConfig);
            SetField(popup, "_hasBoundChoices", true);
            owner.SetActive(true);
            yield return null;
            yield return null;
            yield return null;
            RewardPopupInteractionGate gate =
                GetField<RewardPopupInteractionGate>(popup, "_interactionGate");
            Assert.That(gate.IsOpen, Is.True);
            gate.Close();

            popup.enabled = false;
            popup.enabled = true;
            yield return null;
            yield return null;
            yield return null;

            Assert.That(gate.IsOpen, Is.True);
            UnityEngine.Object.Destroy(owner);
            UnityEngine.Object.Destroy(animationConfig);
            UnityEngine.Object.Destroy(actionConfig);
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

        private static T GetField<T>(object target, string name)
        {
            return (T)target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(target);
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
