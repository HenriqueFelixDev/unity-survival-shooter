namespace SurvivalShooter.Core.Timers
{
    public class ChronometerTimer : Timer
    {
        public ChronometerTimer(TimerManager timerManager) : base(timerManager, 0) { }

        protected override void HandleTick(float deltaTime)
        {
            if (IsRunning)
            {
                CurrentTime += deltaTime;
            }
        }
    }
}