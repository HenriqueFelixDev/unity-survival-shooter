using System;
using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Stats;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace EditorModeTest.Stats
{
    public class HealthTests
    {
        private PlayerBaseStats m_baseStats;
        private Health m_health;
        private readonly FloatEqualityComparer floatEqualityComparer = new(0.01f);

        [SetUp]
        public void SetUp()
        {
            m_baseStats = ScriptableObject.CreateInstance<PlayerBaseStats>();
            m_baseStats.MaxHealth = 100f;
            m_health = new Health(m_baseStats);
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
        public void ApplyDamage_LethalDamage_ClampsHealthAtZeroAndRaisesDeathEvent(float damage)
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
    }
}
