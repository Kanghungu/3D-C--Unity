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
        private void OnEnable()
        {
            UnitHealth.OnUnitDied += HandleUnitDied;
        }

        private void OnDisable()
        {
            UnitHealth.OnUnitDied -= HandleUnitDied;
        }

        private static void HandleUnitDied(UnitTeam team, UnitArchetype archetype)
        {
            ProceduralAudioUtility.PlayUnitDeath(team);
        }
    }
}
