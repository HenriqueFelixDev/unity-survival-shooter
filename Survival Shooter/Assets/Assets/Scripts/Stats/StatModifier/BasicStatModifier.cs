using SurvivalShooter.Stats.ModifierStrategy;

namespace SurvivalShooter.Stats.StatModifier
{
    public abstract class BasicStatModifier : IStatModifier
    {
        public bool MarkToRemoval { get; protected set; }

        public readonly StatType StatType;
        protected readonly IModifierStrategy m_strategy;

        protected BasicStatModifier(StatType statType, IModifierStrategy strategy)
        {
            StatType = statType;
            m_strategy = strategy;
        }

        public virtual bool CanHandle(object sender, Query query)
        {
            return StatType == query.StatType && !MarkToRemoval;
        }

        public virtual void Handle(object sender, Query query)
        {
            query.Value = m_strategy.Execute(query.Value);
        }

        public abstract void Update(float deltaTime);

        public virtual void Dispose() { }

        public int GetPriority() => (int)m_strategy.OperationType;
    }
}
