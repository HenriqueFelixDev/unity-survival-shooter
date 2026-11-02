using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter.Characters
{
    public class Enemy : Character
    {
        [SerializeField] private EnemyBaseStats baseStats;

        private void Awake()
        {
            Initialize(baseStats);
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
            Health.ApplyDamage(damage);

            if (Health.IsDead())
            {
                player.Experience.IncrementExperience(ExperienceGranted);
            }
        }
    }
}
