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

        /// <summary>MissionDefinition.enemyPatternId — 간격·슬롯 가중</summary>
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
                    interval *= 0.82f;
                    earlySlotBias = 0.64f;
                    break;
                case "defensive_turtle":
                    interval *= 1.22f;
                    earlySlotBias = 0.38f;
                    break;
                case "elite_mix":
                    interval *= 0.9f;
                    earlySlotBias = 0.28f;
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
