using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// ?ë™ ?ì›: ë§?ì´??¬ë ˆ?§ì´ ì¦ê??œë‹¤. ?ì‚° ë¹„ìš©?€ TrySpend?ì„œ ì°¨ê°.
    /// </summary>
    public class BattleAcesEconomy : MonoBehaviour
    {
        [Header("Starting economy values")]
        [SerializeField] private float playerCredits = 220f;
        [SerializeField] private float enemyCredits = 55f;
        [SerializeField] private float playerIncomePerSecond = 7.5f;
        [SerializeField] private float enemyIncomePerSecond = 1.65f;

        /// <summary>?Œë ˆ?´ì–´ ì½”ì–´ ?…ê·¸?ˆì´?œë¡œ ì¶”ê??˜ëŠ” ì´ˆë‹¹ ?ì›</summary>
        private float playerBonusIncomePerSecond;

        public float PlayerCredits => playerCredits;
        public float EnemyCredits => enemyCredits;
        public float PlayerTotalIncomePerSecond => playerIncomePerSecond + playerBonusIncomePerSecond;

        /// <summary>?©ì…˜/ë¯¸ì…˜?ì„œ ê¸°ë³¸ ?˜ì…??ê³±í•¨(ì½”ì–´ U ?…ê·¸?ˆì´??ë³´ë„ˆ?¤ëŠ” ë³„ë„)</summary>
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

        /// <summary>ì½”ì–´ ?…ê·¸?ˆì´?????ë™ ?ì› ì¦ê?</summary>
        public void AddPlayerIncomePerSecond(float delta)
        {
            if (delta > 0f)
            {
                playerBonusIncomePerSecond += delta;
            }
        }

        /// <summary>? ë‹› ?•ì˜ ê¸°ë°˜ ?ˆë ¨ ë¹„ìš© (?„ë¡œ?•ì…˜ ?œê°„??ê¸¸ìˆ˜ë¡?ë¹„ìŒˆ)</summary>
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
