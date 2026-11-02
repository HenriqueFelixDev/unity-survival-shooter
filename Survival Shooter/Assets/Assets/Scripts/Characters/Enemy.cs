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
            Health.OnDie += (sender, e) => Destroy(gameObject);
        }

        public float ExperienceGranted
        {
            get
            {
                return baseStats.XpGranted;
            }
        }

        protected override void TakeDamage(Character character)
        {
            if (character is not Player player || Health.IsDead())
                return;

            float damage = player.AttackDamage;
            Health.ApplyDamage(damage);

            Debug.Log($"[Enemy({name})] Health after damage: {Health.CurrentHealth}");

            if (Health.IsDead())
            {
                player.Experience.IncrementExperience(ExperienceGranted);
            }
        }
    }
}
