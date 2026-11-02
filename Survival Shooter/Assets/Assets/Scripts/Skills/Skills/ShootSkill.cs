using SurvivalShooter.Core.Timers;
using UnityEngine;

namespace SurvivalShooter
{
    public class ShootSkill : Skill
    {
        private readonly Transform m_aim;
        private readonly LayerMask m_targetLayer;
        private readonly Character m_owner;

        private const float attackRange = 100f;

        public ShootSkill(
            Transform aim,
            LayerMask targetLayer,
            Character owner,
            TimerManager timerManager) 
            : base(timerManager, CalculateCooldownTime(owner.AttackSpeed))
        {
            m_aim = aim;
            m_targetLayer = targetLayer;
            m_owner = owner;

            Cooldown.OnFinish += Cooldown_OnFinish;
        }

        private void Cooldown_OnFinish()
        {
            Cooldown.Reset(CalculateCooldownTime(m_owner.AttackSpeed));
        }

        private static float CalculateCooldownTime(float attackSpeed)
        {
            return 1 / attackSpeed;
        }

        public override SkillType SkillType => SkillType.Shoot;

        public override void ResolveConflict(Skill other) { }

        protected override void HandleUpdate(float deltaTime)
        {
            if (ShootHitTarget(out Character target))
            {
                m_owner.ApplyDamage(target);
            }
        }

        protected override bool IsCompleted()
        {
            return true;
        }

        private bool ShootHitTarget(out Character target)
        {
            if (Physics.Raycast(
                m_aim.position,
                m_aim.forward,
                out RaycastHit hitInfo,
                attackRange,
                m_targetLayer))
            {

                return hitInfo.collider.TryGetComponent(out target);
            }

            target = null;
            return false;
        }

        public Vector3 GetShootLimitPosition()
        {
            return m_aim.forward * attackRange;
        }

        public void DebugDrawRay()
        {
            Debug.DrawRay(m_aim.position, GetShootLimitPosition(), Color.red);
        }
    }
}
