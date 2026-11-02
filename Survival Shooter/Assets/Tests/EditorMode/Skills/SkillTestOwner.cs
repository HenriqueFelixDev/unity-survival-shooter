using SurvivalShooter;

namespace EditorModeTests.Skills
{
    public sealed class SkillTestOwner
    {
        private Skill m_skill;

        public bool OwnsSkill => m_skill != null;

        public void Own(Skill skill)
        {
            m_skill = skill;
        }

        public void Destroy()
        {
            if (m_skill == null)
                return;

            Skill skill = m_skill;
            m_skill = null;
            skill.Cancel();
            skill.Dispose();
        }
    }
}
