using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter
{
    public abstract class Character : MonoBehaviour
    {
        private readonly StatMediator m_mediator = new();

        private Health m_health;

        public StatMediator Mediator => m_mediator;

        public Health Health
        {
            get
            {
                return m_health;
            }
        }

        public void Initialize(BaseStats baseStats)
        {
            m_health = new Health(baseStats, m_mediator);
        }
    }
}
