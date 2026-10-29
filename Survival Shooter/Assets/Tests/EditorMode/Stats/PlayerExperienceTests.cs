using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Stats;
using UnityEngine;

namespace EditorModeTest.Stats
{
    public class PlayerExperienceTests
    {
        [Test]
        public void IncrementExperience_BelowThreshold_UpdatesExperienceWithoutLevelingUp()
        {
            CharacterExperience xp = new(CreatePlayerBaseStats(100f, 200f, 300f));
            
            float xpGranted = 40f;
            int statChangeCount = 0;
            int levelUpCount = 0;

            xp.OnChange += (source, e) => statChangeCount++;

            xp.OnLevelUp += (source, e) => levelUpCount++;

            xp.IncrementExperience(xpGranted);

            Assert.That(xp.Level, Is.EqualTo(1f));
            Assert.That(xp.Experience, Is.EqualTo(xpGranted));
            Assert.That(statChangeCount, Is.EqualTo(1));
            Assert.That(levelUpCount, Is.Zero);
        }

        [Test]
        public void IncrementExperience_ExactThreshold_AdvancesLevelAndResetsExperience()
        {
            CharacterExperience xp = new(CreatePlayerBaseStats(100f, 200f, 300f));
            float xpGranted = 100f;

            int levelUpCount = 0;

            xp.OnLevelUp += (source, e) => levelUpCount++;

            xp.IncrementExperience(xpGranted);

            Assert.That(xp.Level, Is.EqualTo(2f));
            Assert.That(xp.Experience, Is.Zero);
            Assert.That(xp.MaxExperienceForCurrentLevel, Is.EqualTo(200f));
            Assert.That(levelUpCount, Is.EqualTo(1));
        }

        [Test]
        public void IncrementExperience_CrossesMultipleThresholds_CarriesRemainder()
        {
            CharacterExperience xp = new(CreatePlayerBaseStats(100f, 200f, 300f));
            
            float xpGranted = 350f;
            int levelUpCount = 0;

            xp.OnLevelUp += (source, e) => levelUpCount++;

            xp.IncrementExperience(xpGranted);

            Assert.That(xp.Level, Is.EqualTo(3f));
            Assert.That(xp.Experience, Is.EqualTo(50f));
            Assert.That(xp.MaxExperienceForCurrentLevel, Is.EqualTo(300f));
            Assert.That(levelUpCount, Is.EqualTo(1));
        }

        [Test]
        public void IncrementExperience_NegativeEnemyExperience_ThrowsWithoutChangingProgress()
        {
            CharacterExperience xp = new(CreatePlayerBaseStats(100f, 200f));
            float xpGranted = -10f;

            int statChangeCount = 0;
            int levelUpCount = 0;

            xp.OnChange += (source, e) => statChangeCount++;
            xp.OnLevelUp += (source, e) => levelUpCount++;

            Assert.Throws<System.ArgumentOutOfRangeException>(() => xp.IncrementExperience(xpGranted));

            Assert.That(xp.Level, Is.EqualTo(1f));
            Assert.That(xp.Experience, Is.Zero);
            Assert.That(statChangeCount, Is.Zero);
            Assert.That(levelUpCount, Is.Zero);
        }

        private PlayerBaseStats CreatePlayerBaseStats(params float[] experienceThresholds)
        {
            PlayerBaseStats baseStats = ScriptableObject.CreateInstance<PlayerBaseStats>();
            baseStats.XpLevels = CreateExperienceLevels(experienceThresholds);

            return baseStats;
        }

        private static PlayerExperienceLevel[] CreateExperienceLevels(float[] thresholds)
        {
            PlayerExperienceLevel[] experienceLevels = new PlayerExperienceLevel[thresholds.Length];

            for (int i = 0; i < thresholds.Length; i++)
            {
                experienceLevels[i] = new PlayerExperienceLevel { ExperienceAmount = thresholds[i] };
            }

            return experienceLevels;
        }
    }
}
