using Game.Units;
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

        private void OnGUI()
        {
            EnsureStyles();

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            int playerUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int enemyUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);
            PrototypeMatchController matchController = FindAnyObjectByType<PrototypeMatchController>();

            DrawTopPanel(playerBase, enemyBase, playerUnits, enemyUnits, matchController);

            if (matchController != null && matchController.IsFinished)
            {
                DrawMatchOverlay(matchController.Result);
            }
        }

        private void DrawTopPanel(BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits, PrototypeMatchController matchController)
        {
            Rect panel = new Rect(12f, 12f, 420f, 240f);
            GUI.Box(panel, GUIContent.none, panelStyle);

            string status = BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits);
            string production = playerBase == null ? "Unavailable" : playerBase.QueueLabel;
            string progress = playerBase == null ? "0%" : $"{Mathf.RoundToInt(playerBase.ProductionProgressNormalized * 100f)}%";

            GUI.Label(new Rect(24f, 22f, 360f, 24f), "RTS Prototype", titleStyle);
            GUI.Label(new Rect(24f, 52f, 360f, 22f), $"Friendly units: {playerUnits}", labelStyle);
            GUI.Label(new Rect(24f, 74f, 360f, 22f), $"Enemy units: {enemyUnits}", labelStyle);
            GUI.Label(new Rect(24f, 96f, 360f, 22f), $"Player base HP: {ToPercent(playerBase)}", labelStyle);
            GUI.Label(new Rect(24f, 118f, 360f, 22f), $"Enemy base HP: {ToPercent(enemyBase)}", labelStyle);
            GUI.Label(new Rect(24f, 140f, 390f, 22f), $"Production queue: {production} ({progress})", labelStyle);
            GUI.Label(new Rect(24f, 162f, 390f, 22f), status, labelStyle);
            GUI.Label(new Rect(24f, 186f, 390f, 22f), "Controls: drag select, right-click move/attack, [1] Vanguard, [2] Skirmisher.", labelStyle);
            GUI.Label(new Rect(24f, 208f, 390f, 22f), "Goal: break the enemy base and survive the counterattack.", labelStyle);
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

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = Texture2D.whiteTexture;
            panelStyle.normal.textColor = Color.white;

            overlayStyle = new GUIStyle(panelStyle);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 20;
            titleStyle.fontStyle = FontStyle.Bold;
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

            if (enemyUnits > playerUnits)
            {
                return "Status: Enemy pressure is building. Produce more units.";
            }

            if (enemyBase != null && enemyBase.HealthNormalized < 0.5f)
            {
                return "Status: The enemy base is weakened. Push forward.";
            }

            return "Status: Live battle. Flank, reinforce, and break the enemy line.";
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
