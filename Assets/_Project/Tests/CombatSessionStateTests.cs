using LastSeed.Gameplay.Combat;
using NUnit.Framework;

namespace LastSeed.Tests
{
    public sealed class CombatSessionStateTests
    {
        [Test]
        public void SetShootingEnabled_WhenValueChanges_PublishesNewValueOnce()
        {
            CombatSessionState state = new();
            int notifications = 0;
            bool publishedValue = false;
            state.ShootingEnabledChanged += value =>
            {
                notifications++;
                publishedValue = value;
            };

            state.SetShootingEnabled(true);
            state.SetShootingEnabled(true);

            Assert.That(state.IsShootingEnabled, Is.True);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(publishedValue, Is.True);
        }

        [Test]
        public void Reset_FromEnabledState_PublishesDisabledValue()
        {
            CombatSessionState state = new();
            bool? publishedValue = null;
            state.SetShootingEnabled(true);
            state.ShootingEnabledChanged += value => publishedValue = value;

            state.Reset();

            Assert.That(state.IsShootingEnabled, Is.False);
            Assert.That(publishedValue, Is.False);
        }
    }
}
