namespace SurvivalShooter.Stats.ModifierStrategy
{
    public interface IModifierStrategy
    {
        float Execute(float value);
        OperationType OperationType { get; }
    }
}
