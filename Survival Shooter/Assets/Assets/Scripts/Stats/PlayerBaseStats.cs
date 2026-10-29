using UnityEngine;

namespace SurvivalShooter.Stats
{
    [CreateAssetMenu(fileName = "PlayerBaseStats", menuName = "Scriptable Objects/PlayerBaseStats")]
    public class PlayerBaseStats : BaseStats
    {
        public PlayerExperienceLevel[] XpLevels;
    }
}
