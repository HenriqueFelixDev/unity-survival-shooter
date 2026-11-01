using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter.Characters
{
    public class Player : MonoBehaviour, IVisitable
    {
        [SerializeField] private PlayerBaseStats baseStats;

        private readonly StatMediator m_mediator = new();

        private Health m_health;
        private CharacterExperience m_experience;

        private void Awake()
        {
            m_health = new Health(baseStats, m_mediator);
            m_experience = new CharacterExperience(baseStats);
        }

        internal void Initialize(PlayerBaseStats stats)
        {
            baseStats = stats;
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public StatMediator Mediator => m_mediator;

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

        public void ApplyDamage(Enemy enemy)
        {
            enemy.TakeDamage(this);
        }

        public void TakeDamage(Enemy enemy)
        {
            m_health.ApplyDamage(enemy.AttackDamage);
        }
    }
}
