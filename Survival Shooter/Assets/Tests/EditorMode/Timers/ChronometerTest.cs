using NUnit.Framework;
using SurvivalShooter.Core.Timers;
using UnityEngine.TestTools.Utils;

namespace EditorModeTests.Timers
{
    public class TimersTest
    {
        private readonly FloatEqualityComparer floatEqualityComparer = new(0.001f);

        private const float WAIT_TIME = 0.01f;

        [Test]
        public void CreatedTimer_CheckCurrentTime_EqualsInitial()
        {
            float initialTime = 0;
            TimerManager timerManager = new TimerManager();
            Timer timer = new ChronometerTimer(timerManager);

            Assert.That(
                timer.CurrentTime,
                Is.EqualTo(initialTime).Using(floatEqualityComparer));
        }

        [Test]
        public void CreatedTimer_Start_CurrentTimeIsGreaterThanInitialTime()
        {
            float initialTime = 0;
            TimerManager timerManager = new TimerManager();
            Timer timer = new ChronometerTimer(timerManager);

            timer.Start();
            timerManager.Update(WAIT_TIME);

            Assert.That(
                timer.CurrentTime,
                Is.EqualTo(initialTime + WAIT_TIME).Using(floatEqualityComparer));
            Assert.True(timer.IsRunning);
            Assert.AreEqual(1, timerManager.TimersCount);
        }

        [Test]
        public void RunningTimer_Stop_CurrentTimeStopsUpdate()
        {
            TimerManager timerManager = new TimerManager();
            Timer timer = new ChronometerTimer(timerManager);

            timer.Start();
            timerManager.Update(WAIT_TIME);

            Assert.True(timer.IsRunning);
            Assert.AreEqual(1, timerManager.TimersCount);

            timer.Stop();

            float stoppedTime = timer.CurrentTime;

            timerManager.Update(WAIT_TIME);

            Assert.That(timer.CurrentTime, Is.EqualTo(stoppedTime));
            Assert.False(timer.IsRunning);
            Assert.Zero(timerManager.TimersCount);
        }

        [Test]
        public void RunningTimer_Reset_CurrentTimeChangeToInitialTime()
        {
            float initialTime = 0;

            TimerManager timerManager = new TimerManager();
            Timer timer = new ChronometerTimer(timerManager);

            timer.Start();
            timerManager.Update(WAIT_TIME);

            Assert.That(timer.CurrentTime, Is.EqualTo(initialTime + WAIT_TIME));

            timer.Reset();

            Assert.That(timer.CurrentTime, Is.EqualTo(initialTime));
            Assert.True(timer.IsRunning);
            Assert.AreEqual(1, timerManager.TimersCount);
        }

        [Test]
        public void RunningTimer_ResetWithNewValue_CurrentTimeChangeToNewValue()
        {
            float initialTime = 0;
            float resetValue = 2f;

            TimerManager timerManager = new TimerManager();
            Timer timer = new ChronometerTimer(timerManager);

            timer.Start();
            timerManager.Update(WAIT_TIME);

            Assert.That(timer.CurrentTime, Is.EqualTo(initialTime + WAIT_TIME));

            timer.Reset(resetValue);

            Assert.That(timer.CurrentTime, Is.EqualTo(resetValue));
        }
    }
}
