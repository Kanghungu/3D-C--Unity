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
        [SerializeField] private float totalAssaultUnlockRatio = 0.55f;
        [SerializeField] private float weakenedEnemyUnlockRatio = 0.45f;
        [SerializeField] private int weakenedEnemyBasePhase = 3;
        [SerializeField] private int maxRecentNewsCount = 3;
        [SerializeField] private float assaultStartHighlightDuration = 8f;

        private bool playerTotalAssault;
        private bool enemyTotalAssault;
        private string currentNews;
        private float newsTimer;
        private float enemyThinkTimer;
        private readonly List<string> recentNews = new();
        private bool playerAssaultReadyAnnounced;
        private bool enemyAssaultReadyAnnounced;
        private bool playerBaseCriticalAnnounced;
        private bool enemyBaseCriticalAnnounced;
        private bool playerBaseEmergencyAnnounced;
        private float playerAssaultStartTime = -100f;
        private float enemyAssaultStartTime = -100f;

        public static BattleDirectiveController Instance { get; private set; }
        public string CurrentNews => newsTimer > 0f ? currentNews : string.Empty;
        public bool HasNews => newsTimer > 0f && !string.IsNullOrWhiteSpace(currentNews);
        public IReadOnlyList<string> RecentNews => recentNews;
        public bool HasRecentEnemyAssaultStart => enemyTotalAssault && Time.time - enemyAssaultStartTime <= assaultStartHighlightDuration;
        public int GetNewsPriority(string message) => EvaluateNewsPriority(message);

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

            EvaluateLateBattleAlerts();

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

            int ownedNodes = 0;
            foreach (ControlNode node in nodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (node.OwnerTeam == team)
                {
                    ownedNodes++;
                }
            }

            if (ownedNodes <= 0)
            {
                return false;
            }

            float controlRatio = GetControlRatio(team);
            if (controlRatio >= totalAssaultUnlockRatio)
            {
                return true;
            }

            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player);
            if (enemyBase != null
                && enemyBase.IsAlive
                && enemyBase.CurrentPhase >= weakenedEnemyBasePhase
                && ownedNodes >= 2
                && controlRatio >= weakenedEnemyUnlockRatio)
            {
                return true;
            }

            return false;
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

        public float GetTotalAssaultUnlockRatio()
        {
            return Mathf.Clamp01(totalAssaultUnlockRatio);
        }

        public int GetAssaultUnlockScore(UnitTeam team)
        {
            int total = GetTotalControlScore();
            if (total <= 0)
            {
                return 0;
            }

            return Mathf.CeilToInt(total * GetTotalAssaultUnlockRatio());
        }

        public string GetAssaultUnlockStatusLabel(UnitTeam team)
        {
            if (IsTotalAssaultActive(team))
            {
                return "ON";
            }

            if (CanDeclareTotalAssault(team))
            {
                return "준비";
            }

            int currentScore = GetControlScore(team);
            int unlockScore = GetAssaultUnlockScore(team);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player);
            bool softenedUnlockActive = enemyBase != null
                && enemyBase.IsAlive
                && enemyBase.CurrentPhase >= weakenedEnemyBasePhase;
            int softenedUnlockScore = Mathf.CeilToInt(GetTotalControlScore() * Mathf.Clamp01(weakenedEnemyUnlockRatio));

            if (softenedUnlockActive)
            {
                return $"{currentScore}/{softenedUnlockScore} 약화";
            }

            return $"{currentScore}/{unlockScore}";
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
                playerAssaultStartTime = Time.time;
                PushNews("아군 총공격 개시. 적 본진 압박을 시작합니다.");
            }
            else
            {
                enemyTotalAssault = true;
                enemyAssaultStartTime = Time.time;
                PushNews("적 총공격 개시. 적이 본진 압박 단계로 전환했습니다.");
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
            ControlNode bestNeutralNode = null;
            float bestNeutralDistance = float.MaxValue;
            ControlNode bestHostileNode = null;
            float bestHostileDistance = float.MaxValue;

            foreach (ControlNode node in PrototypeRuntimeRegistry.GetControlNodes())
            {
                if (node == null || node.OwnerTeam == team)
                {
                    continue;
                }

                float distance = enemyBase != null
                    ? Vector3.Distance(node.transform.position, enemyBase.transform.position)
                    : Vector3.Distance(node.transform.position, Vector3.zero);

                if (!node.OwnerTeam.HasValue)
                {
                    distance -= node.StrategicWeight * 8f;
                    if (distance < bestNeutralDistance)
                    {
                        bestNeutralDistance = distance;
                        bestNeutralNode = node;
                    }

                    continue;
                }

                distance -= node.StrategicWeight * 5f;
                if (distance < bestHostileDistance)
                {
                    bestHostileDistance = distance;
                    bestHostileNode = node;
                }
            }

            return bestNeutralNode != null ? bestNeutralNode : bestHostileNode;
        }

        public static bool CanTargetEnemyBaseStatic(UnitTeam team)
        {
            return Instance != null && Instance.CanTargetEnemyBase(team);
        }

        public string GetAssaultPressureLabel(UnitTeam team, BaseStructure alliedBase)
        {
            bool active = IsTotalAssaultActive(team);
            bool recentStart = team == UnitTeam.Enemy
                ? enemyTotalAssault && Time.time - enemyAssaultStartTime <= assaultStartHighlightDuration
                : playerTotalAssault && Time.time - playerAssaultStartTime <= assaultStartHighlightDuration;

            if (active)
            {
                if (team == UnitTeam.Enemy)
                {
                    if (recentStart)
                    {
                        return alliedBase != null && alliedBase.IsDefenseEmergency
                            ? "적 총공격 돌입 | 본진 직격"
                            : "적 총공격 돌입 | 본진 노출";
                    }

                    return alliedBase != null && alliedBase.IsDefenseEmergency
                        ? "적 총공격 진행 | 방어 최우선"
                        : "적 총공격 진행";
                }

                return recentStart ? "아군 총공격 돌입" : "아군 총공격 진행";
            }

            if (team == UnitTeam.Enemy && CanDeclareTotalAssault(UnitTeam.Enemy))
            {
                return "적 총공격 임박";
            }

            if (team == UnitTeam.Player && CanDeclareTotalAssault(UnitTeam.Player))
            {
                return "아군 총공격 준비";
            }

            return team == UnitTeam.Enemy ? "적 총공격 잠금" : "아군 총공격 잠금";
        }

        private void EvaluateLateBattleAlerts()
        {
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);

            bool playerReady = !playerTotalAssault && CanDeclareTotalAssault(UnitTeam.Player);
            if (playerReady && !playerAssaultReadyAnnounced)
            {
                PushNews("아군 총공격 준비 완료. T 키로 본진 압박을 시작할 수 있습니다.");
                playerAssaultReadyAnnounced = true;
            }
            else if (!playerReady)
            {
                playerAssaultReadyAnnounced = false;
            }

            bool enemyReady = !enemyTotalAssault && CanDeclareTotalAssault(UnitTeam.Enemy);
            if (enemyReady && !enemyAssaultReadyAnnounced)
            {
                PushNews("적 총공격 준비 완료. 적 본진 압박이 곧 시작됩니다.");
                enemyAssaultReadyAnnounced = true;
            }
            else if (!enemyReady)
            {
                enemyAssaultReadyAnnounced = false;
            }

            bool playerCritical = playerBase != null && playerBase.IsAlive && playerBase.CurrentPhase >= 3;
            if (playerCritical && !playerBaseCriticalAnnounced)
            {
                PushNews($"아군 본진 경고. P{playerBase.CurrentPhase} {playerBase.PhaseStatusLabel}.");
                playerBaseCriticalAnnounced = true;
            }
            else if (!playerCritical)
            {
                playerBaseCriticalAnnounced = false;
            }

            bool playerEmergency = playerBase != null && playerBase.IsAlive && playerBase.IsDefenseEmergency;
            if (playerEmergency && !playerBaseEmergencyAnnounced)
            {
                PushNews($"아군 본진 비상. 적 {playerBase.NearbyHostileCount}기 접근, 수비 {playerBase.NearbyFriendlyCount}기.");
                playerBaseEmergencyAnnounced = true;
            }
            else if (!playerEmergency)
            {
                playerBaseEmergencyAnnounced = false;
            }

            bool enemyCritical = enemyBase != null && enemyBase.IsAlive && enemyBase.CurrentPhase >= 3;
            if (enemyCritical && !enemyBaseCriticalAnnounced)
            {
                PushNews($"적 본진 붕괴 임박. P{enemyBase.CurrentPhase} {enemyBase.PhaseStatusLabel}.");
                enemyBaseCriticalAnnounced = true;
            }
            else if (!enemyCritical)
            {
                enemyBaseCriticalAnnounced = false;
            }
        }

        private void PushNews(string message)
        {
            message = NormalizeNewsHeadline(message?.Trim());
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (recentNews.Count > 0 && recentNews[0] == message)
            {
                currentNews = message;
                newsTimer = newsDuration;
                return;
            }

            currentNews = message;
            newsTimer = newsDuration;

            recentNews.Remove(message);
            int messagePriority = EvaluateNewsPriority(message);
            int insertIndex = recentNews.Count;
            for (int index = 0; index < recentNews.Count; index++)
            {
                if (messagePriority >= EvaluateNewsPriority(recentNews[index]))
                {
                    insertIndex = index;
                    break;
                }
            }

            recentNews.Insert(insertIndex, message);
            int keepCount = Mathf.Max(1, maxRecentNewsCount);
            if (recentNews.Count > keepCount)
            {
                recentNews.RemoveRange(keepCount, recentNews.Count - keepCount);
            }
        }

        private static int EvaluateNewsPriority(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return 0;
            }

            if (message.Contains("본진 비상") || message.Contains("총공격 개시") || message.Contains("붕괴 임박") || message.Contains("함락"))
            {
                return 3;
            }

            if (message.Contains("본진 경고") || message.Contains("총공격 준비") || message.Contains("재탈환") || message.Contains("전방 탈환"))
            {
                return 2;
            }

            if (message.Contains("단계 변화") || message.Contains("복구") || message.Contains("점령"))
            {
                return 1;
            }

            return 0;
        }

        private static string NormalizeNewsHeadline(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            message = message.Trim();

            return message switch
            {
                "아군 총공격 개시. 적 본진 압박을 시작합니다." => "아군 총공격 개시",
                "적 총공격 개시. 적이 본진 압박 단계로 전환했습니다." => "적 총공격 개시",
                "아군 총공격 준비 완료. T 키로 본진 압박을 시작할 수 있습니다." => "아군 총공격 준비 완료",
                "적 총공격 준비 완료. 적 본진 압박이 곧 시작됩니다." => "적 총공격 준비 완료",
                _ => NormalizeDynamicNewsHeadline(message)
            };
        }

        private static string NormalizeDynamicNewsHeadline(string message)
        {
            if (message.StartsWith("아군 본진 경고. P"))
            {
                return message
                    .Replace("아군 본진 경고. ", "아군 본진 경고 | ")
                    .TrimEnd('.');
            }

            if (message.StartsWith("아군 본진 비상. 적 "))
            {
                return message
                    .Replace("아군 본진 비상. 적 ", "아군 본진 비상 | 적 ")
                    .Replace("기 접근, 수비 ", " / 수비 ")
                    .Replace("기.", "");
            }

            if (message.StartsWith("적 본진 붕괴 임박. P"))
            {
                return message
                    .Replace("적 본진 붕괴 임박. ", "적 본진 붕괴 임박 | ")
                    .TrimEnd('.');
            }

            if (message.EndsWith("아군 전선이 복구됩니다."))
            {
                return message
                    .Replace(". 아군 전선이 복구됩니다.", " | 전선 복구");
            }

            if (message.EndsWith("전방 탈환 대응이 필요합니다."))
            {
                return message
                    .Replace(". 전방 탈환 대응이 필요합니다.", " | 탈환 대응");
            }

            if (message.Contains("단계 변화"))
            {
                return message
                    .Replace(". ", " | ")
                    .Replace(" 단계 변화 ", " 단계 | ")
                    .TrimEnd('.');
            }

            return message;
        }
    }
}
