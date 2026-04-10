using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Handles Battle Aces credits, passive income, and spend helpers for player and enemy teams.
    /// </summary>
    public class BattleAcesEconomy : MonoBehaviour
    {
        [Header("Starting economy values")]
        [SerializeField] private float playerCredits = 235f;
        [SerializeField] private float enemyCredits = 55f;
        [SerializeField] private float playerIncomePerSecond = 8f;
        [SerializeField] private float enemyIncomePerSecond = 1.68f;

        /// <summary>Additional player income granted by upgrades or mission modifiers.</summary>
        private float playerBonusIncomePerSecond;

        /// <summary>Temporary opening boost that adds extra credits per second for a short window.</summary>
        private float openingBoostSecondsRemaining;
        private float openingBoostCreditsPerSecond;

        public float PlayerCredits => playerCredits;
        public float EnemyCredits => enemyCredits;
        public float PlayerTotalIncomePerSecond => playerIncomePerSecond + playerBonusIncomePerSecond;

        /// <summary>Applies income multipliers while clamping values to a safe lower bound.</summary>
        public void ApplyIncomeMultipliers(float playerMult, float enemyMult)
        {
            playerMult = Mathf.Max(0.05f, playerMult);
            enemyMult = Mathf.Max(0.05f, enemyMult);
            playerIncomePerSecond *= playerMult;
            enemyIncomePerSecond *= enemyMult;
        }

        /// <summary>Activates a temporary opening income boost for the player economy.</summary>
        public void ActivateOpeningIncomeBoost(float durationSeconds, float extraCreditsPerSecond)
        {
            if (durationSeconds <= 0f || extraCreditsPerSecond <= 0f)
            {
                return;
            }

            openingBoostSecondsRemaining = Mathf.Max(openingBoostSecondsRemaining, durationSeconds);
            openingBoostCreditsPerSecond = Mathf.Max(openingBoostCreditsPerSecond, extraCreditsPerSecond);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (openingBoostSecondsRemaining > 0f)
            {
                float step = Mathf.Min(dt, openingBoostSecondsRemaining);
                playerCredits += openingBoostCreditsPerSecond * step;
                openingBoostSecondsRemaining -= step;
                if (openingBoostSecondsRemaining <= 0f)
                {
                    openingBoostSecondsRemaining = 0f;
                    openingBoostCreditsPerSecond = 0f;
                }
            }

            playerCredits += (playerIncomePerSecond + playerBonusIncomePerSecond) * dt;
            enemyCredits += enemyIncomePerSecond * dt;
        }

        /// <summary>Adds persistent bonus income to the player economy.</summary>
        public void AddPlayerIncomePerSecond(float delta)
        {
            if (delta > 0f)
            {
                playerBonusIncomePerSecond += delta;
            }
        }

        /// <summary>Derives a simple training cost estimate from a unit definition.</summary>
        public static int GetTrainCost(UnitDefinition definition)
        {
            if (definition == null)
            {
                return 999;
            }

            float raw = definition.ProductionDuration * 3.92f;
            return Mathf.Clamp(Mathf.RoundToInt(raw), 12, 82);
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