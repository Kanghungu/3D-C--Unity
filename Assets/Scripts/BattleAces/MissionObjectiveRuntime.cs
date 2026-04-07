using Game.Audio;
using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 미션 종류별 승리 조건(코어 파괴 외). 브리핑 종료 후부터 판정한다.
    /// </summary>
    public sealed class MissionObjectiveRuntime : MonoBehaviour
    {
        private MissionDefinition mission;
        private BattleAcesMatchController match;
        private BattleAcesCore playerCore;
        private RelicMissionObject relic;
        private MissionCaptureZone captureZone;
        private CombatTarget heresyTarget;
        private CampaignBattleFlow flow;

        private bool evacReachedEscortCheckpoint;

        private bool warnedSanctuary10Sec;
        private bool warnedEscortNear;
        private bool warnedSeizeAlmost;
        private bool warnedHeresyLow;
        private bool warnedRecoverNearEscort;

        public void Initialize(
            MissionDefinition def,
            BattleAcesMatchController m,
            BattleAcesCore player,
            RelicMissionObject relicObj,
            MissionCaptureZone capture,
            CombatTarget heresy,
            CampaignBattleFlow battleFlow)
        {
            mission = def;
            match = m;
            playerCore = player;
            relic = relicObj;
            captureZone = capture;
            heresyTarget = heresy;
            flow = battleFlow;

            if (mission == null || match == null)
            {
                return;
            }

            if (mission.ObjectiveKind == MissionObjectiveKind.DestroyEnemyCore)
            {
                match.SetVictoryMode(BattleAcesMatchController.VictoryMode.EnemyCoreDestroyed);
            }
            else
            {
                match.SetVictoryMode(BattleAcesMatchController.VictoryMode.MissionCustom);
            }
        }

        private void Update()
        {
            if (mission == null || match == null || match.IsFinished)
            {
                return;
            }

            if (flow != null && !flow.IsGameplayStarted)
            {
                return;
            }

            if (playerCore == null || playerCore.Health == null || !playerCore.Health.IsAlive)
            {
                return;
            }

            switch (mission.ObjectiveKind)
            {
                case MissionObjectiveKind.DestroyEnemyCore:
                    break;
                case MissionObjectiveKind.SanctuaryDefense:
                    UpdateSanctuaryDefense();
                    break;
                case MissionObjectiveKind.EscortRelic:
                    UpdateEscortRelic();
                    break;
                case MissionObjectiveKind.SeizeRelicOrNode:
                    UpdateSeize();
                    break;
                case MissionObjectiveKind.DestroyHeresyStronghold:
                    UpdateHeresy();
                    break;
                case MissionObjectiveKind.RecoverRelicAndEvacuate:
                    UpdateRecoverEvac();
                    break;
            }
        }

        private void UpdateSanctuaryDefense()
        {
            float start = flow != null ? flow.GameplayStartTime : 0f;
            float remain = mission.DefenseDurationSeconds - (Time.time - start);

            if (!warnedSanctuary10Sec && remain <= 10f && remain > 0.5f)
            {
                warnedSanctuary10Sec = true;
                PushImminentBanner("제한 시간 10초 이내 — 코어를 지키십시오.");
            }

            if (Time.time >= start + mission.DefenseDurationSeconds)
            {
                match.DeclarePlayerVictory();
            }
        }

        private void UpdateEscortRelic()
        {
            if (relic == null)
            {
                return;
            }

            if (relic.CombatTarget != null && !relic.CombatTarget.IsAlive)
            {
                match.DeclarePlayerDefeat(BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost);
                return;
            }

            TryWarnEscortNear();

            if (relic.EscortTargetZone != null && relic.IsInsideEscortZone(relic.transform.position))
            {
                match.DeclarePlayerVictory();
            }
        }

        private void UpdateSeize()
        {
            if (captureZone != null && !warnedSeizeAlmost && captureZone.HoldProgress01 >= 0.72f && !captureZone.IsCompleted)
            {
                warnedSeizeAlmost = true;
                PushImminentBanner("거점 점령 임박 — 구역 안을 유지하십시오.");
            }

            if (captureZone != null && captureZone.IsCompleted)
            {
                match.DeclarePlayerVictory();
            }
        }

        private void UpdateHeresy()
        {
            if (heresyTarget != null && heresyTarget.Health != null && !warnedHeresyLow)
            {
                if (heresyTarget.Health.Normalized <= 0.28f && heresyTarget.IsAlive)
                {
                    warnedHeresyLow = true;
                    PushImminentBanner("이단 본거지 붕괴 직전입니다.");
                }
            }

            if (heresyTarget == null || !heresyTarget.IsAlive)
            {
                match.DeclarePlayerVictory();
            }
        }

        private void UpdateRecoverEvac()
        {
            if (relic == null)
            {
                return;
            }

            if (relic.CombatTarget != null && !relic.CombatTarget.IsAlive)
            {
                match.DeclarePlayerDefeat(BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost);
                return;
            }

            Vector3 p = relic.transform.position;
            if (!evacReachedEscortCheckpoint && relic.EscortTargetZone != null && relic.IsInsideEscortZone(p))
            {
                evacReachedEscortCheckpoint = true;
            }

            if (!warnedRecoverNearEscort && relic.EscortTargetZone != null)
            {
                Vector3 c = relic.EscortTargetZone.bounds.center;
                float flat = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(c.x, c.z));
                if (flat < 16f)
                {
                    warnedRecoverNearEscort = true;
                    PushImminentBanner("호구 구역 근접 — 철수 지점으로 이어집니다.");
                }
            }

            if (evacReachedEscortCheckpoint && relic.EvacuationZone != null && relic.IsInsideEvacuationZone(p))
            {
                match.DeclarePlayerVictory();
            }
        }

        private void TryWarnEscortNear()
        {
            if (warnedEscortNear || relic == null || relic.EscortTargetZone == null)
            {
                return;
            }

            Vector3 p = relic.transform.position;
            Vector3 c = relic.EscortTargetZone.bounds.center;
            float flat = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(c.x, c.z));
            if (flat < 15f)
            {
                warnedEscortNear = true;
                PushImminentBanner("목표 구역 근접 — 성유물을 유도하십시오.");
            }
        }

        private static void PushImminentBanner(string line)
        {
            ProceduralAudioUtility.PlayObjectivePulse();
            if (BattleAcesStoryBanner.Instance != null)
            {
                BattleAcesStoryBanner.Instance.ShowLine(line, 5f);
            }
        }
    }
}
