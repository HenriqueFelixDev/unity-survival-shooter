using SurvivalShooter.Stats;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SurvivalShooter.Characters
{
    [Serializable]
    public class PlayerStatChangeEvent : UnityEvent<Player, StatType, float> { }

    [Serializable]
    public class LevelUpEvent : UnityEvent<Player, int> { }

    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerBaseStats baseStats;

        private Health m_health;
        private CharacterExperience m_experience;

        public LevelUpEvent OnLevelUp;
        public PlayerStatChangeEvent OnStatChange;
        public UnityEvent<Player> OnDie;

        private void Awake()
        {
            m_health = new(baseStats);
            m_experience = new(baseStats);
        }

        private void OnEnable()
        {
            m_health.OnChange += Health_OnChange;
            m_health.OnDie += Health_OnDie;
            m_experience.OnChange += Experience_OnChange;
            m_experience.OnLevelUp += Experience_OnLevelUp;
        }

        private void OnDisable()
        {
            m_health.OnChange -= Health_OnChange;
            m_health.OnDie -= Health_OnDie;
            m_experience.OnChange -= Experience_OnChange;
            m_experience.OnLevelUp -= Experience_OnLevelUp;
        }

        public float Health
        {
            get
            {
                return m_health.CurrentHealth;
            }
        }

        public float MaxHealth
        {
            get
            {
                return m_health.MaxHealth;
            }
        }

        public float AttackDamage
        {
            get
            {
                return baseStats.AttackDamage;
            }
        }

        public float AttackSpeed
        {
            get
            {
                return baseStats.AttackSpeed;
            }
        }

        public float AttackRange
        {
            get
            {
                return baseStats.AttackRange;
            }
        }

        public float MovementSpeed
        {
            get
            {
                return baseStats.MovementSpeed;
            }
        }

        public float Level
        {
            get
            {
                return m_experience.Level;
            }
        }

        public float Experience
        {
            get
            {
                return m_experience.Experience;
            }
        }

        public float MaxExperienceForCurrentLevel
        {
            get
            {
                return m_experience.MaxExperienceForCurrentLevel;
            }
        }

        public int MaxLevel
        {
            get
            {
                return m_experience.MaxLevel;
            }
        }

        public void Health_OnChange(object sender, EventArgs e)
        {
            OnStatChange?.Invoke(this, StatType.Health, Health);
        }

        public void Health_OnDie(object sender, EventArgs e)
        {
            OnDie?.Invoke(this);
        }

        private void Experience_OnLevelUp(object sender, EventArgs e)
        {
            OnStatChange?.Invoke(this, StatType.Level, Level);
        }

        private void Experience_OnChange(object sender, EventArgs e)
        {
            OnStatChange?.Invoke(this, StatType.Experience, Experience);
        }
    }
}
