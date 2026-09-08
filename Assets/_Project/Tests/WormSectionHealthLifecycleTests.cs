using NUnit.Framework;

using Game.Gameplay.Enemy.Worm.Combat;

namespace Game.Tests
{
    public sealed class WormSectionHealthLifecycleTests
    {
        [Test]
        public void DestroyedPayload_RemainsFinalAfterSectionReset()
        {
            WormSection section = new();
            WormSectionDestroyed destroyed = default;
            section.Destroyed += payload => destroyed = payload;
            section.InitializeHp(10);

            section.Damage(15);
            section.ResetHp(20);

            Assert.That(destroyed.Section, Is.SameAs(section));
            Assert.That(destroyed.FinalChange.PreviousHp, Is.EqualTo(10));
            Assert.That(destroyed.FinalChange.CurrentHp, Is.Zero);
            Assert.That(destroyed.FinalChange.MaxHp, Is.EqualTo(10));
            Assert.That(destroyed.FinalChange.AppliedDamage, Is.EqualTo(10));
            Assert.That(destroyed.FinalChange.IsDepleted, Is.True);
            Assert.That(section.CurrentHp, Is.EqualTo(20));
            Assert.That(section.IsDestroyed, Is.False);
        }

        [Test]
        public void RemovedObservers_DoNotReceiveLaterLifecycleEvents()
        {
            WormSection section = new();
            int changedCount = 0;
            int destroyedCount = 0;
            void OnChanged(WormSectionHealthChanged _) => changedCount++;
            void OnDestroyed(WormSectionDestroyed _) => destroyedCount++;
            section.HpChanged += OnChanged;
            section.Destroyed += OnDestroyed;
            section.InitializeHp(10);
            section.HpChanged -= OnChanged;
            section.Destroyed -= OnDestroyed;

            section.Damage(10);

            Assert.That(changedCount, Is.Zero);
            Assert.That(destroyedCount, Is.Zero);
        }
    }
}
