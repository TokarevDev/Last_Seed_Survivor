using NUnit.Framework;
using UnityEngine;

using Game.Presentation.UI.Rewards;

namespace Game.Tests
{
    public sealed class RewardPopupAnimationConfigTests
    {
        private RewardPopupAnimationConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<RewardPopupAnimationConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void CreateSettings_UsesMigratedPrefabValues()
        {
            RewardPopupAnimationSettings settings = _config.CreateSettings();

            Assert.That(settings.RootFadeDuration, Is.EqualTo(0.12f));
            Assert.That(settings.TopEnterOffset, Is.EqualTo(160f));
            Assert.That(settings.TopEnterDuration, Is.EqualTo(0.28f));
            Assert.That(settings.RewardEnterOffset, Is.EqualTo(-230f));
            Assert.That(settings.RewardEnterDuration, Is.EqualTo(0.32f));
            Assert.That(settings.RewardEnterStagger, Is.EqualTo(0.045f));
            Assert.That(settings.ActionEnterOffset, Is.EqualTo(-90f));
            Assert.That(settings.ActionEnterDuration, Is.EqualTo(0.16f));
            Assert.That(settings.RefreshCardStagger, Is.EqualTo(0.055f));
            Assert.That(settings.RefreshOutDuration, Is.EqualTo(0.12f));
            Assert.That(settings.RefreshInDuration, Is.EqualTo(0.22f));
            Assert.That(settings.SelectionFocusDuration, Is.EqualTo(0.22f));
            Assert.That(settings.SelectionGrowDuration, Is.EqualTo(0.16f));
            Assert.That(settings.SelectionExitDuration, Is.EqualTo(0.22f));
            Assert.That(settings.SelectionScaleMultiplier, Is.EqualTo(1.05f));
            Assert.That(settings.SelectionExitScaleMultiplier, Is.EqualTo(0.96f));
            Assert.That(settings.SelectionExitOffset, Is.EqualTo(-230f));
            Assert.That(settings.UnselectedExitDuration, Is.EqualTo(0.22f));
            Assert.That(settings.UnselectedExitStagger, Is.EqualTo(0.045f));
            Assert.That(settings.UnselectedExitScaleMultiplier, Is.EqualTo(0.96f));
            Assert.That(settings.UnselectedExitOffset, Is.EqualTo(-230f));
            Assert.That(settings.TopExitOffset, Is.EqualTo(160f));
            Assert.That(settings.ActionExitOffset, Is.EqualTo(-90f));
        }
    }
}
