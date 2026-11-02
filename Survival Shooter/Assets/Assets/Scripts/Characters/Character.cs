using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter
{
    public abstract class Character : MonoBehaviour
    {
        private readonly StatMediator m_mediator = new();
        private readonly SkillManager m_skillManager = new();
        private BaseStats m_baseStats;

        private Health m_health;

        public StatMediator Mediator => m_mediator;
        public SkillManager SkillManager => m_skillManager;

        public void Initialize(BaseStats baseStats)
        {
            m_baseStats = baseStats;
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
                Query query = new Query(StatType.AttackDamage, m_baseStats.AttackDamage);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackSpeed
        {
            get
            {
                Query query = new Query(StatType.AttackSpeed, m_baseStats.AttackSpeed);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float AttackRange
        {
            get
            {
                Query query = new Query(StatType.AttackRange, m_baseStats.AttackRange);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public float MovementSpeed
        {
            get
            {
                Query query = new Query(StatType.MovementSpeed, m_baseStats.MovementSpeed);
                Mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public virtual void ApplyDamage(Character character)
        {
            character.TakeDamage(this);
        }

        protected abstract void TakeDamage(Character character);
    }
}
