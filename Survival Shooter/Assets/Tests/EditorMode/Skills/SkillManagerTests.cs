using System.Collections.Generic;
using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats;
using UnityEngine;

namespace EditorModeTests.Skills
{
    public class SkillManagerTests
    {
        private TimerManager m_timerManager;
        private PlayerBaseStats m_baseStats;
        private Health m_owner;
        private SkillManager m_skillManager;
        private readonly List<TestSkill> m_createdSkills = new();

        [SetUp]
        public void SetUp()
        {
            m_timerManager = new TimerManager();
            m_createdSkills.Clear();

            m_baseStats = ScriptableObject.CreateInstance<PlayerBaseStats>();
            m_baseStats.MaxHealth = 10f;
            m_owner = new Health(m_baseStats, new StatMediator());
            m_skillManager = new SkillManager();
            m_skillManager.Initialize(m_owner);
        }

        [TearDown]
        public void TearDown()
        {
            m_skillManager.Dispose();

            foreach (TestSkill skill in m_createdSkills)
                skill.Dispose();

            m_createdSkills.Clear();
            Object.DestroyImmediate(m_baseStats);
        }

        [Test]
        public void AddSkill_RegistersOneSkillPerTypeAndResolvesConflicts()
        {
            TestSkill dash = CreateSkill(SkillType.Dash);
            TestSkill replacementDash = CreateSkill(SkillType.Dash);
            TestSkill shoot = CreateSkill(SkillType.Shoot);

            m_skillManager.AddSkill(dash);
            m_skillManager.AddSkill(replacementDash);
            m_skillManager.AddSkill(shoot);

            Assert.That(m_skillManager.SkillCount, Is.EqualTo(2));
            Assert.That(m_skillManager.Skills.Count, Is.EqualTo(2));
            Assert.That(m_skillManager.GetSkill(SkillType.Dash), Is.SameAs(dash));
            Assert.That(m_skillManager.TryGetSkill(SkillType.Shoot, out Skill registeredShoot), Is.True);
            Assert.That(registeredShoot, Is.SameAs(shoot));
            Assert.That(m_skillManager.TryGetSkill(SkillType.Root, out Skill missingSkill), Is.False);
            Assert.That(missingSkill, Is.Null);
            Assert.That(m_skillManager.GetSkill(SkillType.Root), Is.Null);
            Assert.That(dash.ConflictCount, Is.EqualTo(1));
            Assert.That(dash.LastConflict, Is.SameAs(replacementDash));
        }

        [Test]
        public void AddSkill_IgnoresNullDisposedSkillsAndSkillsAddedAfterDisposal()
        {
            TestSkill disposedSkill = CreateSkill(SkillType.Dash);
            disposedSkill.Dispose();

            m_skillManager.AddSkill(null);
            m_skillManager.AddSkill(disposedSkill);

            Assert.That(m_skillManager.SkillCount, Is.Zero);

            m_skillManager.Dispose();
            m_skillManager.AddSkill(CreateSkill(SkillType.Shoot));

            Assert.That(m_skillManager.SkillCount, Is.Zero);
        }

        [Test]
        public void Activate_RequiresRegistrationAndSkillExecutionConditions()
        {
            TestSkill skill = CreateSkill(SkillType.Dash);
            int startCount = 0;
            skill.OnStart += () => startCount++;

            m_skillManager.Activate(SkillType.Dash);
            Assert.That(startCount, Is.Zero);

            m_skillManager.AddSkill(skill);
            skill.CanExecuteCondition = false;
            m_skillManager.Activate(SkillType.Dash);
            Assert.That(startCount, Is.Zero);

            skill.CanExecuteCondition = true;
            m_skillManager.Activate(SkillType.Dash);
            m_skillManager.Activate(SkillType.Dash);

            Assert.That(startCount, Is.EqualTo(1));
            Assert.That(skill.IsExecuting, Is.True);
        }

        [Test]
        public void Update_UpdatesSkillsAndAdvancesEachSkillCooldown()
        {
            TestSkill shortCooldown = CreateSkill(SkillType.Dash, 1f);
            TestSkill longCooldown = CreateSkill(SkillType.Shoot, 3f);
            shortCooldown.CompleteOnNextUpdate = true;
            longCooldown.CompleteOnNextUpdate = true;
            m_skillManager.AddSkill(shortCooldown);
            m_skillManager.AddSkill(longCooldown);

            m_skillManager.Activate(SkillType.Dash);
            m_skillManager.Update(0f);
            m_skillManager.Activate(SkillType.Shoot);
            m_skillManager.Update(0f);

            m_skillManager.Update(1.1f);

            Assert.That(shortCooldown.UpdateCount, Is.EqualTo(1));
            Assert.That(longCooldown.UpdateCount, Is.EqualTo(1));
            Assert.That(shortCooldown.Cooldown.IsRunning, Is.False);
            Assert.That(longCooldown.Cooldown.IsRunning, Is.True);
            Assert.That(shortCooldown.CanExecute(), Is.True);
            Assert.That(longCooldown.CanExecute(), Is.False);
        }

