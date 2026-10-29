using SurvivalShooter.Stats;
using System;

namespace SurvivalShooter
{
    public class CharacterExperience
    {
        private readonly PlayerBaseStats m_baseStats;
        private int m_currentLevel = 1;
        private float m_currentExperience = 0;

        public event EventHandler OnChange;
        public event EventHandler OnLevelUp;

        public CharacterExperience(PlayerBaseStats baseStats)
        {
            m_baseStats = baseStats;
        }

        public float Level
        {
            get
            {
                return m_currentLevel;
            }
        }

        public float Experience
        {
            get
            {
                return m_currentExperience;
            }
        }

        public float MaxExperienceForCurrentLevel
        {
            get
            {
                if (LevelIsValid(m_currentLevel))
                {
                    return m_baseStats.XpLevels[m_currentLevel - 1].ExperienceAmount;
                }

                return 0;
            }
        }

        public int MaxLevel
        {
            get
            {
                return m_baseStats.XpLevels.Length;
            }
        }

        public void IncrementExperience(float experienceGranted)
        {
            if (experienceGranted < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(experienceGranted), experienceGranted.ToString());
            }

            if (!LevelIsValid(m_currentLevel))
            {
                throw new ArgumentOutOfRangeException("level", m_currentLevel.ToString());
            }

            UpdateExperienceAndLevel(experienceGranted, out int increasedLevels);

            OnChange?.Invoke(this, EventArgs.Empty);

            if (increasedLevels > 0)
            {
                OnLevelUp?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateExperienceAndLevel(float xpGained, out int increasedLevels)
        {
            m_currentExperience += xpGained;
            increasedLevels = 0;

            while (m_currentExperience >= MaxExperienceForCurrentLevel)
            {
                m_currentExperience -= MaxExperienceForCurrentLevel;

                if (!LevelIsValid(m_currentLevel + 1))
                {
                    return;
                }

                m_currentLevel++;
                increasedLevels++;
            }
        }

        private bool LevelIsValid(int level)
        {
            return level > 0 && level <= MaxLevel;
        }
    }
}
