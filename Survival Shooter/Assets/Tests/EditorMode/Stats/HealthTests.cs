using System;
using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Stats;
using SurvivalShooter.Stats.ModifierStrategy;
using SurvivalShooter.Stats.StatModifier;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace EditorModeTest.Stats
{
    public class HealthTests
    {
        private PlayerBaseStats m_baseStats;
        private StatMediator m_mediator;
        private Health m_health;
        private readonly FloatEqualityComparer floatEqualityComparer = new(0.01f);

        [SetUp]
        public void SetUp()
        {
            m_baseStats = ScriptableObject.CreateInstance<PlayerBaseStats>();
            m_baseStats.MaxHealth = 100f;
            m_mediator = new StatMediator();
            m_health = new Health(m_baseStats, m_mediator);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(m_baseStats);
        }

        [Test]
        public void NewHealth_StartsAtMaximumHealth()
        {
            Assert.That(m_health.MaxHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
        }

        [Test]
        public void MaxHealthModifier_Increase_PreservesCurrentHealthPercentage()
        {
            m_health.ApplyDamage(25f);
            m_mediator.AddModifier(new FixedStatModifier(
                StatType.MaxHealth, new AddModifierStrategy(50f)));

            Assert.That(m_health.CurrentHealth, Is.EqualTo(112.5f).Using(floatEqualityComparer));
            Assert.That(m_health.MaxHealth, Is.EqualTo(150f).Using(floatEqualityComparer));
        }

        [Test]
        public void MaxHealthModifier_Decrease_PreservesCurrentHealthPercentage()
        {
            m_health.ApplyDamage(25f);
            m_mediator.AddModifier(new FixedStatModifier(
                StatType.MaxHealth, new AddModifierStrategy(-40f)));

            Assert.That(m_health.CurrentHealth, Is.EqualTo(45f).Using(floatEqualityComparer));
            Assert.That(m_health.MaxHealth, Is.EqualTo(60f).Using(floatEqualityComparer));
        }

        [Test]
        public void MaxHealthModifier_Removal_ClampsCurrentHealthToReducedMaximum()
        {
            RemovableFixedStatModifier modifier = new(
                StatType.MaxHealth, new AddModifierStrategy(50f));
            m_mediator.AddModifier(modifier);
            Assert.That(m_health.CurrentHealth, Is.EqualTo(150f).Using(floatEqualityComparer));

            int changeCount = 0;
            m_health.OnChange += (sender, args) => changeCount++;

            modifier.MarkForRemoval();
            m_mediator.Update(0f);

            Assert.That(m_health.MaxHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(changeCount, Is.EqualTo(1));
        }

        [Test]
        public void NonMaxHealthModifier_DoesNotChangeHealthProperties()
        {
            m_mediator.AddModifier(new FixedStatModifier(
                StatType.AttackDamage, new AddModifierStrategy(50f)));

            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(m_health.MaxHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
        }

        [Test]
        public void ApplyDamage_ReducesHealthAndRaisesChangeEvent()
        {
            int changeCount = 0;
            int deathCount = 0;
            object eventSender = null;
            EventArgs eventArgs = null;

            m_health.OnChange += (sender, args) =>
            {
                changeCount++;
                eventSender = sender;
                eventArgs = args;
            };

            m_health.OnDie += (sender, args) => deathCount++;

            m_health.ApplyDamage(35f);

            Assert.That(m_health.CurrentHealth, Is.EqualTo(65f).Using(floatEqualityComparer));
            Assert.That(changeCount, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(m_health));
            Assert.That(eventArgs, Is.SameAs(EventArgs.Empty));
            Assert.That(deathCount, Is.Zero);
        }

        [TestCase(100f)]
        [TestCase(150f)]
        public void ApplyDamage_LethalOrOverkill_ClampsHealthAtZeroAndRaisesDeathEvent(float damage)
        {
            int changeCount = 0;
            int deathCount = 0;
            m_health.OnChange += (sender, args) => changeCount++;
            m_health.OnDie += (sender, args) => deathCount++;

            m_health.ApplyDamage(damage);

            Assert.That(m_health.CurrentHealth, Is.Zero);
            Assert.That(changeCount, Is.EqualTo(1));
            Assert.That(deathCount, Is.EqualTo(1));
        }

        [Test]
        public void Heal_RestoresHealthAndRaisesChangeEvent()
        {
            m_health.ApplyDamage(35f);

            int changeCount = 0;
            int deathCount = 0;
            object eventSender = null;
            EventArgs eventArgs = null;
            m_health.OnChange += (sender, args) =>
            {
                changeCount++;
                eventSender = sender;
                eventArgs = args;
            };
            m_health.OnDie += (sender, args) => deathCount++;

            m_health.Heal(20f);

            Assert.That(m_health.CurrentHealth, Is.EqualTo(85f).Using(floatEqualityComparer));
            Assert.That(changeCount, Is.EqualTo(1));
            Assert.That(eventSender, Is.SameAs(m_health));
            Assert.That(eventArgs, Is.SameAs(EventArgs.Empty));
            Assert.That(deathCount, Is.Zero);
        }

        [Test]
        public void Heal_AboveMaximum_ClampsHealthAtMaximum()
        {
            m_health.ApplyDamage(35f);

            int changeCount = 0;
            m_health.OnChange += (sender, args) => changeCount++;

            m_health.Heal(100f);

            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(changeCount, Is.EqualTo(1));
        }

        [Test]
        public void Heal_NegativeAmount_ThrowsWithoutChangingHealthOrRaisingEvents()
        {
            int changeCount = 0;
            int deathCount = 0;
            m_health.OnChange += (sender, args) => changeCount++;
            m_health.OnDie += (sender, args) => deathCount++;

            Assert.Throws<ArgumentOutOfRangeException>(() => m_health.Heal(-1f));

            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f).Using(floatEqualityComparer));
            Assert.That(changeCount, Is.Zero);
            Assert.That(deathCount, Is.Zero);
        }

        [Test]
        public void ApplyDamage_NegativeDamage_ThrowsWithoutChangingHealthOrRaisingEvents()
        {
            int changeCount = 0;
            int deathCount = 0;
            m_health.OnChange += (sender, args) => changeCount++;
            m_health.OnDie += (sender, args) => deathCount++;

            Assert.Throws<ArgumentOutOfRangeException>(() => m_health.ApplyDamage(-1f));

            Assert.That(m_health.CurrentHealth, Is.EqualTo(100f));
            Assert.That(changeCount, Is.Zero);
            Assert.That(deathCount, Is.Zero);
        }

        private sealed class RemovableFixedStatModifier : FixedStatModifier
        {
            public RemovableFixedStatModifier(StatType statType, AddModifierStrategy strategy)
                : base(statType, strategy) { }

            public void MarkForRemoval()
            {
                MarkToRemoval = true;
            }
        }
    }
}
