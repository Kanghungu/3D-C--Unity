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
                DrawMatchOverlay(matchController.Result);
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
            Rect panel = new Rect(10f, 10f, 430f, 188f);
            GUI.Box(panel, GUIContent.none, panelStyle);

            string status = BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits);
            string production = playerProduction == null ? "Unavailable" : playerProduction.QueueLabel;
            string progress = playerProduction == null ? "0%" : $"{Mathf.RoundToInt(playerProduction.ProductionProgressNormalized * 100f)}%";
            string queuePreview = playerProduction == null ? "Idle" : Shorten(playerProduction.QueuePreview, 46);
            string rally = playerProduction == null ? "Unavailable" : playerProduction.RallyLabel;
            string options = BuildProductionOptions(database);
            string nodeLabel = controlNode == null ? "None" : controlNode.OwnerLabel;
            string bonusLabel = controlNode == null ? "x1.00" : $"x{controlNode.BonusMultiplier:0.00}";

            GUI.Label(new Rect(20f, 18f, 220f, 20f), "Battle HUD", titleStyle);
            GUI.Label(new Rect(20f, 40f, 390f, 18f), $"Units  P:{playerUnits}  E:{enemyUnits}", labelStyle);
            GUI.Label(new Rect(20f, 58f, 390f, 18f), $"Base   P:{ToPercent(playerBase)}  E:{ToPercent(enemyBase)}", labelStyle);
            GUI.Label(new Rect(20f, 76f, 390f, 18f), $"Foundry {production} ({progress})", labelStyle);
            GUI.Label(new Rect(20f, 94f, 390f, 18f), $"Queue  {queuePreview}", labelStyle);
            GUI.Label(new Rect(20f, 112f, 390f, 18f), $"Rally  {rally}", labelStyle);
            GUI.Label(new Rect(20f, 130f, 390f, 18f), $"Node   {nodeLabel}  Bonus {bonusLabel}", labelStyle);
            GUI.Label(new Rect(20f, 148f, 390f, 18f), status, labelStyle);
            GUI.Label(new Rect(20f, 166f, 400f, 18f), $"[1][2][3] Build | [A+RMB] Attack Move | [Alt+RMB] Rally | {options}", labelStyle);
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

            Rect panel = new Rect(10f, Screen.height - 112f, 320f, 92f);
            GUI.Box(panel, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 10f, 200f, 18f), $"Selection ({selectedUnits.Count})", titleStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 30f, 290f, 18f), BuildSelectionSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 48f, 290f, 18f), BuildSelectionOrdersSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 10f, panel.y + 66f, 290f, 18f), BuildSelectionRangeSummary(selectedUnits), labelStyle);
        }

        private void DrawMatchOverlay(MatchResult result)
        {
            Rect overlay = new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 70f, 360f, 140f);
            GUI.Box(overlay, GUIContent.none, overlayStyle);
            string title = result == MatchResult.Victory ? "Victory" : "Defeat";
            string body = result == MatchResult.Victory
                ? "The enemy base and army collapsed."
                : "Your base and remaining army were destroyed.";

            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 24f, 280f, 24f), title, titleStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 56f, 300f, 20f), body, labelStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 84f, 300f, 20f), "Press R to restart the battle.", labelStyle);
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelTexture = MakeTexture(new Color(0.05f, 0.08f, 0.12f, 0.74f));
            overlayTexture = MakeTexture(new Color(0.03f, 0.03f, 0.04f, 0.88f));

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
            labelStyle.normal.textColor = Color.white;

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
                    MatchResult.Victory => "Status: Victory.",
                    MatchResult.Defeat => "Status: Defeat.",
                    _ => BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits)
                };
            }

            return BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits);
        }

        private static string BuildOngoingStatus(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            if (enemyUnits > playerUnits + 1)
            {
                return "Status: Enemy pressure is rising.";
            }

            if (enemyBase != null && enemyBase.HealthNormalized < 0.5f)
            {
                return "Status: Enemy base is weakened.";
            }

            if (playerBase != null && playerBase.HealthNormalized < 0.5f)
            {
                return "Status: Your base is damaged. Stabilize the line.";
            }

            return "Status: Capture the center and push forward.";
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

            return builder.Length == 0 ? "No valid units" : Shorten(builder.ToString(), 42);
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
