namespace SurvivalShooter.Stats.ModifierStrategy
{
    public class AddModifierStrategy : IModifierStrategy
    {
        public float Value { get; private set; }

        public AddModifierStrategy(float value)
        {
            Value = value;
        }

        public float Execute(float value)
        {
            return value + Value;
        }

        public OperationType OperationType => OperationType.Sum;
    }
}
