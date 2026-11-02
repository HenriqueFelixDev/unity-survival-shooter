using SurvivalShooter.Core.Timers;
using System;

namespace SurvivalShooter
{
    public abstract class Skill
    {
        private readonly Timer m_cooldownTimer;

        protected SkillManager m_skillManager;

        private bool m_isSkillExecuting;
        private bool m_isDisposed;

        public event Action OnStart = delegate { };
        public event Action OnComplete = delegate { };
        public event Action OnCancel = delegate { };

        protected Skill(TimerManager timerManager, float cooldownTime)
        {
            m_cooldownTimer = new CountdownTimer(timerManager, cooldownTime);
        }

        public Timer Cooldown => m_cooldownTimer;

        public bool IsExecuting => m_isSkillExecuting;

        public bool IsDisposed => m_isDisposed;

        public void SetSkillManager(SkillManager skillManager)
        {
            m_skillManager = skillManager;
        }

        public virtual bool CanExecute()
        {
            return !(m_isDisposed || m_isSkillExecuting || m_cooldownTimer.IsRunning);
        }

        public abstract SkillType SkillType { get; }
        protected abstract bool IsCompleted();

        protected abstract void HandleUpdate(float deltaTime);

        public abstract void ResolveConflict(Skill other);

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
            if (!m_isSkillExecuting && !m_cooldownTimer.IsRunning)
                return;

            m_isSkillExecuting = false;
            m_cooldownTimer.Stop();
            OnCancel?.Invoke();
        }

        public virtual void Update(float deltaTime)
        {
            if (m_isDisposed)
                return;

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
            if (m_isDisposed)
                return;

            m_isDisposed = true;
            Cancel();
            m_cooldownTimer.Dispose();
        }
    }
}
