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
            int playerUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int enemyUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);
            PrototypeMatchController matchController = FindAnyObjectByType<PrototypeMatchController>();

            DrawTopPanel(playerBase, enemyBase, playerUnits, enemyUnits, matchController);
            DrawSelectionPanel();

            if (matchController != null && matchController.IsFinished)
            {
                DrawMatchOverlay(matchController.Result);
            }
        }

        private void DrawTopPanel(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits, PrototypeMatchController matchController)
        {
            Rect panel = new Rect(12f, 12f, 470f, 254f);
            GUI.Box(panel, GUIContent.none, panelStyle);

            string status = BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits);
            string production = playerBase == null ? "Unavailable" : playerBase.QueueLabel;
            string progress = playerBase == null ? "0%" : $"{Mathf.RoundToInt(playerBase.ProductionProgressNormalized * 100f)}%";
            string queuePreview = playerBase == null ? "Idle" : playerBase.QueuePreview;

            GUI.Label(new Rect(26f, 24f, 360f, 24f), "RTS Prototype", titleStyle);
            GUI.Label(new Rect(26f, 56f, 420f, 22f), $"Friendly units: {playerUnits} / Enemy units: {enemyUnits}", labelStyle);
            GUI.Label(new Rect(26f, 78f, 360f, 22f), $"Player base HP: {ToPercent(playerBase)}", labelStyle);
            GUI.Label(new Rect(26f, 100f, 360f, 22f), $"Enemy base HP: {ToPercent(enemyBase)}", labelStyle);
            GUI.Label(new Rect(26f, 122f, 420f, 22f), $"Currently producing: {production} ({progress})", labelStyle);
            GUI.Label(new Rect(26f, 144f, 420f, 38f), $"Queue: {queuePreview}", labelStyle);
            GUI.Label(new Rect(26f, 182f, 420f, 22f), status, labelStyle);
            GUI.Label(new Rect(26f, 204f, 420f, 22f), "Controls: [1]/[2] queue, Shift+[1]/[2] queue x3, Backspace cancels last.", labelStyle);
            GUI.Label(new Rect(26f, 226f, 420f, 22f), "Double-click a unit to select all friendly units of that role.", labelStyle);
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

            Rect panel = new Rect(12f, Screen.height - 170f, 340f, 148f);
            GUI.Box(panel, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 12f, 260f, 24f), "Selection", titleStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 42f, 280f, 22f), $"Selected units: {selectedUnits.Count}", labelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 64f, 300f, 22f), BuildSelectionSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 88f, 300f, 22f), BuildSelectionOrdersSummary(selectedUnits), labelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 112f, 300f, 22f), "Tip: mix Vanguard in front and Skirmisher behind.", labelStyle);
        }

        private void DrawMatchOverlay(MatchResult result)
        {
            Rect overlay = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 80f, 440f, 160f);
            GUI.Box(overlay, GUIContent.none, overlayStyle);
            string title = result == MatchResult.Victory ? "Victory" : "Defeat";
            string body = result == MatchResult.Victory
                ? "The enemy base and army collapsed."
                : "Your base and remaining army were destroyed.";

            GUI.Label(new Rect(overlay.x + 32f, overlay.y + 28f, 320f, 28f), title, titleStyle);
            GUI.Label(new Rect(overlay.x + 32f, overlay.y + 66f, 360f, 24f), body, labelStyle);
            GUI.Label(new Rect(overlay.x + 32f, overlay.y + 98f, 360f, 24f), "Press R to restart the battle.", labelStyle);
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelTexture = MakeTexture(new Color(0.05f, 0.08f, 0.12f, 0.78f));
            overlayTexture = MakeTexture(new Color(0.03f, 0.03f, 0.04f, 0.88f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTexture;
            panelStyle.border = new RectOffset(8, 8, 8, 8);

            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.normal.background = overlayTexture;
            overlayStyle.border = new RectOffset(8, 8, 8, 8);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.wordWrap = true;
            labelStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 20;
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
                    MatchResult.Victory => "Status: Victory. Your assault succeeded.",
                    MatchResult.Defeat => "Status: Defeat. The frontline collapsed.",
                    _ => BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits)
                };
            }

            return BuildOngoingStatus(playerBase, enemyBase, playerUnits, enemyUnits);
        }

        private static string BuildOngoingStatus(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            if (playerBase != null && playerBase.HasQueuedProduction)
            {
                return "Status: Reinforcements are being prepared at your base.";
            }

            if (enemyUnits > playerUnits + 1)
            {
                return "Status: Enemy pressure is rising. Queue more units and regroup.";
            }

            if (enemyBase != null && enemyBase.HealthNormalized < 0.5f)
            {
                return "Status: The enemy base is weakened. Push forward.";
            }

            return "Status: Live battle. Build momentum before the next enemy wave.";
        }

        private static string BuildSelectionSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            Dictionary<UnitArchetype, int> counts = new();

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                if (!counts.TryAdd(unit.Archetype, 1))
                {
                    counts[unit.Archetype]++;
                }
            }

            StringBuilder builder = new();
            bool isFirst = true;

            foreach ((UnitArchetype archetype, int count) in counts)
            {
                if (!isFirst)
                {
                    builder.Append(" | ");
                }

                builder.Append(archetype);
                builder.Append(": ");
                builder.Append(count);
                isFirst = false;
            }

            return builder.Length == 0 ? "No valid units" : builder.ToString();
        }

        private static string BuildSelectionOrdersSummary(IReadOnlyList<SelectableUnit> selectedUnits)
        {
            int attackers = 0;
            int movers = 0;
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
                else if (mover != null && mover.IsMoving)
                {
                    movers++;
                }
            }

            return $"Orders: attacking {attackers}, moving {movers}, idle {Mathf.Max(0, validUnits - attackers - movers)}";
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
