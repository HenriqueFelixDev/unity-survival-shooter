using SurvivalShooter.Stats.ModifierStrategy;

namespace SurvivalShooter.Stats.StatModifier
{
    public class FixedStatModifier : BasicStatModifier
    {
        public FixedStatModifier(StatType statType, IModifierStrategy strategy) 
            : base(statType, strategy) { }

        public override void Update(float deltaTime) { }
    }
}
