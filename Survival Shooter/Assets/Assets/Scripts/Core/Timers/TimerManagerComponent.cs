using UnityEngine;

namespace SurvivalShooter.Core.Timers
{
    public class TimerManagerComponent : MonoBehaviour
    {
        public static TimerManagerComponent Instance { get; private set; }

        private readonly TimerManager m_timerManager = new();

        public TimerManager TimerManager => m_timerManager;

        private bool m_isPaused = false;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Pause()
        {
            m_isPaused = true;
        }

        public void Resume()
        {
            m_isPaused = false;
        }

        private void Update()
        {
            if (m_isPaused) return;

            m_timerManager.Update(Time.deltaTime);
        }
    }
}