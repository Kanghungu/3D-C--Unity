using Game.Selection;
using Game.Units;
using System.Collections.Generic;
using System.Text;

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
                    MatchResult.Victory => "Status: Victory condition met.",
                    MatchResult.Defeat => "Status: Defeat condition met.",
                    _ => BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits)
                };
            }

            return BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits);
        }

        public static string BuildControlGroupLine(PrototypeSelectionController selectionController)
        {
            string summary = selectionController != null ? selectionController.GetControlGroupSummary() : "None";
            return $"Ctrl+F1-F5 assign | F1-F5 recall | double-tap focus | Groups {summary}";
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
            string playerDirective = directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Player)
                ? "P assault ON"
                : "P assault locked";
            string enemyDirective = directiveController != null && directiveController.IsTotalAssaultActive(UnitTeam.Enemy)
                ? "E assault ON"
                : "E assault locked";

            return $"Nodes P:{playerOwned}/{totalNodes} E:{enemyOwned}/{totalNodes} | Score P:{playerScore}/{totalScore} E:{enemyScore}/{totalScore} | {playerDirective} | {enemyDirective}";
        }

        public static string BuildStrategicPressureLine(BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            if (directiveController == null || controlNodes == null || controlNodes.Count == 0)
            {
                return "Frontline pressure data unavailable.";
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

            return $"Pressure P:{directiveController.GetStrategicPressureLabel(UnitTeam.Player)} / E:{directiveController.GetStrategicPressureLabel(UnitTeam.Enemy)} | Grand P:{grandPlayer} E:{grandEnemy} | Major P:{majorPlayer} E:{majorEnemy}";
        }

        public static string BuildVictoryBody(BaseStructure enemyBase, int enemyUnits)
        {
            if (enemyBase == null || !enemyBase.IsAlive)
            {
                return "The enemy bastion has fallen.";
            }

            if (enemyUnits == 0)
            {
                return "The enemy army has been wiped out.";
            }

            return "The enemy collapsed.";
        }

        public static string BuildDefeatBody(BaseStructure playerBase, int playerUnits)
        {
            if (playerBase == null || !playerBase.IsAlive)
            {
                return "Your sanctum has been destroyed.";
            }

            if (playerUnits == 0)
            {
                return "Your army has been wiped out.";
            }

            return "Your forces have collapsed.";
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
                return "Orders None";
            }

            List<KeyValuePair<string, int>> ordered = new(counts);
            ordered.Sort((left, right) => right.Value.CompareTo(left.Value));

            StringBuilder builder = new();
            builder.Append("Orders ");

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

                return $"Ability {unit.DisplayName}: {unit.AbilityStatus}";
            }

            return "Ability None";
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

                if (!counts.TryAdd(unit.DisplayName, 1))
                {
                    counts[unit.DisplayName]++;
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
                return "Production Missing structure";
            }

            string progress = $"{UnityEngine.Mathf.RoundToInt(structure.ProductionProgressNormalized * 100f)}%";
            string queuePreview = Shorten(structure.QueuePreview, 38);
            return $"{structure.StructureLabel} | {structure.QueueLabel} ({progress}) | {queuePreview}";
        }

        public static string BuildControlNodeLine(List<ControlNode> controlNodes)
        {
            if (controlNodes == null || controlNodes.Count == 0)
            {
                return "Holy Sites None";
            }

            List<ControlNode> orderedNodes = new(controlNodes);
            orderedNodes.RemoveAll(node => node == null);
            orderedNodes.Sort((left, right) => right.StrategicWeight.CompareTo(left.StrategicWeight));

            StringBuilder builder = new();
            builder.Append("Objectives ");
            int shown = 0;

            foreach (ControlNode node in orderedNodes)
            {
                if (shown > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(node.NodeLabel);
                builder.Append(" ");
                builder.Append(node.TierLabel);
                builder.Append(" ");
                builder.Append(node.OwnerLabel);
                shown++;

                if (shown >= 3)
                {
                    break;
                }
            }

            return Shorten(builder.ToString(), 66);
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
            if (enemyUnits > playerUnits + 80)
            {
                return "Enemy line is massing on the horizon.";
            }

            if (playerUnits > enemyUnits + 120)
            {
                return "Imperial crusade line advancing.";
            }

            if (enemyBase != null && enemyBase.CurrentPhase >= 3)
            {
                return "Enemy bastion is open.";
            }

            if (playerBase != null && playerBase.CurrentPhase >= 3)
            {
                return "Sanctum under pressure.";
            }

            return "Hold shrines and break the enemy front.";
        }
    }
}
