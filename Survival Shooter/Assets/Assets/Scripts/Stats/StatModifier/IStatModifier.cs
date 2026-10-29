using System;

namespace SurvivalShooter.Stats.StatModifier
{
    public interface IStatModifier : IDisposable
    {
        public bool MarkToRemoval { get; }

        int GetPriority();

        public void Handle(object sender, Query query);

        public void Update(float deltaTime);
    }
}
