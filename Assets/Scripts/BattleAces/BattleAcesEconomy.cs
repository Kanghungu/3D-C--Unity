using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 자동 자원: 매 초 크레딧이 증가한다. 생산 비용은 TrySpend에서 차감.
    /// </summary>
    public class BattleAcesEconomy : MonoBehaviour
    {
        [Header("난이도(플레이어 유리) — 인스펙터에서 조정 가능")]
        [SerializeField] private float playerCredits = 220f;
        [SerializeField] private float enemyCredits = 55f;
        [SerializeField] private float playerIncomePerSecond = 7.5f;
        [SerializeField] private float enemyIncomePerSecond = 1.65f;

        /// <summary>플레이어 코어 업그레이드로 추가되는 초당 자원</summary>
        private float playerBonusIncomePerSecond;

        public float PlayerCredits => playerCredits;
        public float EnemyCredits => enemyCredits;
        public float PlayerTotalIncomePerSecond => playerIncomePerSecond + playerBonusIncomePerSecond;

        /// <summary>팩션/미션에서 기본 수입에 곱함(코어 U 업그레이드 보너스는 별도)</summary>
        public void ApplyIncomeMultipliers(float playerMult, float enemyMult)
        {
            playerMult = Mathf.Max(0.05f, playerMult);
            enemyMult = Mathf.Max(0.05f, enemyMult);
            playerIncomePerSecond *= playerMult;
            enemyIncomePerSecond *= enemyMult;
        }

        private void Update()
        {
            playerCredits += (playerIncomePerSecond + playerBonusIncomePerSecond) * Time.deltaTime;
            enemyCredits += enemyIncomePerSecond * Time.deltaTime;
        }

        /// <summary>코어 업그레이드 — 자동 자원 증가</summary>
        public void AddPlayerIncomePerSecond(float delta)
        {
            if (delta > 0f)
            {
                playerBonusIncomePerSecond += delta;
            }
        }

        /// <summary>유닛 정의 기반 훈련 비용 (프로덕션 시간이 길수록 비쌈)</summary>
        public static int GetTrainCost(UnitDefinition definition)
        {
            if (definition == null)
            {
                return 999;
            }

            float raw = definition.ProductionDuration * 4f;
            return Mathf.Clamp(Mathf.RoundToInt(raw), 12, 85);
        }

        public bool TrySpendPlayer(int amount)
        {
            if (amount <= 0 || playerCredits < amount)
            {
                return false;
            }

            playerCredits -= amount;
            return true;
        }

        public bool TrySpendEnemy(int amount)
        {
            if (amount <= 0 || enemyCredits < amount)
            {
                return false;
            }

            enemyCredits -= amount;
            return true;
        }

        public void AddDebugCredits(int amount)
        {
            playerCredits += amount;
        }
    }
}
