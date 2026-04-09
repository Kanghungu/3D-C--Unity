using Game.Campaign.Data;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// ClassicDuel 한 판(평지+데모 무대) — 수입·적 AI 집결/물결을 맵과 맞춤.
    /// 목표: 약 10~15분 전후 한 판, 코어↔중앙 왕복이 덜 막히게.
    /// </summary>
    public static class BattleAcesClassicDuelArenaTuning
    {
        /// <summary>미션별 수입 튜닝 뒤에 호출 — ClassicDuel 만 추가 배율</summary>
        public static void ApplyEconomyForLayout(
            BattleAcesEconomy economy,
            BattleArenaLayoutKind layoutKind,
            MissionDefinition mission)
        {
            if (economy == null || layoutKind != BattleArenaLayoutKind.ClassicDuel)
            {
                return;
            }

            // 스커미시는 ApplyMissionIncomeTuning 에서 이미 배율 적용 — 여기선 맵 보정만 약하게
            float playerMul = 1.042f;
            float enemyMul = 0.978f;
            string mid = mission != null ? (mission.MissionId ?? string.Empty) : string.Empty;
            if (mid.StartsWith("skirmish_vs_ai", System.StringComparison.Ordinal))
            {
                playerMul = 1.024f;
                enemyMul = 0.988f;
            }

            // 미러 변주: 시야·측면 기동이 길어질 수 있어 플레이어 수입만 아주 소폭
            if (mission != null && mission.MirroredLayoutVariant)
            {
                playerMul *= 1.012f;
            }

            economy.ApplyIncomeMultipliers(playerMul, enemyMul);
        }

        /// <summary>ApplyEnemyPattern 직후 호출</summary>
        public static void ApplyEnemyBrainForLayout(
            BattleAcesEnemyBrain brain,
            BattleArenaLayoutKind layoutKind,
            bool mirroredLayout)
        {
            if (brain == null || layoutKind != BattleArenaLayoutKind.ClassicDuel)
            {
                return;
            }

            brain.ApplyClassicDuelStageLayoutModifiers(mirroredLayout);
        }
    }
}
