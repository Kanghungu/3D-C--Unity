using Game.Campaign.Data;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 미션 보조 목표(선택) — 승리 시에만 판정. 실패해도 클리어는 유지.
    /// </summary>
    public static class BattleAcesMissionBonusEvaluator
    {
        public static bool Evaluate(MissionDefinition mission, BattleAcesCore playerCore, BattleAcesRunStats stats)
        {
            if (mission == null)
            {
                return false;
            }

            string id = mission.OptionalBonusObjectiveId;
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            switch (id)
            {
                case "core_survive_50":
                    return playerCore != null &&
                           playerCore.Health != null &&
                           playerCore.Health.Normalized >= 0.499f;
                case "train_variety_3":
                    return stats != null && stats.CountDistinctPlayerProductionArchetypes() >= 3;
                default:
                    Debug.LogWarning("[BattleAces] 알 수 없는 보조 목표 ID: " + id);
                    return false;
            }
        }

        public static string DescribeBonusForUi(string id)
        {
            return id switch
            {
                "core_survive_50" => "아군 코어 체력 50% 이상으로 승리",
                "train_variety_3" => "서로 다른 병과 3종 이상 생산 후 승리",
                _ => "특수 목표"
            };
        }
    }
}