        [Test]
        public void RemoveSkill_DisposesSkillAndStopsUpdatingIt()
        {
            TestSkill skill = CreateSkill(SkillType.Dash);
            int cancelCount = 0;
            skill.OnCancel += () => cancelCount++;
            m_skillManager.AddSkill(skill);
            m_skillManager.Activate(SkillType.Dash);
            m_skillManager.Update(0.1f);
            int updateCount = skill.UpdateCount;

            Assert.That(m_skillManager.RemoveSkill(SkillType.Dash), Is.True);
            m_skillManager.Update(1f);

            Assert.That(cancelCount, Is.EqualTo(1));
            Assert.That(skill.IsDisposed, Is.True);
            Assert.That(skill.UpdateCount, Is.EqualTo(updateCount));
            Assert.That(m_skillManager.SkillCount, Is.Zero);
            Assert.That(m_skillManager.RemoveSkill(SkillType.Dash), Is.False);
        }

        [Test]
        public void OwnerDeath_CancelsEveryRegisteredSkill()
        {
            TestSkill dash = CreateSkill(SkillType.Dash);
            TestSkill shoot = CreateSkill(SkillType.Shoot);
            int dashCancelCount = 0;
            int shootCancelCount = 0;
            dash.OnCancel += () => dashCancelCount++;
            shoot.OnCancel += () => shootCancelCount++;
            m_skillManager.AddSkill(dash);
            m_skillManager.AddSkill(shoot);
            m_skillManager.Enable();
            m_skillManager.Activate(SkillType.Dash);
            m_skillManager.Activate(SkillType.Shoot);

            m_owner.ApplyDamage(10f);
            m_skillManager.Update(0.1f);

            Assert.That(dashCancelCount, Is.EqualTo(1));
            Assert.That(shootCancelCount, Is.EqualTo(1));
            Assert.That(dash.IsExecuting, Is.False);
            Assert.That(shoot.IsExecuting, Is.False);
            Assert.That(dash.UpdateCount, Is.Zero);
            Assert.That(shoot.UpdateCount, Is.Zero);
        }

        [Test]
        public void Disable_CancelsSkillsAndUnsubscribesFromOwnerDeath()
        {
            TestSkill skill = CreateSkill(SkillType.Dash);
            int cancelCount = 0;
            skill.OnCancel += () => cancelCount++;
            m_skillManager.AddSkill(skill);
            m_skillManager.Enable();
            m_skillManager.Activate(SkillType.Dash);

            m_skillManager.Disable();
            m_owner.ApplyDamage(10f);

            Assert.That(cancelCount, Is.EqualTo(1));
            Assert.That(skill.IsExecuting, Is.False);
        }

        [Test]
        public void Dispose_DisposesAllSkillsAndUnregistersTheirCooldowns()
        {
            TestSkill skill = CreateSkill(SkillType.Dash, 2f);
            skill.CompleteOnNextUpdate = true;
            m_skillManager.AddSkill(skill);
            m_skillManager.Activate(SkillType.Dash);
            m_skillManager.Update(0f);
            Assert.That(m_timerManager.TimersCount, Is.EqualTo(1));

            m_skillManager.Dispose();
            m_skillManager.Dispose();
            m_skillManager.Update(1f);

            Assert.That(skill.IsDisposed, Is.True);
            Assert.That(skill.UpdateCount, Is.EqualTo(1));
            Assert.That(m_skillManager.SkillCount, Is.Zero);
            Assert.That(m_timerManager.TimersCount, Is.Zero);
        }

        private TestSkill CreateSkill(SkillType skillType, float cooldown = 0f)
        {
            TestSkill skill = new TestSkill(m_timerManager, cooldown, skillType);
            m_createdSkills.Add(skill);
            return skill;
        }

        private sealed class TestSkill : Skill
        {
            public bool CanExecuteCondition { get; set; } = true;
            public bool CompleteOnNextUpdate { get; set; }
            public int UpdateCount { get; private set; }
            public int ConflictCount { get; private set; }
            public Skill LastConflict { get; private set; }
            public override SkillType SkillType { get; }

            private bool m_completed;

            public TestSkill(TimerManager timerManager, float cooldownTime, SkillType skillType)
                : base(timerManager, cooldownTime)
            {
                SkillType = skillType;
            }

            public override bool CanExecute()
            {
                return CanExecuteCondition && base.CanExecute();
            }

            protected override bool IsCompleted()
            {
                return m_completed;
            }

            protected override void HandleUpdate(float deltaTime)
            {
                UpdateCount++;
                m_completed = CompleteOnNextUpdate;
                CompleteOnNextUpdate = false;
            }

            public override void ResolveConflict(Skill other)
            {
                ConflictCount++;
                LastConflict = other;
            }
        }
    }
}
