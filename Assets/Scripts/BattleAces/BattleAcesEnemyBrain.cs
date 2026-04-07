using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 적 코어가 주기적으로 덱 슬롯 중 하나를 무작위로 생산 시도한다.
    /// </summary>
    public class BattleAcesEnemyBrain : MonoBehaviour
    {
        [SerializeField] private BattleAcesCore enemyCore;
        [SerializeField] private BattleAcesMatchController match;
        [SerializeField] private float thinkInterval = 19f;

        /// <summary>0~1 — 높을수록 저코스트(앞 슬롯 0~3) 생산 비중</summary>
        private float earlySlotBias = 0.5f;

        private float timer;

        public void Bind(BattleAcesCore core, BattleAcesMatchController matchController)
        {
            enemyCore = core;
            match = matchController;
        }

        /// <summary>팩션 생산 배율에 맞춰 생산 시도 주기 조정(낮은 배율 = 빠른 생산 = 더 자주 시도)</summary>
        public void SetThinkIntervalBase(float seconds)
        {
            thinkInterval = Mathf.Max(4f, seconds);
        }

        /// <summary>
        /// MissionDefinition.enemyPatternId — 기본 간격(baseIntervalSeconds)에 곱하고 슬롯 가중만 조정.
        /// aggressive_push: 짧은 간격·저슬롯 비중↑ / elite_mix: 긴 간격·고슬롯(후반)↑ 로 체감 분리.
        /// </summary>
        public void ApplyEnemyPattern(string patternId, float baseIntervalSeconds)
        {
            float interval = Mathf.Max(4f, baseIntervalSeconds);
            earlySlotBias = 0.5f;

            if (string.IsNullOrEmpty(patternId))
            {
                patternId = "default_skirmish";
            }

            switch (patternId)
            {
                case "aggressive_push":
                    interval *= 0.76f;
                    earlySlotBias = 0.7f;
                    break;
                case "defensive_turtle":
                    interval *= 1.22f;
                    earlySlotBias = 0.38f;
                    break;
                case "elite_mix":
                    interval *= 1.06f;
                    earlySlotBias = 0.2f;
                    break;
                case "airborne_siege":
                    interval *= 0.84f;
                    earlySlotBias = 0.18f;
                    break;
                // 미션별 세분 — MissionDefinition.enemyPatternId
                case "escort_harass":
                    interval *= 0.79f;
                    earlySlotBias = 0.63f;
                    break;
                case "seize_pressure":
                    interval *= 0.73f;
                    earlySlotBias = 0.68f;
                    break;
                case "heresy_breach":
                    interval *= 1.02f;
                    earlySlotBias = 0.23f;
                    break;
                case "fortress_break":
                    interval *= 0.9f;
                    earlySlotBias = 0.25f;
                    break;
                case "counter_rush":
                    interval *= 0.68f;
                    earlySlotBias = 0.78f;
                    break;
                default:
                    break;
            }

            thinkInterval = interval;
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

            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }

            timer = thinkInterval + Random.Range(-1.5f, 2f);
            int slot = Random.value < earlySlotBias ? Random.Range(0, 4) : Random.Range(4, 8);
            enemyCore.TryEnqueueDeckSlot(slot);
        }
    }
}
