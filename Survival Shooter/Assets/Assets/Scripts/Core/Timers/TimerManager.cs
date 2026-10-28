using System.Collections.Generic;

namespace SurvivalShooter.Core.Timers
{
    public class TimerManager
    {
        private readonly List<Timer> m_timers = new();

        public int TimersCount => m_timers.Count;

        public void RegisterTime(Timer timer)
        {
            m_timers.Add(timer);
        }

        public void UnregisterTime(Timer timer)
        {
            m_timers.Remove(timer);
        }

        public void Update(float deltaTime)
        {
            foreach (var timer in new List<Timer>(m_timers))
            {
                timer.Tick(deltaTime);
            }
        }
    }
}