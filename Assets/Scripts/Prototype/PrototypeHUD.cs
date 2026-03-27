using Game.Units;
using Game.Selection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Lightweight debug HUD for the RTS prototype.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle overlayStyle;
        private Texture2D panelTexture;
        private Texture2D overlayTexture;

        private void OnGUI()
        {
            EnsureStyles();

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            ProductionStructure playerProduction = PrototypeRuntimeQuery.FindPlayerProductionStructure();
            ControlNode controlNode = PrototypeRuntimeQuery.FindControlNode();
            PrototypeGameDatabase database = PrototypeRuntimeQuery.FindDatabase();
            int playerUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int enemyUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);
            PrototypeMatchController matchController = FindAnyObjectByType<PrototypeMatchController>();

            DrawTopPanel(playerBase, enemyBase, playerProduction, controlNode, database, playerUnits, enemyUnits, matchController);
            DrawSelectionPanel();

            if (matchController != null && matchController.IsFinished)
            {
                DrawMatchOverlay(matchController.Result, playerBase, enemyBase, playerUnits, enemyUnits);
            }
        }

        private void DrawTopPanel(
            BaseStructure playerBase,
            BaseStructure enemyBase,
            ProductionStructure playerProduction,
            ControlNode controlNode,
            PrototypeGameDatabase database,
            int playerUnits,
            int enemyUnits,
            PrototypeMatchController matchController)
        {
            Rect panel = new Rect(10f, 10f, 520f, 208f);
            GUI.Box(panel, GUIContent.none, panelStyle);

            string status = BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits);
            string production = playerProduction == null ? "Unavailable" : playerProduction.QueueLabel;
            string progress = playerProduction == null ? "0%" : $"{Mathf.RoundToInt(playerProduction.ProductionProgressNormalized * 100f)}%";
            string queuePreview = playerProduction == null ? "Idle" : Shorten(playerProduction.QueuePreview, 58);
            string rally = playerProduction == null ? "Unavailable" : playerProduction.RallyLabel;
            string options = BuildProductionOptions(database);
            string nodeLabel = controlNode == null ? "None" : controlNode.OwnerLabel;
            string bonusLabel = controlNode == null ? "x1.00" : $"x{controlNode.BonusMultiplier:0.00}";

            GUI.Label(new Rect(20f, 18f, 220f, 20f), "Battle HUD", titleStyle);
            GUI.Label(new Rect(20f, 40f, 470f, 18f), $"Units  P:{playerUnits}  E:{enemyUnits}", labelStyle);
            GUI.Label(new Rect(20f, 58f, 470f, 18f), $"Victory rule: destroy enemy base OR wipe enemy army. Lose if your base OR army falls.", labelStyle);
            GUI.Label(new Rect(20f, 76f, 470f, 18f), $"Base   P:{ToPercent(playerBase)}  E:{ToPercent(enemyBase)}", labelStyle);
            GUI.Label(new Rect(20f, 94f, 470f, 18f), $"Foundry production  {production} ({progress})", labelStyle);
            GUI.Label(new Rect(20f, 112f, 470f, 18f), $"Queue  {queuePreview}", labelStyle);
            GUI.Label(new Rect(20f, 130f, 470f, 18f), $"Rally  {rally}  |  Alt+RMB on ground updates where new units run.", labelStyle);
            GUI.Label(new Rect(20f, 148f, 470f, 18f), $"Control Node  {nodeLabel}  |  Production bonus {bonusLabel}", labelStyle);
            GUI.Label(new Rect(20f, 166f, 470f, 18f), status, labelStyle);
            GUI.Label(new Rect(20f, 184f, 490f, 18f), $"Foundry: [1][2][3] queue units | [F] ability | [A+RMB] attack move | {options}", labelStyle);
        }

        private void DrawSelectionPanel()
        {
            PrototypeSelectionController selectionController = PrototypeSelectionController.Instance;

            if (selectionController == null)
            {
                return;
            }

            selectionController.RemoveDestroyedSelections();
            IReadOnlyList<SelectableUnit> selectedUnits = selectionController.SelectedUnits;

            if (selectedUnits.Count == 0)
            {
                return;
            }

            Rect panel = new Rect(10f, Screen.height - 130f, 380f, 110f);
            GUI.Box(panel, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 10f, 220f, 18f), $"Selection ({selectedUnits.Count})", titleStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 30f, 350f, 18f), BuildSelectionSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 48f, 350f, 18f), BuildSelectionOrdersSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 66f, 350f, 18f), BuildSelectionRangeSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 84f, 350f, 18f), BuildSelectionAbilitySummary(selectedUnits), labelStyle);
        }

        private void DrawMatchOverlay(MatchResult result, BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            Rect overlay = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 76f, 420f, 152f);
            GUI.Box(overlay, GUIContent.none, overlayStyle);
            string title = result == MatchResult.Victory ? "Victory" : "Defeat";
            string body = result == MatchResult.Victory
                ? BuildVictoryBody(enemyBase, enemyUnits)
                : BuildDefeatBody(playerBase, playerUnits);

            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 24f, 320f, 24f), title, titleStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 56f, 360f, 20f), body, labelStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 84f, 360f, 20f), "Win by enemy base destruction or total enemy wipe.", labelStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 108f, 360f, 20f), "Press R to restart the battle.", labelStyle);
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelTexture = MakeTexture(new Color(0.19f, 0.14f, 0.09f, 0.78f));
            overlayTexture = MakeTexture(new Color(0.12f, 0.09f, 0.06f, 0.9f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTexture;
            panelStyle.border = new RectOffset(8, 8, 8, 8);

            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.normal.background = overlayTexture;
            overlayStyle.border = new RectOffset(8, 8, 8, 8);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.wordWrap = false;
            labelStyle.clipping = TextClipping.Clip;
            labelStyle.normal.textColor = new Color(0.98f, 0.94f, 0.86f);

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static string BuildStatus(PrototypeMatchController matchController, BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
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

        private static string BuildOngoingStatus(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            if (enemyUnits > playerUnits + 2)
            {
                return "Status: Enemy gunline is growing. Reinforce the center.";
            }

            if (enemyBase != null && enemyBase.HealthNormalized < 0.45f)
            {
                return "Status: Enemy bastion is vulnerable. Finish the assault.";
            }

            if (playerBase != null && playerBase.HealthNormalized < 0.5f)
            {
                return "Status: Your sanctum is under threat. Pull back and stabilize.";
            }

            return "Status: Hold the holy node, build at the foundry, then break the enemy line.";
        }

        private static string BuildVictoryBody(BaseStructure enemyBase, int enemyUnits)
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

        private static string BuildDefeatBody(BaseStructure playerBase, int playerUnits)
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

        private static string BuildSelectionSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            Dictionary<string, int> counts = new();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                string label = unit.DisplayName;

                if (!counts.TryAdd(label, 1))
                {
                    counts[label]++;
                }
            }

            StringBuilder builder = new();
            bool isFirst = true;

            foreach ((string label, int count) in counts)
            {
                if (!isFirst)
                {
                    builder.Append(" | ");
                }

                builder.Append(label);
                builder.Append(':');
                builder.Append(count);
                isFirst = false;
            }

            return builder.Length == 0 ? "No valid units" : Shorten(builder.ToString(), 44);
        }

        private static string BuildSelectionOrdersSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            int attackers = 0;
            int movers = 0;
            int attackMovers = 0;
            int validUnits = 0;

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                validUnits++;
                UnitCombat combat = unit.GetComponent<UnitCombat>();
                SimpleUnitMover mover = unit.GetComponent<SimpleUnitMover>();

                if (combat != null && combat.CurrentTarget != null)
                {
                    attackers++;
                }
                else if (combat != null && combat.HasAttackMoveDestination)
                {
                    attackMovers++;
                }
                else if (mover != null && mover.IsMoving)
                {
                    movers++;
                }
            }

            return $"Orders A:{attackers} AM:{attackMovers} M:{movers} I:{Mathf.Max(0, validUnits - attackers - attackMovers - movers)}";
        }

        private static string BuildSelectionRangeSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            float longestRange = 0f;
            string longestLabel = "None";

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit?.Definition == null)
                {
                    continue;
                }

                if (unit.Definition.AttackRange > longestRange)
                {
                    longestRange = unit.Definition.AttackRange;
                    longestLabel = unit.DisplayName;
                }
            }

            return $"Range {longestLabel} {longestRange:0.0}";
        }

        private static string BuildSelectionAbilitySummary(IReadOnlyList<SelectableUnit> selectedUnits)
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

        private static string BuildProductionOptions(PrototypeGameDatabase database)
        {
            if (database == null)
            {
                return "";
            }

            StringBuilder builder = new();
            int hotkey = 1;

            foreach (UnitDefinition definition in database.GetProductionOptions())
            {
                if (definition == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }

                builder.Append('[');
                builder.Append(hotkey++);
                builder.Append(']');
                builder.Append(definition.DisplayName[0]);
            }

            return builder.ToString();
        }

        private static string Shorten(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..Mathf.Max(0, maxLength - 3)] + "...";
        }

        private static string ToPercent(BaseStructure baseStructure)
        {
            if (baseStructure == null)
            {
                return "0%";
            }

            return $"{Mathf.RoundToInt(baseStructure.HealthNormalized * 100f)}%";
        }
    }
}
