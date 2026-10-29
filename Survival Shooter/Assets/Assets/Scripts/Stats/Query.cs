namespace SurvivalShooter.Stats
{
    public class Query
    {
        public StatType StatType { get; private set; }
        public float Value;

        public Query(StatType statType, float value)
        {
            StatType = statType;
            Value = value;
        }
    }
}
