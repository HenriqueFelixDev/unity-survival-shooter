using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats.ModifierStrategy;

namespace SurvivalShooter.Stats.StatModifier
{
    public abstract class TemporaryStatModifier : BasicStatModifier
    {
        private readonly Timer m_timer;

        protected TemporaryStatModifier(
            TimerManager timerManager,
            float duration,
            StatType statType,
            IModifierStrategy strategy) : base(statType, strategy)
        {
            m_timer = new CountdownTimer(timerManager, duration);
            m_timer.OnStop += Invalidate;
            m_timer.OnFinish += Invalidate;
            m_timer.Start();
        }

        public override void Update(float deltaTime)
        {
            m_timer.Tick(deltaTime);
        }

        private void Invalidate()
        {
            MarkToRemoval = true;
        }

        public override void Dispose()
        {
            m_timer.Dispose();
        }
    }
}
