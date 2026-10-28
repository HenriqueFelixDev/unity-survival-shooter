using System;
using UnityEngine;

namespace SurvivalShooter.Core.Timers
{
    public abstract class Timer : IDisposable
    {
        private readonly TimerManager m_manager;
        private bool disposed = false;

        public float InitialTime { get; private set; }
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; protected set; }
        public float Progress => InitialTime == 0
            ? CurrentTime
            : Mathf.Clamp01(CurrentTime / InitialTime);

        public event Action OnStart = delegate { };
        public event Action<Timer> OnTick = delegate { };
        public event Action OnStop = delegate { };
        public event Action OnFinish = delegate { };

        protected Timer(TimerManager timerManager, float initialTime)
        {
            m_manager = timerManager;
            InitialTime = initialTime;
            CurrentTime = InitialTime;
        }

        public void Start()
        {
            CurrentTime = InitialTime;

            if (!IsRunning)
            {
                IsRunning = true;
                IsFinished = false;
                m_manager.RegisterTime(this);
                OnStart?.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                m_manager.UnregisterTime(this);
                OnStop?.Invoke();
            }
        }

        public void Reset()
        {
            CurrentTime = InitialTime;
        }

        public void Reset(float newInitialTime)
        {
            InitialTime = newInitialTime;
            Reset();
        }

        public void Finish()
        {
            Stop();
            IsFinished = true;
            OnFinish?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            HandleTick(deltaTime);
            OnTick?.Invoke(this);
        }

        protected abstract void HandleTick(float deltaTime);

        ~Timer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                m_manager.UnregisterTime(this);
            }

            disposed = true;
        }
    }
}