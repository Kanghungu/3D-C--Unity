using Game.Selection;
using Game.Units;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Shared text formatting for the prototype HUD.
    /// Keeps string assembly out of the main GUI flow so HUD changes stay easier to read.
    /// </summary>
    public static class PrototypeHudTextUtility
    {
        public static string BuildStatus(PrototypeMatchController matchController, BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            if (matchController != null)
            {
                return matchController.Result switch
                {
                    MatchResult.Victory => BuildFinishedStatus(matchController.EndReason, true),
                    MatchResult.Defeat => BuildFinishedStatus(matchController.EndReason, false),
                    _ => BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits)
                };
            }

            return BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits);
        }

        public static string BuildOverviewMapLine(List<ControlNode> controlNodes)
        {
            int totalNodes = 0;
            int playerNodes = 0;
            int enemyNodes = 0;

            if (controlNodes != null)
            {
                foreach (ControlNode node in controlNodes)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    totalNodes++;
                    if (node.OwnerTeam == UnitTeam.Player)
                    {
                        playerNodes++;
                    }
                    else if (node.OwnerTeam == UnitTeam.Enemy)
                    {
                        enemyNodes++;
                    }
                }
            }

            return totalNodes > 0
                ? $"거점   아군 {playerNodes}/{totalNodes}  /  적 {enemyNodes}/{totalNodes}"
                : "거점   정보 없음";
        }

        public static string BuildOverviewForceLine(int playerUnits, int enemyUnits)
        {
            int delta = playerUnits - enemyUnits;
            string deltaLabel = delta > 0 ? $"+{delta}" : delta.ToString();
            return $"병력   아군 {playerUnits}  /  적 {enemyUnits}   ({deltaLabel})";
        }

        public static string BuildOverviewBaseLine(BaseStructure playerBase, BaseStructure enemyBase)
        {
            string playerBaseLabel = playerBase != null
                ? $"{ToPercent(playerBase)}%"
                : "--";
            string enemyBaseLabel = enemyBase != null
                ? $"{ToPercent(enemyBase)}%"
                : "--";
            return $"본진   아군 {playerBaseLabel}  /  적 {enemyBaseLabel}";
        }

        public static string BuildControlGroupLine(PrototypeSelectionController selectionController)
        {
            string summary = selectionController != null ? selectionController.GetControlGroupSummary() : "없음";
            return $"Ctrl+F1-F5 지정 | F1-F5 호출 | 두 번 눌러 화면 이동 | 그룹 {summary}";
        }

        public static string BuildCommandControlLine(PrototypeSelectionController selectionController)
        {
            string commandLabel = "대기";

            if (selectionController != null && !string.IsNullOrWhiteSpace(selectionController.MoveMarkerLabel))
            {
                commandLabel = selectionController.MoveMarkerLabel;
            }

            string groupSummary = selectionController != null ? selectionController.GetControlGroupSummary() : "None";
            string commandHint = commandLabel == "대기"
                ? "A 공격  B 후퇴  H 고정  R 재시작"
                : commandLabel;
            string groupEventLabel = string.Empty;

            if (selectionController != null)
            {
                if (selectionController.HasRecentControlGroupAssignment)
                {
                    groupEventLabel = $" | 지정 {selectionController.RecentControlGroupAssignmentLabel}";
                }
                else if (selectionController.HasRecentControlGroupRecall)
                {
                    string routeLabel = selectionController.RecentControlGroupRouteLabel;
                    groupEventLabel = string.IsNullOrWhiteSpace(routeLabel)
                        ? $" | 이동 {selectionController.RecentControlGroupRecallLabel}"
                        : $" | 이동 {selectionController.RecentControlGroupRecallLabel} {routeLabel}";
                }
            }

            return Shorten($"명령 {commandHint}{groupEventLabel} | 그룹 {groupSummary}", 58);
        }

        public static string BuildDirectiveLine(BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            int totalNodes = controlNodes.Count;
            int playerOwned = 0;
            int enemyOwned = 0;

            foreach (ControlNode node in controlNodes)
            {
                if (node == null)
                {
                    continue;
                }

                if (node.OwnerTeam == UnitTeam.Player)
                {
                    playerOwned++;
                }
                else if (node.OwnerTeam == UnitTeam.Enemy)
                {
                    enemyOwned++;
                }
            }

            int totalScore = directiveController != null ? directiveController.GetTotalControlScore() : 0;
            int playerScore = directiveController != null ? directiveController.GetControlScore(UnitTeam.Player) : 0;
            int enemyScore = directiveController != null ? directiveController.GetControlScore(UnitTeam.Enemy) : 0;
            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            string playerDirective = directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Player)
                ? directiveController.GetAssaultPressureLabel(UnitTeam.Player, null)
                : $"아군 총공격 {directiveController.GetAssaultUnlockStatusLabel(UnitTeam.Player)}";
            string enemyDirective = directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Enemy)
                ? directiveController.GetAssaultPressureLabel(UnitTeam.Enemy, playerBase)
                : directiveController != null
                    ? directiveController.GetAssaultPressureLabel(UnitTeam.Enemy, playerBase)
                    : "적 총공격 잠금";

            return $"거점 아군:{playerOwned}/{totalNodes} 적:{enemyOwned}/{totalNodes} | 점수 아군:{playerScore}/{totalScore} 적:{enemyScore}/{totalScore} | {playerDirective} | {enemyDirective}";
        }

        public static string BuildStrategicPressureLine(BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            if (directiveController == null || controlNodes == null || controlNodes.Count == 0)
            {
                return "전선 압박 정보를 아직 계산할 수 없습니다.";
            }

            int grandPlayer = 0;
            int grandEnemy = 0;
            int majorPlayer = 0;
            int majorEnemy = 0;

            foreach (ControlNode node in controlNodes)
            {
                if (node == null || !node.OwnerTeam.HasValue)
                {
                    continue;
                }

                if (node.IsGrand)
                {
                    if (node.OwnerTeam == UnitTeam.Player)
                    {
                        grandPlayer++;
                    }
                    else
                    {
                        grandEnemy++;
                    }
                }
                else if (node.IsMajor)
                {
                    if (node.OwnerTeam == UnitTeam.Player)
                    {
                        majorPlayer++;
                    }
                    else
                    {
                        majorEnemy++;
                    }
                }
            }

            return $"전선 압박   아군 {directiveController.GetStrategicPressureLabel(UnitTeam.Player)}  /  적 {directiveController.GetStrategicPressureLabel(UnitTeam.Enemy)}";
        }

        public static string BuildVictoryBody(PrototypeMatchController matchController, BaseStructure enemyBase, int enemyUnits)
        {
            if (matchController != null)
            {
                return matchController.EndReason switch
                {
                    MatchEndReason.EnemyBaseDestroyed => "적 본진이 무너졌습니다.",
                    MatchEndReason.EnemyArmyDestroyed => "적 병력이 전멸했습니다.",
                    MatchEndReason.MutualAnnihilation => "양측이 함께 붕괴했지만 전장을 지키지 못했습니다.",
                    _ => BuildVictoryFallback(enemyBase, enemyUnits)
                };
            }

            return BuildVictoryFallback(enemyBase, enemyUnits);
        }

        public static string BuildDefeatBody(PrototypeMatchController matchController, BaseStructure playerBase, int playerUnits)
        {
            if (matchController != null)
            {
                return matchController.EndReason switch
                {
                    MatchEndReason.PlayerBaseDestroyed => "아군 본진이 파괴됐습니다.",
                    MatchEndReason.PlayerArmyDestroyed => "아군 병력이 전멸했습니다.",
                    MatchEndReason.MutualAnnihilation => "양측이 함께 무너져도 승리로 인정되지 않습니다.",
                    _ => BuildDefeatFallback(playerBase, playerUnits)
                };
            }

            return BuildDefeatFallback(playerBase, playerUnits);
        }

        private static string BuildVictoryFallback(BaseStructure enemyBase, int enemyUnits)
        {
            if (enemyBase == null || !enemyBase.IsAlive)
            {
                return "적 본진이 무너졌습니다.";
            }

            if (enemyUnits == 0)
            {
                return "적 병력이 전멸했습니다.";
            }

            return "적 전선이 붕괴했습니다.";
        }

        private static string BuildDefeatFallback(BaseStructure playerBase, int playerUnits)
        {
            if (playerBase == null || !playerBase.IsAlive)
            {
                return "아군 본진이 파괴됐습니다.";
            }

            if (playerUnits == 0)
            {
                return "아군 병력이 전멸했습니다.";
            }

            return "아군 전선이 붕괴했습니다.";
        }

        public static string BuildSelectionOrdersSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            Dictionary<string, int> counts = new();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                string label = unit.OrderLabel;
                if (!counts.TryAdd(label, 1))
                {
                    counts[label]++;
                }
            }

            if (counts.Count == 0)
            {
                return "명령 없음";
            }

            List<KeyValuePair<string, int>> ordered = new(counts);
            ordered.Sort((left, right) => right.Value.CompareTo(left.Value));

            StringBuilder builder = new();
            builder.Append("명령 ");

            for (int index = 0; index < ordered.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(ordered[index].Key);
                builder.Append(":");
                builder.Append(ordered[index].Value);
            }

            return Shorten(builder.ToString(), 54);
        }

        public static string BuildSelectionAbilitySummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                return $"능력 {unit.DisplayName}: {unit.AbilityStatus}";
            }

            return "능력 없음";
        }

        public static string BuildSelectionPriorityLine(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            if (selectedUnits == null || selectedUnits.Count == 0)
            {
                return "우선 행동 없음";
            }

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BattleDirectiveController directiveController = BattleDirectiveController.Instance;
            int shieldCount = 0;
            int spearCount = 0;
            int rifleCount = 0;
            int fireSupportCount = 0;
            int assaultCount = 0;

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                switch (unit.Archetype)
                {
                    case UnitArchetype.ShieldInfantry:
                        shieldCount++;
                        break;
                    case UnitArchetype.Spearman:
                        spearCount++;
                        break;
                    case UnitArchetype.Rifleman:
                        rifleCount++;
                        break;
                    case UnitArchetype.Artillery:
                    case UnitArchetype.Fighter:
                        fireSupportCount++;
                        break;
                    case UnitArchetype.SpecialWarrior:
                    case UnitArchetype.RoyalGuard:
                    case UnitArchetype.MobileFortress:
                    case UnitArchetype.AirborneCitadel:
                        assaultCount++;
                        break;
                }
            }

            if (playerBase != null && playerBase.IsDefenseEmergency)
            {
                if (shieldCount + spearCount >= rifleCount + fireSupportCount)
                {
                    return "우선 행동 본진 차단선 복귀 | 후퇴(B) 또는 본진 우클릭";
                }

                return "우선 행동 본진 지원 사격 | 본진 뒤 화력선 재정렬";
            }

            if (directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Enemy))
            {
                return fireSupportCount + rifleCount >= shieldCount + spearCount
                    ? "우선 행동 본진 직격 대응 | 후방 화력 유지"
                    : "우선 행동 적 돌입 저지 | 본진 앞 재집결";
            }

            if (directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Player))
            {
                return assaultCount + fireSupportCount >= shieldCount + spearCount
                    ? "우선 행동 적 본진 압박 | 공격(A) 유지"
                    : "우선 행동 전선 열기 | 총공격 축 합류";
            }

            if (fireSupportCount >= 2)
            {
                return "우선 행동 후방 화력 유지 | 방패 보병 뒤 배치";
            }

            if (shieldCount + spearCount >= Mathf.Max(3, rifleCount + assaultCount))
            {
                return "우선 행동 전선 유지 | 중립/전방 거점 고정";
            }

            if (rifleCount + assaultCount >= 3)
            {
                return "우선 행동 측면 압박 | 빈 거점 또는 약한 전선 돌파";
            }

            return "우선 행동 전선 합류 | 현재 명령 축 유지";
        }

        public static List<string> BuildSelectionGroupLines(IReadOnlyList<SelectableUnit> selectedUnits, int maxEntries)
        {
            Dictionary<string, int> counts = new();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                string label = $"{BuildArchetypeTag(unit.Archetype)} {unit.DisplayName}";

                if (!counts.TryAdd(label, 1))
                {
                    counts[label]++;
                }
            }

            List<KeyValuePair<string, int>> ordered = new(counts);
            ordered.Sort((left, right) => right.Value.CompareTo(left.Value));

            List<string> result = new();
            int added = 0;

            foreach (KeyValuePair<string, int> pair in ordered)
            {
                result.Add($"{pair.Key} x{pair.Value}");
                added++;

                if (added >= maxEntries)
                {
                    break;
                }
            }

            return result;
        }

        public static string BuildProductionLine(ProductionStructure structure)
        {
            if (structure == null)
            {
                return "생산 건물 없음";
            }

            if (!structure.IsAlive)
            {
                return $"{structure.StructureLabel} | 비활성";
            }

            string productionState = structure.HasQueuedProduction
                ? $"{structure.QueueLabel} {UnityEngine.Mathf.RoundToInt(structure.ProductionProgressNormalized * 100f)}%"
                : "대기";

            return Shorten($"{structure.StructureLabel} | {structure.PriorityModeLabel} | {productionState} | 대기열 {structure.QueueCount}", 62);
        }

        public static string BuildProductionDetailLine(ProductionStructure structure)
        {
            if (structure == null)
            {
                return "랠리 정보 없음";
            }

            bool baseEmergency = structure.PriorityModeLabel == "본진 비상";
            string responseLabel = Shorten(structure.ProductionResponseLabel, baseEmergency ? 25 : 28);
            string unitLabel = baseEmergency
                ? $"권장{Shorten(structure.SuggestedUnitsLabel, 20)}"
                : $"추천 {Shorten(structure.SuggestedUnitsLabel, 20)}";
            string speedLabel = baseEmergency
                ? $"(x{structure.SpeedMultiplier:0.#})"
                : $"생산 x{structure.SpeedMultiplier:0.00}";
            return Shorten($"{responseLabel} | {unitLabel} | {speedLabel}", baseEmergency ? 67 : 62);
        }

        public static string BuildProductionControlHint(BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            string controlHint = "1-8 생산 선택   |   Alt+우클릭 랠리 지점 설정";
            if (directiveController == null || controlNodes == null)
            {
                return controlHint;
            }

            string directiveSummary = BuildCompactDirectiveSummary(directiveController, controlNodes);
            return Shorten($"{directiveSummary} | {controlHint}", 74);
        }

        public static string BuildControlNodeLine(List<ControlNode> controlNodes)
        {
            if (controlNodes == null || controlNodes.Count == 0)
            {
                return "성지 없음";
            }

            ControlNode emergencyNode = null;
            foreach (ControlNode node in controlNodes)
            {
                if (node != null && node.IsPlayerRecaptureEmergency)
                {
                    if (emergencyNode == null
                        || node.StrategicWeight > emergencyNode.StrategicWeight
                        || node.CaptureProgressNormalized > emergencyNode.CaptureProgressNormalized)
                    {
                        emergencyNode = node;
                    }
                }
            }

            if (emergencyNode != null)
            {
                return $"거점 경고 {emergencyNode.NodeLabel} {emergencyNode.TierShortLabel} | {emergencyNode.RecaptureAlertLabel}";
            }

            List<ControlNode> orderedNodes = new(controlNodes);
            orderedNodes.RemoveAll(node => node == null);
            orderedNodes.Sort((left, right) => right.StrategicWeight.CompareTo(left.StrategicWeight));

            StringBuilder builder = new();
            builder.Append("목표 ");
            int shown = 0;

            foreach (ControlNode node in orderedNodes)
            {
                if (shown > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(node.NodeLabel);
                builder.Append(" ");
                builder.Append(node.TierShortLabel);
                builder.Append(" ");
                builder.Append(node.CaptureSummaryLabel);
                shown++;

                if (shown >= 3)
                {
                    break;
                }
            }

            return Shorten(builder.ToString(), 66);
        }

        public static string BuildBaseDefenseLine(BaseStructure playerBase)
        {
            if (playerBase == null)
            {
                return "본진 경보 정보를 아직 읽을 수 없습니다.";
            }

            if (!playerBase.IsAlive)
            {
                return "본진 경보 본진이 이미 붕괴했습니다.";
            }

            if (playerBase.NearbyHostileCount <= 0)
            {
                string reserveLabel = playerBase.CurrentPhase >= 3 ? "추가 수비 필요" : "주둔 안정";
                return $"본진 경보 {ToDefenseUrgencyKorean(playerBase.DefenseUrgencyLabel)} | 수비 {playerBase.NearbyFriendlyCount} | {reserveLabel}";
            }

            if (playerBase.IsDefenseEmergency)
            {
                return $"본진 경보 적 {playerBase.NearbyHostileCount} / 수비 {playerBase.NearbyFriendlyCount} | 즉시 귀환 필요";
            }

            if (playerBase.CurrentPhase >= 3)
            {
                return $"본진   적 접근 {playerBase.NearbyHostileCount}  /  수비 {playerBase.NearbyFriendlyCount}   방어 급함";
            }

            return $"본진 경보 적 {playerBase.NearbyHostileCount} / 수비 {playerBase.NearbyFriendlyCount} | {ToDefenseUrgencyKorean(playerBase.DefenseUrgencyLabel)}";
        }

        public static string BuildProductionNetworkLine(List<ProductionStructure> playerProductions, List<ControlNode> controlNodes)
        {
            ProductionStructure threatenedProduction = null;
            if (playerProductions != null)
            {
                foreach (ProductionStructure structure in playerProductions)
                {
                    if (structure == null || !structure.IsAlive || !structure.IsThreatened)
                    {
                        continue;
                    }

                    if (threatenedProduction == null
                        || structure.NearbyHostileCount > threatenedProduction.NearbyHostileCount
                        || structure.NearbyThreatPressure > threatenedProduction.NearbyThreatPressure)
                    {
                        threatenedProduction = structure;
                    }
                }
            }

            ControlNode urgentNode = null;
            if (controlNodes != null)
            {
                foreach (ControlNode node in controlNodes)
                {
                    if (node == null || !node.IsPlayerRecaptureEmergency)
                    {
                        continue;
                    }

                    if (urgentNode == null
                        || node.StrategicWeight > urgentNode.StrategicWeight
                        || node.CaptureProgressNormalized > urgentNode.CaptureProgressNormalized)
                    {
                        urgentNode = node;
                    }
                }
            }

            if (urgentNode != null && threatenedProduction != null)
            {
                return $"연결 경고 {urgentNode.NodeLabel} -> {threatenedProduction.StructureLabel} | 거점 탈환과 생산선 방어 동시 대응";
            }

            if (threatenedProduction != null)
            {
                return $"생산선 경고 {threatenedProduction.StructureLabel} | 적 {threatenedProduction.NearbyHostileCount}기 접근";
            }

            if (urgentNode != null)
            {
                return $"거점 연계 {urgentNode.NodeLabel} | 후방 생산선 지원 준비";
            }

            return "거점-생산선 연결 안정 | 현재 지원선 유지";
        }

        public static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..System.Math.Max(0, maxLength - 3)] + "...";
        }

        private static string BuildOngoingStatus(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            bool playerCritical = playerBase != null && playerBase.CurrentPhase >= 3;
            bool enemyCritical = enemyBase != null && enemyBase.CurrentPhase >= 3;
            bool enemyCollapsing = enemyBase != null && enemyBase.CurrentPhase >= 4;
            bool playerCollapsing = playerBase != null && playerBase.CurrentPhase >= 4;
            bool playerEmergency = playerBase != null && playerBase.IsDefenseEmergency;

            if (playerCollapsing && enemyCollapsing)
            {
                return "양측 본진이 모두 최종 붕괴 단계입니다.";
            }

            if (playerCollapsing)
            {
                return "아군 본진이 최종 붕괴 단계입니다. 즉시 방어선을 복구하세요.";
            }

            if (playerEmergency)
            {
                return $"아군 본진에 적 {playerBase.NearbyHostileCount}기가 파고들었습니다. 병력을 즉시 되돌리세요.";
            }

            if (enemyCollapsing)
            {
                return "적 본진이 최종 붕괴 단계입니다. 전선을 밀어 끝내세요.";
            }

            if (enemyUnits > playerUnits + 140)
            {
                return "적 병력이 전장을 덮고 있습니다. 본진 방어선을 먼저 지켜야 합니다.";
            }

            if (enemyUnits > playerUnits + 80)
            {
                return "적 병력이 빠르게 불어나고 있습니다.";
            }

            if (playerUnits > enemyUnits + 180)
            {
                return "아군 병력이 전장을 장악했습니다. 총공격으로 마무리할 수 있습니다.";
            }

            if (playerUnits > enemyUnits + 120)
            {
                return "아군 전선이 강하게 밀어붙이고 있습니다.";
            }

            if (playerCritical && enemyCritical)
            {
                return "양측 본진이 함께 흔들리고 있습니다. 마지막 교전이 중요합니다.";
            }

            if (enemyCritical)
            {
                return "적 본진이 흔들리고 있습니다.";
            }

            if (playerCritical)
            {
                return "아군 본진 방어가 위태롭습니다.";
            }

            return "거점을 지키며 적 전선을 무너뜨리세요.";
        }

        private static string GetMapLabel()
        {
            BattlefieldMapProfile mapProfile = UnityEngine.Object.FindAnyObjectByType<BattlefieldMapProfile>();
            if (mapProfile == null)
            {
                return "프로토타입 전장";
            }

            return string.IsNullOrWhiteSpace(mapProfile.MapLabel)
                ? "프로토타입 전장"
                : mapProfile.MapLabel;
        }

        private static int ToPercent(BaseStructure baseStructure)
        {
            if (baseStructure == null)
            {
                return 0;
            }

            return UnityEngine.Mathf.RoundToInt(baseStructure.HealthNormalized * 100f);
        }

        private static string BuildFinishedStatus(MatchEndReason endReason, bool isVictory)
        {
            return endReason switch
            {
                MatchEndReason.EnemyBaseDestroyed => "상태: 적 본진 파괴로 승리.",
                MatchEndReason.EnemyArmyDestroyed => "상태: 적 전멸로 승리.",
                MatchEndReason.PlayerBaseDestroyed => "상태: 본진 함락으로 패배.",
                MatchEndReason.PlayerArmyDestroyed => "상태: 병력 전멸로 패배.",
                MatchEndReason.MutualAnnihilation => "상태: 동시 붕괴. 전투 패배.",
                _ => isVictory ? "상태: 승리 조건 달성." : "상태: 패배 조건 발생."
            };
        }

        private static string BuildRallySummary(UnityEngine.Vector3 origin, UnityEngine.Vector3 rallyPoint)
        {
            UnityEngine.Vector3 offset = rallyPoint - origin;
            offset.y = 0f;
            float distance = offset.magnitude;

            string direction = "Center";
            if (distance > 0.25f)
            {
                float angle = UnityEngine.Mathf.Atan2(offset.x, offset.z) * UnityEngine.Mathf.Rad2Deg;
                if (angle < 0f)
                {
                    angle += 360f;
                }

                direction = angle switch
                {
                    >= 337.5f or < 22.5f => "N",
                    >= 22.5f and < 67.5f => "NE",
                    >= 67.5f and < 112.5f => "E",
                    >= 112.5f and < 157.5f => "SE",
                    >= 157.5f and < 202.5f => "S",
                    >= 202.5f and < 247.5f => "SW",
                    >= 247.5f and < 292.5f => "W",
                    _ => "NW"
                };
            }

            return $"랠리 {direction} {distance:0}m";
        }

        private static string BuildCompactDirectiveSummary(BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            int totalScore = 0;
            int playerScore = 0;

            foreach (ControlNode node in controlNodes)
            {
                if (node == null)
                {
                    continue;
                }

                totalScore += node.StrategicWeight;
                if (node.OwnerTeam == UnitTeam.Player)
                {
                    playerScore += node.StrategicWeight;
                }
            }

            string playerDirective = directiveController.IsTotalAssaultActive(UnitTeam.Player)
                ? "아군 총공격 ON"
                : $"아군 {directiveController.GetAssaultUnlockStatusLabel(UnitTeam.Player)}";
            string enemyDirective = directiveController.IsTotalAssaultActive(UnitTeam.Enemy)
                ? "적 총공격 ON"
                : directiveController.GetAssaultPressureLabel(UnitTeam.Enemy, PrototypeRuntimeQuery.FindBase(UnitTeam.Player));

            return $"총공격 {playerScore}/{totalScore} | {playerDirective} | {enemyDirective}";
        }

        private static string ToDefenseUrgencyKorean(string label)
        {
            return label switch
            {
                "Destroyed" => "붕괴",
                "Unstable" => "불안정",
                "Stable" => "안정",
                "Immediate" => "즉시 귀환",
                "Urgent" => "방어 급함",
                "Engaged" => "교전 중",
                _ => "안정"
            };
        }

        private static string BuildArchetypeTag(UnitArchetype archetype)
        {
            return archetype switch
            {
                UnitArchetype.Spearman => "[Spear]",
                UnitArchetype.ShieldInfantry => "[Shield]",
                UnitArchetype.Rifleman => "[Rifle]",
                UnitArchetype.SpecialWarrior => "[Special]",
                UnitArchetype.RoyalGuard => "[Guard]",
                UnitArchetype.Artillery => "[Siege]",
                UnitArchetype.Fighter => "[Air]",
                UnitArchetype.MobileFortress => "[Fort]",
                UnitArchetype.AirborneCitadel => "[Sky]",
                _ => "[Unit]"
            };
        }
    }
}
