using UnityEngine;

namespace SurvivalShooter.Stats
{
    [CreateAssetMenu(fileName = "EnemyBaseStats", menuName = "Scriptable Objects/EnemyBaseStats")]
    public class EnemyBaseStats : BaseStats
    {
        public int RequiredPlayerLevel;
        public float XpGranted;
        public float MinimumSpawnInterval;
        public float MaximumSpawnInterval;
    }
}
