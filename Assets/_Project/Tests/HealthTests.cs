using System;
using System.Collections.Generic;
using NUnit.Framework;

using Game.Core.Combat;

namespace Game.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void ApplyDamage_ClampsAtZeroAndRaisesDepletedOnce()
        {
            Health health = new();
            int changedCount = 0;
            int depletedCount = 0;
            HealthChange finalChange = default;
            health.Changed += _ => changedCount++;
            health.Depleted += change =>
            {
                depletedCount++;
                finalChange = change;
            };
            health.Initialize(10);

            health.ApplyDamage(15);
            health.ApplyDamage(1);

            Assert.That(health.CurrentHp, Is.Zero);
            Assert.That(health.IsDepleted, Is.True);
            Assert.That(changedCount, Is.EqualTo(1));
            Assert.That(depletedCount, Is.EqualTo(1));
            Assert.That(finalChange.PreviousHp, Is.EqualTo(10));
            Assert.That(finalChange.CurrentHp, Is.Zero);
            Assert.That(finalChange.MaxHp, Is.EqualTo(10));
            Assert.That(finalChange.AppliedDamage, Is.EqualTo(10));
            Assert.That(finalChange.IsReset, Is.False);
            Assert.That(finalChange.IsDepleted, Is.True);
        }

        [Test]
        public void Reset_RestoresFullHealthAndNotifiesObservers()
        {
            Health health = new();
            int changedCount = 0;
            HealthChange lastChange = default;
            health.Changed += change =>
            {
                changedCount++;
                lastChange = change;
            };
            health.Initialize(10);
            health.ApplyDamage(4);

            health.Reset(20);

            Assert.That(health.MaxHp, Is.EqualTo(20));
            Assert.That(health.CurrentHp, Is.EqualTo(20));
            Assert.That(health.HasTakenDamage, Is.False);
            Assert.That(changedCount, Is.EqualTo(2));
            Assert.That(lastChange.PreviousHp, Is.EqualTo(6));
            Assert.That(lastChange.CurrentHp, Is.EqualTo(20));
            Assert.That(lastChange.MaxHp, Is.EqualTo(20));
            Assert.That(lastChange.AppliedDamage, Is.Zero);
            Assert.That(lastChange.IsReset, Is.True);
        }

        [Test]
        public void ApplyDamage_IgnoresNonPositiveDamage()
        {
            Health health = new();
            health.Initialize(10);

            health.ApplyDamage(0);
            health.ApplyDamage(-5);

            Assert.That(health.CurrentHp, Is.EqualTo(10));
        }

        [Test]
        public void ApplyDamage_WhenDepleted_RaisesChangedBeforeDepletedWithSamePayload()
        {
            Health health = new();
            List<string> eventOrder = new();
            HealthChange changedPayload = default;
            HealthChange depletedPayload = default;
            health.Changed += change =>
            {
                eventOrder.Add(nameof(health.Changed));
                changedPayload = change;
            };
            health.Depleted += change =>
            {
                eventOrder.Add(nameof(health.Depleted));
                depletedPayload = change;
            };
            health.Initialize(10);

            health.ApplyDamage(10);

            Assert.That(eventOrder, Is.EqualTo(new[]
            {
                nameof(health.Changed),
                nameof(health.Depleted)
            }));
            Assert.That(depletedPayload.PreviousHp, Is.EqualTo(changedPayload.PreviousHp));
            Assert.That(depletedPayload.CurrentHp, Is.EqualTo(changedPayload.CurrentHp));
            Assert.That(depletedPayload.MaxHp, Is.EqualTo(changedPayload.MaxHp));
            Assert.That(depletedPayload.AppliedDamage, Is.EqualTo(changedPayload.AppliedDamage));
            Assert.That(depletedPayload.IsReset, Is.EqualTo(changedPayload.IsReset));
        }

        [Test]
        public void Reset_AfterDepletion_AllowsExactlyOneDepletedEventInNextLifecycle()
        {
            Health health = new();
            int depletedCount = 0;
            health.Depleted += _ => depletedCount++;
            health.Initialize(10);
            health.ApplyDamage(10);

            health.Reset(5);
            health.ApplyDamage(5);
            health.ApplyDamage(1);

            Assert.That(depletedCount, Is.EqualTo(2));
            Assert.That(health.CurrentHp, Is.Zero);
        }
    }
}
