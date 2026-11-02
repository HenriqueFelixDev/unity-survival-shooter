using SurvivalShooter.Core.Timers;
using System;

namespace SurvivalShooter
{
    public abstract class Skill
    {
        private readonly Timer m_cooldownTimer;

        private bool m_isSkillExecuting;

        public event Action OnStart = delegate { };
        public event Action OnComplete = delegate { };
        public event Action OnCancel = delegate { };

        protected Skill(TimerManager timerManager, float cooldownTime)
        {
            m_cooldownTimer = new CountdownTimer(timerManager, cooldownTime);
        }

        public Timer Cooldown => m_cooldownTimer;

        public virtual bool CanExecute()
        {
            return !(m_isSkillExecuting || m_cooldownTimer.IsRunning);
        }

        protected abstract bool IsCompleted();

        protected abstract void HandleUpdate(float deltaTime);

        public void Execute()
        {
            if (CanExecute())
                Start();
        }

        private void Start()
        {
            m_isSkillExecuting = true;
            OnStart?.Invoke();
        }

        private void Complete()
        {
            m_isSkillExecuting = false;
            OnComplete?.Invoke();
            m_cooldownTimer.Start();
        }

        public void Cancel()
        {
            m_isSkillExecuting = false;
            m_cooldownTimer.Stop();
            OnCancel?.Invoke();
        }

        public virtual void Update(float deltaTime)
        {
            if (m_cooldownTimer.IsRunning)
            {
                m_cooldownTimer.Tick(deltaTime);
            }

            if (!m_isSkillExecuting)
                return;

            HandleUpdate(deltaTime);

            if (IsCompleted())
                Complete();
        }

        public void Dispose()
        {
            m_cooldownTimer.Dispose();
        }
    }
}
