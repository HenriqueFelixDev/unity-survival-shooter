using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats;
using UnityEngine;

namespace SurvivalShooter.Characters
{
    public class Player : Character, IVisitable
    {
        [SerializeField] private PlayerBaseStats baseStats;
        [SerializeField] private Transform aim;
        [SerializeField] private LayerMask shootTargetLayer;

        private CharacterExperience m_experience;

        private void Awake()
        {
            Initialize(baseStats);
            m_experience = new CharacterExperience(baseStats);

            SkillManager.Initialize(Health);

            SkillManager.AddSkill(new ShootSkill(
                aim,
                shootTargetLayer,
                this,
                TimerManagerComponent.Instance.TimerManager));
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public CharacterExperience Experience
        {
            get
            {
                return m_experience;
            }
        }

        protected override void TakeDamage(Character character)
        {
            if (character is Enemy enemy)
            {
                Health.ApplyDamage(enemy.AttackDamage);
            }
        }

        public void UseSkill()
        {
            SkillManager.Activate(SkillType.Shoot);
        }

        private void Update()
        {
            SkillManager.Update(Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (SkillManager.TryGetSkill(SkillType.Shoot, out ShootSkill shootSkill))
            {
                shootSkill.DebugDrawRay();
            }
        }
    }
}
