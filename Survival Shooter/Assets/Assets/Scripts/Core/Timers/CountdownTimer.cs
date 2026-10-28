
namespace SurvivalShooter.Core.Timers
{
    public class CountdownTimer : Timer
    {
        public CountdownTimer(TimerManager timerManager, float initialTime) 
            : base(timerManager, initialTime) { }

        protected override void HandleTick(float deltaTime)
        {
            if (IsRunning && CurrentTime > 0)
            {
                CurrentTime -= deltaTime;
            }

            if (IsRunning && CurrentTime <= 0)
                Finish();
        }
    }
}