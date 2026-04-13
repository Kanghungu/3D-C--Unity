using Game.Audio;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 유닛 사망 등 전투 이벤트에 프로시저럴 효과음 연결.
    /// </summary>
    public sealed class BattleAcesCombatAudio : MonoBehaviour
    {
        private const float EnemyClusterWindowUnscaled = 0.55f;

        private const int EnemyClusterCountTrigger = 3;

        private static int enemyDeathsInWindow;

        private static float enemyDeathWindowResetUnscaled = -999f;

        private void OnEnable()
        {
            enemyDeathsInWindow = 0;
            enemyDeathWindowResetUnscaled = -999f;
            UnitHealth.OnUnitDied += HandleUnitDied;
        }

        private void OnDisable()
        {
            UnitHealth.OnUnitDied -= HandleUnitDied;
        }

        private static void HandleUnitDied(UnitTeam team, UnitArchetype archetype)
        {
            ProceduralAudioUtility.PlayUnitDeath(team);
            if (team == UnitTeam.Enemy)
            {
                ProceduralAudioUtility.PlayEnemyUnitDeathAccent();
                float now = Time.unscaledTime;
                if (now > enemyDeathWindowResetUnscaled)
                {
                    enemyDeathsInWindow = 0;
                }

                enemyDeathsInWindow++;
                enemyDeathWindowResetUnscaled = now + EnemyClusterWindowUnscaled;
                if (enemyDeathsInWindow >= EnemyClusterCountTrigger)
                {
                    ProceduralAudioUtility.PlayEnemyClusterDeathSweep();
                    enemyDeathsInWindow = 0;
                    enemyDeathWindowResetUnscaled = now + 0.85f;
                }
            }
        }
    }
}
