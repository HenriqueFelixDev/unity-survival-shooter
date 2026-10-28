using UnityEngine;

namespace SurvivalShooter.Core.Timers
{
    public class TimerManagerComponent : MonoBehaviour
    {
        private readonly TimerManager m_timerManager = new();

        private bool m_isPaused = false;

        private void Awake()
        {
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