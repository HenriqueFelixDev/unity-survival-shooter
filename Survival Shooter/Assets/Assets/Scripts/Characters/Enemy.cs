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
        
        private Health health;

        public EnemyStatChangeEvent OnStatChange;
        public UnityEvent<Enemy> OnDie;

        private void Awake()
        {
            health = new(baseStats);
        }

        private void OnEnable()
        {
            health.OnChange += Health_OnHealthChange;
            health.OnDie += Health_OnDie;
        }

        private void OnDisable()
        {
            health.OnChange -= Health_OnHealthChange;
            health.OnDie -= Health_OnDie;
        }

        public float Health
        {
            get
            {
                return health.CurrentHealth;
            }
        }

        public float MaxHealth
        {
            get
            {
                return health.MaxHealth;
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

        private void Health_OnHealthChange(object sender, EventArgs e)
        {
            OnStatChange?.Invoke(this, StatType.Health, Health);
        }

        private void Health_OnDie(object sender, EventArgs e)
        {
            OnDie?.Invoke(this);
        }
    }
}
