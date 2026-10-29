using UnityEngine;

namespace SurvivalShooter.Stats
{
    public abstract class BaseStats : ScriptableObject
    {
        public float MaxHealth;
        public float AttackDamage;
        public float AttackSpeed;
        public float AttackRange;
        public float MovementSpeed;
    }
}
