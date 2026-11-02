using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter.Characters
{
    public class Player : Character, IVisitable
    {
        [SerializeField] private PlayerBaseStats baseStats;

        private CharacterExperience m_experience;

        private void Awake()
        {
            Initialize(baseStats);
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
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackSpeed
        {
            get
            {
                Query query = new Query(StatType.AttackSpeed, baseStats.AttackSpeed);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackRange
        {
            get
            {
                Query query = new Query(StatType.AttackRange, baseStats.AttackRange);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float MovementSpeed
        {
            get
            {
                Query query = new Query(StatType.MovementSpeed, baseStats.MovementSpeed);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public void ApplyDamage(Enemy enemy)
        {
            enemy.TakeDamage(this);
        }

        public void TakeDamage(Enemy enemy)
        {
            Health.ApplyDamage(enemy.AttackDamage);
        }
    }
}
