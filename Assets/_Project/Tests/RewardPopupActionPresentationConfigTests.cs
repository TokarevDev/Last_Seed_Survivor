using NUnit.Framework;
using UnityEngine;

namespace LastSeed.Tests
{
    public sealed class RewardPopupActionPresentationConfigTests
    {
        private RewardPopupActionPresentationConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<RewardPopupActionPresentationConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void CreateTextSettings_UsesMigratedPrefabValues()
        {
            RewardPopupActionControls.TextSettings settings = _config.CreateTextSettings();

            Assert.That(settings.AttemptsFormat, Is.EqualTo("attempts left: x{0}"));
            Assert.That(settings.GuaranteeFormat, Is.EqualTo("guarantee: {0}"));
            Assert.That(settings.AdGuaranteeFormat, Is.EqualTo("guarantee: {0}"));
            Assert.That(settings.NumberColor, Is.EqualTo(new Color32(105, 255, 120, 255)));
            Assert.That(settings.CommonRarityColor, Is.EqualTo(new Color32(95, 220, 130, 255)));
            Assert.That(settings.RareRarityColor, Is.EqualTo(new Color32(80, 180, 255, 255)));
            Assert.That(settings.LegendaryRarityColor, Is.EqualTo(new Color32(255, 155, 70, 255)));
            Assert.That(_config.SingleActionButtonAnchoredX, Is.EqualTo(350f));
        }
    }
}
