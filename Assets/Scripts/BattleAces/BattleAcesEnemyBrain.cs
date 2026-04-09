using System.Collections.Generic;
using Game.Prototype;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 적 코어 주기적 생산 + 집결지 근처 병력이 모이면 한꺼번에 공격 이동(한타 느낌).
    /// </summary>
    public class BattleAcesEnemyBrain : MonoBehaviour
    {
        [SerializeField] private BattleAcesCore enemyCore;
        [SerializeField] private BattleAcesMatchController match;
        [SerializeField] private float thinkInterval = 19f;

        /// <summary>0~1 — 높을수록 저코스트(앞 슬롯 0~3) 생산 비중</summary>
        private float earlySlotBias = 0.5f;

        private float productionTimer;

        /// <summary>물결 공격 타이머(생산 주기와 별도)</summary>
        private float attackWaveTimer = 9f;

        private float attackWaveInterval = 14f;

        /// <summary>집결 반경 내 유닛이 이 이상이면(또는 오래 기다렸으면) 전진 — 기본 약 5기 묶음</summary>
        private int minUnitsForAttackWave = 5;

        private float gatherRadius = 62f;

        /// <summary>마지막 물결 이후 너무 오래 대기 시 낮은 인원으로라도 출격(초) — 패턴별 조정</summary>
        private float forceWaveAfterSeconds = 40f;

        /// <summary>강제 출격 시 최소 인원 — 패턴별 조정</summary>
        private int forcedWaveMinUnits = 4;

        private float lastAttackWaveTime = -200f;

        public void Bind(BattleAcesCore core, BattleAcesMatchController matchController)
        {
            enemyCore = core;
            match = matchController;
        }

        public void SetThinkIntervalBase(float seconds)
        {
            thinkInterval = Mathf.Max(4f, seconds);
        }

        /// <summary>MissionDefinition.enemyPatternId — 생산 간격·슬롯 가중·집결/물결 파라미터.</summary>
        public void ApplyEnemyPattern(string patternId, float baseIntervalSeconds)
        {
            float interval = Mathf.Max(4f, baseIntervalSeconds);
            earlySlotBias = 0.5f;
            minUnitsForAttackWave = 5;
            gatherRadius = 62f;
            attackWaveInterval = 14f;
            forceWaveAfterSeconds = 40f;
            forcedWaveMinUnits = 4;

            if (string.IsNullOrEmpty(patternId))
            {
                patternId = "default_skirmish";
            }

            switch (patternId)
            {
                case "aggressive_push":
                    interval *= 0.76f;
                    earlySlotBias = 0.7f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 11f;
                    gatherRadius = 58f;
                    break;
                case "defensive_turtle":
                    interval *= 1.22f;
                    earlySlotBias = 0.38f;
                    minUnitsForAttackWave = 7;
                    attackWaveInterval = 17f;
                    gatherRadius = 72f;
                    break;
                case "elite_mix":
                    interval *= 1.06f;
                    earlySlotBias = 0.2f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 15f;
                    break;
                case "airborne_siege":
                    interval *= 0.84f;
                    earlySlotBias = 0.18f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 12f;
                    break;
                case "escort_harass":
                    interval *= 0.79f;
                    earlySlotBias = 0.63f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 11.5f;
                    break;
                case "seize_pressure":
                    interval *= 0.73f;
                    earlySlotBias = 0.68f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 10.5f;
                    break;
                case "heresy_breach":
                    interval *= 1.02f;
                    earlySlotBias = 0.23f;
                    minUnitsForAttackWave = 6;
                    attackWaveInterval = 15f;
                    break;
                // 데모 챕터 4(이단 거점) — 8~15분 전후 한 판, 물결이 잦고 막히지 않게
                case "heresy_demo_pace":
                    interval *= 0.9f;
                    earlySlotBias = 0.4f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 10.5f;
                    gatherRadius = 59f;
                    forceWaveAfterSeconds = 32f;
                    forcedWaveMinUnits = 4;
                    break;
                case "fortress_break":
                    interval *= 0.9f;
                    earlySlotBias = 0.25f;
                    minUnitsForAttackWave = 5;
                    attackWaveInterval = 13f;
                    break;
                case "counter_rush":
                    interval *= 0.68f;
                    earlySlotBias = 0.78f;
                    minUnitsForAttackWave = 4;
                    attackWaveInterval = 9.5f;
                    gatherRadius = 56f;
                    break;
                default:
                    break;
            }

            thinkInterval = interval;
            attackWaveTimer = Mathf.Min(8f, attackWaveInterval * 0.55f);
        }

        /// <summary>
        /// ClassicDuel + 데모 지형 — 집결 반경·물결 주기를 넓혀 유닛이 덜 뭉치고 출격이 막히지 않게.
        /// 미러 레이아웃은 좌우 반전으로 측면 이동이 길어질 수 있어 집결 판정만 소폭 확대.
        /// </summary>
        public void ApplyClassicDuelStageLayoutModifiers(bool mirroredLayout)
        {
            gatherRadius = Mathf.Min(86f, gatherRadius + 14f);
            attackWaveInterval *= 0.91f;
            attackWaveTimer = Mathf.Min(attackWaveTimer, attackWaveInterval * 0.5f);
            forceWaveAfterSeconds = Mathf.Max(20f, forceWaveAfterSeconds - 12f);
            minUnitsForAttackWave = Mathf.Max(4, minUnitsForAttackWave - 1);
            forcedWaveMinUnits = Mathf.Max(3, forcedWaveMinUnits - 1);

            if (mirroredLayout)
            {
                gatherRadius = Mathf.Min(90f, gatherRadius + 8f);
            }
        }

        private void Update()
        {
            if (match != null && match.IsFinished)
            {
                return;
            }

            if (enemyCore == null)
            {
                return;
            }

            attackWaveTimer -= Time.deltaTime;
            if (attackWaveTimer <= 0f)
            {
                attackWaveTimer = attackWaveInterval + Random.Range(-2f, 2.8f);
                TryIssueGroupedAttackWave();
            }

            productionTimer -= Time.deltaTime;
            if (productionTimer > 0f)
            {
                return;
            }

            productionTimer = thinkInterval + Random.Range(-1.5f, 2f);
            int slot = Random.value < earlySlotBias ? Random.Range(0, 4) : Random.Range(4, 8);
            enemyCore.TryEnqueueDeckSlot(slot);
        }

        /// <summary>기지·집결 반경 안 적만 모아 적 코어 방향으로 공격 이동</summary>
        private void TryIssueGroupedAttackWave()
        {
            if (match == null || match.IsFinished || enemyCore == null || match.PlayerCore == null)
            {
                return;
            }

            UnitHealth playerH = match.PlayerCore.Health;
            if (playerH == null || !playerH.IsAlive)
            {
                return;
            }

            Vector3 corePos = enemyCore.transform.position;
            Vector3 rallyPos = enemyCore.RallyWorldPosition;
            float r = gatherRadius;
            float rSq = r * r;

            List<SelectableUnit> staging = new List<SelectableUnit>(16);

            foreach (SelectableUnit u in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (u == null || u.Team != UnitTeam.Enemy)
                {
                    continue;
                }

                CombatTarget ct = u.GetComponent<CombatTarget>();
                if (ct == null || !ct.IsAlive)
                {
                    continue;
                }

                Vector3 p = u.transform.position;
                float dxz = HorizontalSqrDistance(p, corePos);
                float dxzR = HorizontalSqrDistance(p, rallyPos);
                if (dxz <= rSq || dxzR <= rSq)
                {
                    staging.Add(u);
                }
            }

            int need = minUnitsForAttackWave;
            if (staging.Count >= forcedWaveMinUnits && Time.time - lastAttackWaveTime >= forceWaveAfterSeconds)
            {
                need = Mathf.Min(need, forcedWaveMinUnits);
            }

            if (staging.Count < need)
            {
                return;
            }

            lastAttackWaveTime = Time.time;
            Vector3 pushTarget = match.PlayerCore.transform.position;

            for (int i = 0; i < staging.Count; i++)
            {
                SelectableUnit unit = staging[i];
                if (unit != null)
                {
                    unit.AttackMoveTo(pushTarget);
                }
            }
        }

        private static float HorizontalSqrDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }
    }
}
