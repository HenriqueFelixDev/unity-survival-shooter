using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalShooter
{
    /// <summary>
    /// Registers and updates the skills owned by one entity.
    /// </summary>
    public sealed class SkillManager : IDisposable
    {
        [SerializeField] private Health m_owner;

        private readonly Dictionary<SkillType, Skill> m_skillsDictionary = new();
        private bool m_isDestroying;

        public int SkillCount => m_skillsDictionary.Count;

        public IReadOnlyCollection<Skill> Skills => m_skillsDictionary.Values;

        public Health Owner => m_owner;

        internal void Initialize(Health owner)
        {
            m_owner = owner;
        }

        public void Enable()
        {
            SubscribeToOwnerHealth();
        }

        public void Disable()
        {
            UnsubscribeFromOwnerHealth();
            CancelAllSkills();
        }

        public void Dispose()
        {
            m_isDestroying = true;
            ReleaseAllSkills();
        }

        public void Update(float deltaTime)
        {
            foreach (Skill skill in m_skillsDictionary.Values)
                skill.Update(deltaTime);
        }

        public void Activate(SkillType skillType)
        {
            if (TryGetSkill(skillType, out Skill skill))
            {
                skill.Execute();
            }
        }

        public void AddSkill(Skill skill)
        {
            if (skill == null || skill.IsDisposed || m_isDestroying)
                return;

            if (TryGetSkill(skill.SkillType, out Skill existingSkill))
            {
                existingSkill.ResolveConflict(skill);
                return;
            }

            m_skillsDictionary.Add(skill.SkillType, skill);
            skill.SetSkillManager(this);
        }

        public bool RemoveSkill(SkillType skillType)
        {
            if (TryGetSkill(skillType, out Skill existingSkill))
            {
                m_skillsDictionary.Remove(existingSkill.SkillType);
                ReleaseSkill(existingSkill);

                return true;
            }

            return false;
        }

        public Skill GetSkill(SkillType skillType)
        {
            m_skillsDictionary.TryGetValue(skillType, out Skill skill);

            return skill;
        }

        public bool TryGetSkill<T>(SkillType skillType, out T skill) where T : Skill
        {
            if (m_skillsDictionary.TryGetValue(skillType, out Skill foundSkill))
            {
                skill = foundSkill as T;
                return skill != null;
            }

            skill = null;
            return false;
        }

        private void SubscribeToOwnerHealth()
        {
            if (m_owner != null)
            {
                m_owner.OnDie += OwnerHealth_OnDie;
            }
        }

        private void UnsubscribeFromOwnerHealth()
        {
            if (m_owner != null)
            {
                m_owner.OnDie -= OwnerHealth_OnDie;
            }
        }

        private void OwnerHealth_OnDie(object sender, EventArgs e)
        {
            CancelAllSkills();
        }

        private void CancelAllSkills()
        {
            foreach (Skill skill in m_skillsDictionary.Values)
                skill.Cancel();
        }

        private void ReleaseAllSkills()
        {
            foreach (Skill skill in m_skillsDictionary.Values)
                ReleaseSkill(skill);

            m_skillsDictionary.Clear();
        }

        private void ReleaseSkill(Skill skill)
        {
            skill.Dispose();
        }
    }
}
