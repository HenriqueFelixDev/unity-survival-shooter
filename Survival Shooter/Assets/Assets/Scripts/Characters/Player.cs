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

        [Header("Events")]
        public LevelUpEvent OnLevelUp;
        public PlayerStatChangeEvent OnStatChange;
        public UnityEvent<Player> OnDie;

        private readonly StatMediator m_mediator = new();

        private Health m_health;
        private CharacterExperience m_experience;

        private void Awake()
        {
            m_health = new Health(baseStats, m_mediator);
            m_experience = new CharacterExperience(baseStats);
        }

        public Health Health
        {
            get
            {
                return m_health;
            }
        }

        public CharacterExperience Experience
        {
            get
            {
                return m_experience;
            }
        }

        public float AttackDamage
        {
            get
            {
                Query query = new Query(StatType.AttackDamage, baseStats.AttackDamage);
                m_mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackSpeed
        {
            get
            {
                Query query = new Query(StatType.AttackSpeed, baseStats.AttackSpeed);
                m_mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackRange
        {
            get
            {
                Query query = new Query(StatType.AttackRange, baseStats.AttackRange);
                m_mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float MovementSpeed
        {
            get
            {
                Query query = new Query(StatType.MovementSpeed, baseStats.MovementSpeed);
                m_mediator.PerformQuery(this, query);

                return query.Value;
            }
        }
    }
}
