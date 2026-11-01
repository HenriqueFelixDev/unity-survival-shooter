using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter.Characters
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyBaseStats baseStats;
        
        private Health m_health;
        private readonly StatMediator m_mediator = new();

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

        public void ApplyDamage(Player player)
        {
            player.TakeDamage(this);
        }

        public void TakeDamage(Player player)
        {
            float damage = player.AttackDamage;
            m_health.ApplyDamage(damage);

            if (m_health.IsDead())
            {
                player.Experience.IncrementExperience(ExperienceGranted);
            }
        }
    }
}
