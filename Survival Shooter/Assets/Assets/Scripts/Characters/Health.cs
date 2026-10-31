using SurvivalShooter.Stats;
using SurvivalShooter.Stats.StatModifier;
using System;
using UnityEngine;

namespace SurvivalShooter
{
    public class Health
    {
        private readonly BaseStats m_baseStats;
        private readonly StatMediator m_mediator;
        private float m_currentHealth;
        private float m_healthPercent = 1;

        public event EventHandler OnChange;
        public event EventHandler OnDie;

        public Health(BaseStats baseStats, StatMediator mediator)
        {
            m_baseStats = baseStats;
            m_currentHealth = m_baseStats.MaxHealth;
            m_mediator = mediator;
            mediator.OnChange += Mediator_OnChange;
        }

        private void Mediator_OnChange(IStatModifier modifier)
        {
            if (modifier is BasicStatModifier { StatType: var statType } && statType == StatType.MaxHealth)
            {
                if (modifier.MarkToRemoval)
                {
                    UpdateCurrentHealth(m_currentHealth);
                } else
                {
                    m_currentHealth = MaxHealth * m_healthPercent;
                }
            }
        }

        public float CurrentHealth
        {
            get
            {
                return m_currentHealth;
            }
        }

        public float MaxHealth
        {
            get
            {
                Query query = new Query(StatType.MaxHealth, m_baseStats.MaxHealth);
                m_mediator.PerformQuery(this, query);

                return query.Value;
            }
        }

        public void ApplyDamage(float damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage.ToString());
            }

            UpdateCurrentHealth(m_currentHealth - damage);
        }

        public void Heal(float amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount.ToString());
            }

            UpdateCurrentHealth(m_currentHealth + amount);
        }

        private void UpdateCurrentHealth(float newHealth)
        {
            if (m_currentHealth - newHealth == 0.001f) return;

            m_currentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
            m_healthPercent = m_currentHealth / MaxHealth;

            OnChange?.Invoke(this, EventArgs.Empty);

            if (CurrentHealth <= 0)
            {
                OnDie?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
