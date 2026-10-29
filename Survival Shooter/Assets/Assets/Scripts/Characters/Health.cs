using SurvivalShooter.Stats;
using System;
using UnityEngine;

namespace SurvivalShooter
{
    public class Health
    {
        private readonly BaseStats m_baseStats;
        private float m_baseHealth;

        public event EventHandler OnChange;
        public event EventHandler OnDie;

        public Health(BaseStats baseStats)
        {
            m_baseStats = baseStats;
            m_baseHealth = m_baseStats.MaxHealth;
        }

        public float CurrentHealth
        {
            get
            {
                return m_baseHealth;
            }
        }

        public float MaxHealth
        {
            get
            {
                return m_baseStats.MaxHealth;
            }
        }

        public void ApplyDamage(float damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage.ToString());
            }

            m_baseHealth = Mathf.Clamp(m_baseHealth - damage, 0, MaxHealth);

            OnChange?.Invoke(this, EventArgs.Empty);

            if (m_baseHealth <= 0)
            {
                OnDie?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
