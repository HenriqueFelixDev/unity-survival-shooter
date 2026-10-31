using SurvivalShooter.Stats;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SurvivalShooter.Characters
{
    [Serializable]
    public class EnemyStatChangeEvent : UnityEvent<Enemy, StatType, float> { }

    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyBaseStats baseStats;
        
        private Health m_health;
        private readonly StatMediator m_mediator = new();

        public EnemyStatChangeEvent OnStatChange;
        public UnityEvent<Enemy> OnDie;

        private void Awake()
        {
            m_health = new Health(baseStats, m_mediator);
        }

        public Health Health
        {
            get
            {
                return m_health;
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

        public float ExperienceGranted
        {
            get
            {
                return baseStats.XpGranted;
            }
        }
    }
}
