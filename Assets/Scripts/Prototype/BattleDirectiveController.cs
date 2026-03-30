using System.Collections.Generic;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Prototype
{
    /// <summary>
    /// Controls global assault state, strategic control scoring, and war-news alerts.
    /// </summary>
    public class BattleDirectiveController : MonoBehaviour
    {
        [SerializeField] private float newsDuration = 4.5f;
        [SerializeField] private float enemyThinkInterval = 2.5f;

        private bool playerTotalAssault;
        private bool enemyTotalAssault;
        private string currentNews;
        private float newsTimer;
        private float enemyThinkTimer;

        public static BattleDirectiveController Instance { get; private set; }
        public string CurrentNews => newsTimer > 0f ? currentNews : string.Empty;
        public bool HasNews => newsTimer > 0f && !string.IsNullOrWhiteSpace(currentNews);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (newsTimer > 0f)
            {
                newsTimer -= Time.unscaledDeltaTime;
            }

            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                TryDeclareTotalAssault(UnitTeam.Player);
            }

            enemyThinkTimer -= Time.deltaTime;
            if (enemyThinkTimer <= 0f)
            {
                enemyThinkTimer = enemyThinkInterval;
                TryDeclareTotalAssault(UnitTeam.Enemy);
            }
        }

        public bool IsTotalAssaultActive(UnitTeam team)
        {
            return team == UnitTeam.Player ? playerTotalAssault : enemyTotalAssault;
        }

        public bool CanTargetEnemyBase(UnitTeam team)
        {
            return IsTotalAssaultActive(team);
        }

        public bool CanDeclareTotalAssault(UnitTeam team)
        {
            IReadOnlyList<ControlNode> nodes = PrototypeRuntimeRegistry.GetControlNodes();
            if (nodes.Count == 0)
            {
                return false;
            }

            foreach (ControlNode node in nodes)
            {
                if (node == null || node.OwnerTeam != team)
                {
                    return false;
                }
            }

            return true;
        }

        public int GetControlScore(UnitTeam team)
        {
            int score = 0;
            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node != null && node.OwnerTeam == team)
                {
                    score += node.StrategicWeight;
                }
            }

            return score;
        }

        public int GetTotalControlScore()
        {
            int total = 0;
            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node != null)
                {
                    total += node.StrategicWeight;
                }
            }

            return total;
        }

        public float GetControlRatio(UnitTeam team)
        {
            int total = GetTotalControlScore();
            if (total <= 0)
            {
                return 0f;
            }

            return GetControlScore(team) / (float)total;
        }

        public string GetStrategicPressureLabel(UnitTeam team)
        {
            float ratio = GetControlRatio(team);
            if (ratio >= 0.99f)
            {
                return "Total dominance";
            }

            if (ratio >= 0.72f)
            {
                return "Frontline crushing";
            }

            if (ratio >= 0.56f)
            {
                return "Pressing advantage";
            }

            if (ratio >= 0.4f)
            {
                return "Contested line";
            }

            if (ratio > 0f)
            {
                return "Losing ground";
            }

            return "No foothold";
        }

        public bool TryDeclareTotalAssault(UnitTeam team)
        {
            if (IsTotalAssaultActive(team) || !CanDeclareTotalAssault(team))
            {
                return false;
            }

            if (team == UnitTeam.Player)
            {
                playerTotalAssault = true;
                PushNews("Imperial News: Total Assault authorized.");
            }
            else
            {
                enemyTotalAssault = true;
                PushNews("Enemy Broadcast: Total Assault initiated.");
            }

            return true;
        }

        public void BroadcastNews(string message)
        {
            PushNews(message);
        }

        public static void BroadcastNewsStatic(string message)
        {
            if (Instance != null && !string.IsNullOrWhiteSpace(message))
            {
                Instance.BroadcastNews(message);
            }
        }

        public ControlNode FindPriorityNodeFor(UnitTeam team)
        {
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player);
            ControlNode bestNode = null;
            float bestScore = float.MinValue;

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null || node.OwnerTeam == team)
                {
                    continue;
                }

                float score = node.StrategicWeight * 1000f;
                if (node.OwnerTeam == null)
                {
                    score += 350f;
                }

                if (enemyBase != null)
                {
                    score -= Vector3.Distance(node.transform.position, enemyBase.transform.position) * 0.1f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        public static bool CanTargetEnemyBaseStatic(UnitTeam team)
        {
            return Instance != null && Instance.CanTargetEnemyBase(team);
        }

        private void PushNews(string message)
        {
            currentNews = message;
            newsTimer = newsDuration;
        }
    }
}
