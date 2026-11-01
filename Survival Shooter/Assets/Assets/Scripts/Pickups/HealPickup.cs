using SurvivalShooter.Characters;
using UnityEngine;

namespace SurvivalShooter
{
    public class HealPickup : Pickup
    {
        [SerializeField, Min(1)] private float healAmount;

        internal void Initialize(float healAmount)
        {
            this.healAmount = healAmount;
        }

        protected override void HandlePickup(Player player)
        {
            player.Health.Heal(healAmount);
        }
    }
}
