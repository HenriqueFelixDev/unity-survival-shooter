namespace SurvivalShooter.Stats.ModifierStrategy
{
    public class MultiplyModifierStrategy : IModifierStrategy
    {
        public float Value { get; private set; }

        public MultiplyModifierStrategy(float value)
        {
            Value = value;
        }

        public float Execute(float value)
        {
            return value * Value;
        }

        public OperationType OperationType => OperationType.Multiply;
    }
}
