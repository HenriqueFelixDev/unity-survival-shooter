using SurvivalShooter.Characters;
using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats;
using SurvivalShooter.Stats.ModifierStrategy;
using SurvivalShooter.Stats.StatModifier;
using UnityEngine;

namespace SurvivalShooter
{
    public enum ModifierType { Permanent, Temporary }

    public class StatModifierPickup : Pickup
    {
        [SerializeField] private StatType statType;
        [SerializeField] private OperationType operationType;
        [SerializeField] private ModifierType modifierType;
        [SerializeField] private float value;
        [SerializeField] private float durationInSeconds;

        internal void Initialize(StatType statType, OperationType operationType, ModifierType modifierType, float value, float durationInSeconds)
        {
            this.statType = statType;
            this.operationType = operationType;
            this.modifierType = modifierType;
            this.value = value;
            this.durationInSeconds = durationInSeconds;
        }

        protected override void HandlePickup(Player player)
        {
            player.Mediator.AddModifier(CreateModifier());
        }

        private IStatModifier CreateModifier()
        {
            var strategy = CreateModifierStrategy();

            return modifierType switch
            {
                ModifierType.Permanent => new FixedStatModifier(statType, strategy),
                ModifierType.Temporary => new TemporaryStatModifier(
                    TimerManagerComponent.Instance.TimerManager,
                    durationInSeconds,
                    statType,
                    strategy),
                _ => throw new System.ArgumentException()
            };
        }

        private IModifierStrategy CreateModifierStrategy()
        {
            return operationType switch
            {
                OperationType.Sum => new AddModifierStrategy(value),
                OperationType.Multiply => new MultiplyModifierStrategy(value),
                _ => throw new System.ArgumentException()
            };
        }
    }
}
