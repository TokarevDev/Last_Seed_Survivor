using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class RevivalPopupViewModelTests
    {
        [Test]
        public void WithWaiting_ChangesOnlyInteractionState()
        {
            RevivalPopupViewModel original = new(
                2,
                0.35f,
                0.65f,
                true,
                false);

            RevivalPopupViewModel waiting = original.WithWaiting(true);

            Assert.That(waiting.AttemptsLeft, Is.EqualTo(original.AttemptsLeft));
            Assert.That(
                waiting.CurrentProgressNormalized,
                Is.EqualTo(original.CurrentProgressNormalized));
            Assert.That(
                waiting.RemainingLevelNormalized,
                Is.EqualTo(original.RemainingLevelNormalized));
            Assert.That(waiting.CanRevive, Is.EqualTo(original.CanRevive));
            Assert.That(waiting.IsWaiting, Is.True);
            Assert.That(original.IsWaiting, Is.False);
        }
    }
}
