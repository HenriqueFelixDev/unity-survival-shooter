using System.Collections.Generic;
using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Core.Timers;

namespace EditorModeTests.Skills
{
    public class SkillTests
    {
        private TimerManager m_timerManager;
        private readonly List<TestSkill> m_createdSkills = new();

        [SetUp]
        public void SetUp()
        {
            m_timerManager = new TimerManager();
            m_createdSkills.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (TestSkill skill in m_createdSkills)
                skill.Dispose();

            m_createdSkills.Clear();
        }

        [Test]
        public void Execute_WhenConditionsAllow_StartsOnlyOneExecution()
        {
            TestSkill skill = CreateSkill(2f);
            int startEvents = 0;
            skill.OnStart += () => startEvents++;

            skill.Execute();
            skill.Execute();

            Assert.That(startEvents, Is.EqualTo(1), "A running skill must reject another execution.");
            Assert.That(skill.CanExecute(), Is.False);

            skill.Update(0.25f);

            Assert.That(skill.UpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void Execute_WhenExecutionConditionIsFalse_DoesNotStart()
        {
            TestSkill skill = CreateSkill(2f);
            int startEvents = 0;
            skill.CanExecuteCondition = false;
            skill.OnStart += () => startEvents++;

            skill.Execute();

            Assert.That(startEvents, Is.Zero);
            Assert.That(skill.UpdateCount, Is.Zero);

            skill.CanExecuteCondition = true;
            skill.Execute();

            Assert.That(startEvents, Is.EqualTo(1));
            Assert.That(skill.CanExecute(), Is.False);
        }

        [Test]
        public void Execute_DuringCooldown_IsRejectedUntilCooldownFinishes()
        {
            TestSkill skill = CreateSkill(1f);
            int startEvents = 0;
            int completeEvents = 0;
            skill.OnStart += () => startEvents++;
            skill.OnComplete += () => completeEvents++;
            skill.CompleteOnNextUpdate = true;

            skill.Execute();
            skill.Update(0f);

            Assert.That(completeEvents, Is.EqualTo(1));
            Assert.That(skill.Cooldown.IsRunning, Is.True);
            Assert.That(skill.CanExecute(), Is.False);

            skill.Execute();
            skill.Update(0.5f);
            skill.Execute();

            Assert.That(startEvents, Is.EqualTo(1), "Execution must remain blocked while cooldown is active.");

            skill.Update(0.51f);

            Assert.That(skill.Cooldown.IsRunning, Is.False);
            Assert.That(skill.CanExecute(), Is.True);

            skill.Execute();

            Assert.That(startEvents, Is.EqualTo(2));
        }

        [Test]
        public void Cancel_DuringExecution_EmitsEventAndStopsEffectsAndUpdates()
        {
            TestSkill skill = CreateSkill(2f);
            int cancelEvents = 0;
            int completeEvents = 0;
            skill.OnCancel += () =>
            {
                cancelEvents++;
                skill.RemoveEffect();
            };
            skill.OnComplete += () => completeEvents++;

            skill.Execute();
            skill.Update(0.1f);

            Assert.That(skill.HasEffect, Is.True);
            int updatesBeforeCancel = skill.UpdateCount;

            skill.Cancel();
            skill.Update(1f);

            Assert.That(cancelEvents, Is.EqualTo(1));
            Assert.That(completeEvents, Is.Zero);
            Assert.That(skill.HasEffect, Is.False);
            Assert.That(skill.UpdateCount, Is.EqualTo(updatesBeforeCancel));
            Assert.That(skill.Cooldown.IsRunning, Is.False);
            Assert.That(m_timerManager.TimersCount, Is.Zero);
        }

        [Test]
        public void CompletedSkill_RepeatedUpdates_DoNotCompleteOrApplyEffectsAgain()
        {
            TestSkill skill = CreateSkill(2f);
            int completeEvents = 0;
            skill.OnComplete += () => completeEvents++;
            skill.CompleteOnNextUpdate = true;

            skill.Execute();
            skill.Update(0.1f);
            skill.Update(0.1f);
            skill.Update(0.1f);

            Assert.That(completeEvents, Is.EqualTo(1));
            Assert.That(skill.UpdateCount, Is.EqualTo(1));
            Assert.That(skill.EffectApplications, Is.EqualTo(1));
            Assert.That(skill.Cooldown.IsRunning, Is.True);
            Assert.That(m_timerManager.TimersCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_DuringExecution_CancelsEffectsAndStopsSkillUpdates()
        {
            TestSkill skill = CreateSkill(2f);
            int cancelEvents = 0;
            skill.OnCancel += () =>
            {
                cancelEvents++;
                skill.RemoveEffect();
            };
            skill.Execute();
            skill.Update(0.1f);
            int updatesBeforeDestroy = skill.UpdateCount;
            Assert.That(skill.HasEffect, Is.True);

            skill.Dispose();
            skill.Update(1f);

            Assert.That(cancelEvents, Is.EqualTo(1));
            Assert.That(skill.IsDisposed, Is.True);
            Assert.That(skill.HasEffect, Is.False);
            Assert.That(skill.UpdateCount, Is.EqualTo(updatesBeforeDestroy));
            Assert.That(skill.Cooldown.IsRunning, Is.False);
            Assert.That(m_timerManager.TimersCount, Is.Zero);
        }

        [Test]
        public void Cooldowns_BelongToEachSkillInstance()
        {
            TestSkill shortCooldownSkill = CreateSkill(1f);
            TestSkill longCooldownSkill = CreateSkill(3f);
            shortCooldownSkill.CompleteOnNextUpdate = true;
            longCooldownSkill.CompleteOnNextUpdate = true;

            shortCooldownSkill.Execute();
            shortCooldownSkill.Update(0f);
            longCooldownSkill.Execute();
            longCooldownSkill.Update(0f);

            shortCooldownSkill.Update(1.1f);

            Assert.That(shortCooldownSkill.Cooldown.IsRunning, Is.False);
            Assert.That(longCooldownSkill.Cooldown.IsRunning, Is.True);
            Assert.That(shortCooldownSkill.CanExecute(), Is.True);
            Assert.That(longCooldownSkill.CanExecute(), Is.False);
        }

        [Test]
        public void Dispose_CanBeCalledRepeatedly_AndUnregistersTimerOnce()
        {
            TestSkill skill = CreateSkill(2f);
            skill.CompleteOnNextUpdate = true;
            skill.Execute();
            skill.Update(0f);
            Assert.That(m_timerManager.TimersCount, Is.EqualTo(1));

            skill.Dispose();
            skill.Dispose();

            Assert.That(m_timerManager.TimersCount, Is.Zero);
        }

        private TestSkill CreateSkill(float cooldownTime)
        {
            TestSkill skill = new TestSkill(m_timerManager, cooldownTime);
            m_createdSkills.Add(skill);
            return skill;
        }

        private sealed class TestSkill : Skill
        {
            public bool CanExecuteCondition { get; set; } = true;
            public bool CompleteOnNextUpdate { get; set; }
            public int UpdateCount { get; private set; }
            public int EffectApplications { get; private set; }
            public bool HasEffect { get; private set; }
            public override SkillType SkillType => SurvivalShooter.SkillType.Dash;
            private bool m_completedUpdate;

            public TestSkill(TimerManager timerManager, float cooldownTime)
                : base(timerManager, cooldownTime)
            {
            }

            public override bool CanExecute()
            {
                return CanExecuteCondition && base.CanExecute();
            }

            protected override bool IsCompleted()
            {
                return m_completedUpdate;
            }

            protected override void HandleUpdate(float deltaTime)
            {
                UpdateCount++;
                EffectApplications++;
                HasEffect = true;
                m_completedUpdate = CompleteOnNextUpdate;
                CompleteOnNextUpdate = false;
            }

            public override void ResolveConflict(Skill other)
            {
            }

            public void RemoveEffect()
            {
                HasEffect = false;
            }
        }
    }
}
