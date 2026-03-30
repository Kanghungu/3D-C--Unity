using Game.CameraSystem;
using Game.Selection;
using Game.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    /// <summary>
    /// Bottom command bar HUD with a lightweight minimap and production panels.
    /// </summary>
    public class PrototypeHUD : MonoBehaviour
    {
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle overlayStyle;
        private GUIStyle tinyStyle;
        private GUIStyle badgeStyle;
        private Texture2D panelTexture;
        private Texture2D overlayTexture;
        private Texture2D whiteTexture;
        private PrototypeMatchController cachedMatchController;
        private BattlefieldMapProfile cachedMapProfile;
        private BattlefieldVisionController cachedVisionController;

        private void OnGUI()
        {
            EnsureStyles();

            BaseStructure playerBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Player);
            BaseStructure enemyBase = PrototypeRuntimeQuery.FindBase(UnitTeam.Enemy);
            List<ProductionStructure> playerProductions = PrototypeRuntimeQuery.FindPlayerProductionStructures();
            List<ControlNode> controlNodes = PrototypeRuntimeQuery.FindControlNodes();
            int playerUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int enemyUnits = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);
            PrototypeMatchController matchController = GetMatchController();
            PrototypeSelectionController selectionController = PrototypeSelectionController.Instance;
            BattleDirectiveController directiveController = BattleDirectiveController.Instance;

            if (selectionController != null)
            {
                selectionController.RemoveDestroyedSelections();
            }

            DrawBottomBar(playerBase, enemyBase, playerProductions, controlNodes, playerUnits, enemyUnits, matchController, selectionController, directiveController);
            DrawTopRightNews(directiveController);

            if (matchController != null && matchController.IsFinished)
            {
                DrawMatchOverlay(matchController.Result, playerBase, enemyBase, playerUnits, enemyUnits);
            }
        }

        private void DrawBottomBar(
            BaseStructure playerBase,
            BaseStructure enemyBase,
            List<ProductionStructure> playerProductions,
            List<ControlNode> controlNodes,
            int playerUnits,
            int enemyUnits,
            PrototypeMatchController matchController,
            PrototypeSelectionController selectionController,
            BattleDirectiveController directiveController)
        {
            float barHeight = 176f;
            Rect barRect = new Rect(12f, Screen.height - barHeight - 12f, Screen.width - 24f, barHeight);
            GUI.Box(barRect, GUIContent.none, panelStyle);

            Rect minimapRect = new Rect(barRect.x + 12f, barRect.y + 12f, 196f, 152f);
            Rect overviewRect = new Rect(minimapRect.xMax + 12f, barRect.y + 12f, 328f, 152f);
            Rect productionRect = new Rect(overviewRect.xMax + 12f, barRect.y + 12f, 424f, 152f);
            Rect selectionRect = new Rect(productionRect.xMax + 12f, barRect.y + 12f, Mathf.Max(220f, barRect.xMax - productionRect.xMax - 24f), 152f);

            DrawMinimap(minimapRect, controlNodes);
            DrawOverviewPanel(overviewRect, playerBase, enemyBase, controlNodes, playerUnits, enemyUnits, matchController, directiveController);
            DrawProductionPanel(productionRect, playerProductions);
            DrawSelectionPanel(selectionRect, selectionController != null ? selectionController.SelectedUnits : null);
        }

        private void DrawMinimap(Rect rect, List<ControlNode> controlNodes)
        {
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, 120f, 18f), "Minimap", titleStyle);

            Rect mapRect = new Rect(rect.x + 8f, rect.y + 26f, rect.width - 16f, rect.height - 34f);
            BattlefieldMapProfile mapProfile = GetMapProfile();
            BattlefieldVisionController visionController = GetVisionController();
            DrawSolidRect(mapRect, mapProfile != null ? mapProfile.MinimapBackgroundColor : new Color(0.17f, 0.14f, 0.1f, 0.95f));

            if (mapProfile != null)
            {
                TryHandleMinimapInput(mapRect, mapProfile);
                DrawVisionOverlay(mapRect, mapProfile, visionController);
                DrawCameraPoint(mapRect, mapProfile);
            }

            foreach (ControlNode node in controlNodes)
            {
                if (node == null || !ShouldDrawStaticOnMinimap(node.transform.position, visionController))
                {
                    continue;
                }

                DrawMapPoint(mapRect, node.transform.position, mapProfile, GetNodeColor(node), 12f);
            }

            foreach (BaseStructure baseStructure in PrototypeRuntimeRegistry.GetBaseStructures())
            {
                if (baseStructure == null || !ShouldDrawStaticOnMinimap(baseStructure.transform.position, visionController))
                {
                    continue;
                }

                Color color = baseStructure.Team == UnitTeam.Player ? new Color(0.3f, 0.92f, 1f) : new Color(1f, 0.42f, 0.2f);
                DrawMapPoint(mapRect, baseStructure.transform.position, mapProfile, color, 10f);
            }

            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null || !ShouldDrawUnitOnMinimap(unit, visionController))
                {
                    continue;
                }

                Color color = unit.Team == UnitTeam.Player ? new Color(0.72f, 0.94f, 1f, 0.9f) : new Color(1f, 0.68f, 0.38f, 0.9f);
                float size = unit.Archetype switch
                {
                    UnitArchetype.MobileFortress => 6f,
                    UnitArchetype.AirborneCitadel => 6f,
                    UnitArchetype.RoyalGuard => 4.5f,
                    _ => 2.5f
                };
                DrawMapPoint(mapRect, unit.transform.position, mapProfile, color, size);
            }

            GUI.Label(new Rect(rect.x + 88f, rect.y + 6f, rect.width - 96f, 18f), "LMB jump", tinyStyle);
        }

        private void DrawVisionOverlay(Rect mapRect, BattlefieldMapProfile mapProfile, BattlefieldVisionController visionController)
        {
            if (mapProfile == null || visionController == null)
            {
                return;
            }

            float cellWidth = mapRect.width / visionController.GridWidth;
            float cellHeight = mapRect.height / visionController.GridHeight;

            for (int y = 0; y < visionController.GridHeight; y++)
            {
                for (int x = 0; x < visionController.GridWidth; x++)
                {
                    if (visionController.IsCellVisible(x, y))
                    {
                        continue;
                    }

                    Color overlayColor = visionController.IsCellExplored(x, y)
                        ? mapProfile.MemoryColor
                        : mapProfile.ShroudColor;
                    Rect cellRect = new Rect(mapRect.x + x * cellWidth, mapRect.y + y * cellHeight, cellWidth + 1f, cellHeight + 1f);
                    DrawSolidRect(cellRect, overlayColor);
                }
            }
        }

        private void DrawOverviewPanel(Rect rect, BaseStructure playerBase, BaseStructure enemyBase, List<ControlNode> controlNodes, int playerUnits, int enemyUnits, PrototypeMatchController matchController, BattleDirectiveController directiveController)
        {
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, 160f, 18f), "Battle HUD", titleStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 18f), $"Map {GetMapLabel()}", labelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 46f, rect.width - 16f, 18f), $"Army  P:{playerUnits}  E:{enemyUnits}", labelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 64f, rect.width - 16f, 18f), $"Base  P:{ToPercent(playerBase)} ({GetPhaseLabel(playerBase)})  E:{ToPercent(enemyBase)} ({GetPhaseLabel(enemyBase)})", labelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 82f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 100f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildStrategicPressureLine(directiveController, controlNodes), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 118f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildControlNodeLine(controlNodes), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 136f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildDirectiveLine(directiveController, controlNodes), tinyStyle);
        }

        private void DrawProductionPanel(Rect rect, List<ProductionStructure> playerProductions)
        {
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, 160f, 18f), "Production", titleStyle);

            float lineY = rect.y + 28f;

            if (playerProductions.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), "No active production structure.", labelStyle);
                return;
            }

            foreach (ProductionStructure structure in playerProductions)
            {
                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildProductionLine(structure), tinyStyle);
                lineY += 18f;
                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), $"Rally {structure.RallyLabel}", tinyStyle);
                lineY += 18f;
            }
        }

        private void DrawSelectionPanel(Rect rect, IReadOnlyList<SelectableUnit> selectedUnits)
        {
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, 120f, 18f), "Selection", titleStyle);

            if (selectedUnits == null || selectedUnits.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 32f, rect.width - 16f, 18f), "No unit selected.", labelStyle);
                GUI.Label(new Rect(rect.x + 8f, rect.y + 54f, rect.width - 16f, 18f), "Select units to see command and ability info.", tinyStyle);
                return;
            }

            GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 18f), $"Selected {selectedUnits.Count}", labelStyle);
            DrawSelectionBadges(new Rect(rect.x + 8f, rect.y + 50f, rect.width - 16f, 42f), selectedUnits);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 100f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildSelectionOrdersSummary(selectedUnits), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 120f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildSelectionAbilitySummary(selectedUnits), tinyStyle);
        }

        private void DrawSelectionBadges(Rect rect, IReadOnlyList<SelectableUnit> selectedUnits)
        {
            List<string> badgeLines = PrototypeHudTextUtility.BuildSelectionGroupLines(selectedUnits, 6);
            float badgeX = rect.x;
            float badgeY = rect.y;
            float badgeHeight = 18f;
            float gap = 6f;

            foreach (string badge in badgeLines)
            {
                float width = Mathf.Min(rect.width, 16f + badge.Length * 6.4f);

                if (badgeX + width > rect.xMax)
                {
                    badgeX = rect.x;
                    badgeY += badgeHeight + 4f;
                }

                Rect badgeRect = new Rect(badgeX, badgeY, width, badgeHeight);
                GUI.Box(badgeRect, GUIContent.none, badgeStyle);
                GUI.Label(new Rect(badgeRect.x + 8f, badgeRect.y + 1f, badgeRect.width - 12f, badgeRect.height - 2f), badge, tinyStyle);
                badgeX += width + gap;

                if (badgeY + badgeHeight > rect.yMax)
                {
                    break;
                }
            }
        }

        private void DrawTopRightNews(BattleDirectiveController directiveController)
        {
            if (directiveController == null || !directiveController.HasNews)
            {
                return;
            }

            Rect newsRect = new Rect(Screen.width - 356f, 12f, 344f, 54f);
            GUI.Box(newsRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(newsRect.x + 10f, newsRect.y + 8f, 92f, 18f), "War News", titleStyle);
            GUI.Label(new Rect(newsRect.x + 10f, newsRect.y + 28f, newsRect.width - 20f, 18f), directiveController.CurrentNews, tinyStyle);
        }

        private void DrawMatchOverlay(MatchResult result, BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            Rect overlay = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 76f, 420f, 152f);
            GUI.Box(overlay, GUIContent.none, overlayStyle);
            string title = result == MatchResult.Victory ? "Victory" : "Defeat";
            string body = result == MatchResult.Victory ? PrototypeHudTextUtility.BuildVictoryBody(enemyBase, enemyUnits) : PrototypeHudTextUtility.BuildDefeatBody(playerBase, playerUnits);

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

            whiteTexture = MakeTexture(Color.white);
            panelTexture = MakeTexture(new Color(0.16f, 0.12f, 0.08f, 0.84f));
            overlayTexture = MakeTexture(new Color(0.12f, 0.09f, 0.06f, 0.9f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTexture;
            panelStyle.border = new RectOffset(8, 8, 8, 8);

            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.normal.background = overlayTexture;
            overlayStyle.border = new RectOffset(8, 8, 8, 8);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = new Color(0.98f, 0.94f, 0.86f);
            labelStyle.clipping = TextClipping.Clip;

            tinyStyle = new GUIStyle(labelStyle);
            tinyStyle.fontSize = 11;

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;

            badgeStyle = new GUIStyle(GUI.skin.box);
            badgeStyle.normal.background = MakeTexture(new Color(0.24f, 0.18f, 0.12f, 0.95f));
            badgeStyle.border = new RectOffset(6, 6, 6, 6);
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawMapPoint(Rect mapRect, Vector3 worldPosition, BattlefieldMapProfile mapProfile, Color color, float size)
        {
            if (mapProfile == null)
            {
                return;
            }

            Vector2 normalized = mapProfile.WorldToNormalized(worldPosition);
            Rect pointRect = new Rect(
                mapRect.x + normalized.x * mapRect.width - size * 0.5f,
                mapRect.y + normalized.y * mapRect.height - size * 0.5f,
                size,
                size);
            DrawSolidRect(pointRect, color);
        }

        private void DrawCameraPoint(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || mapProfile == null)
            {
                return;
            }

            DrawMapPoint(mapRect, mainCamera.transform.position, mapProfile, new Color(1f, 1f, 1f, 0.95f), 5f);
        }

        private void TryHandleMinimapInput(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
            {
                return;
            }

            if (!mapRect.Contains(currentEvent.mousePosition))
            {
                return;
            }

            float normalizedX = Mathf.InverseLerp(mapRect.xMin, mapRect.xMax, currentEvent.mousePosition.x);
            float normalizedY = Mathf.InverseLerp(mapRect.yMin, mapRect.yMax, currentEvent.mousePosition.y);
            Vector3 worldPoint = mapProfile.NormalizedToWorld(new Vector2(normalizedX, normalizedY));

            RTSCameraController cameraController = Camera.main != null ? Camera.main.GetComponent<RTSCameraController>() : null;
            if (cameraController != null)
            {
                cameraController.SnapToWorldPoint(worldPoint);
                currentEvent.Use();
            }
        }

        private PrototypeMatchController GetMatchController()
        {
            if (cachedMatchController == null)
            {
                cachedMatchController = FindAnyObjectByType<PrototypeMatchController>();
            }

            return cachedMatchController;
        }

        private BattlefieldMapProfile GetMapProfile()
        {
            if (cachedMapProfile == null)
            {
                cachedMapProfile = FindAnyObjectByType<BattlefieldMapProfile>();
            }

            return cachedMapProfile;
        }

        private BattlefieldVisionController GetVisionController()
        {
            if (cachedVisionController == null)
            {
                cachedVisionController = BattlefieldVisionController.Instance != null
                    ? BattlefieldVisionController.Instance
                    : FindAnyObjectByType<BattlefieldVisionController>();
            }

            return cachedVisionController;
        }

        private string GetMapLabel()
        {
            BattlefieldMapProfile mapProfile = GetMapProfile();
            return mapProfile != null ? mapProfile.MapLabel : "Prototype Front";
        }

        private static bool ShouldDrawStaticOnMinimap(Vector3 worldPosition, BattlefieldVisionController visionController)
        {
            return visionController == null || visionController.IsWorldExplored(worldPosition);
        }

        private static bool ShouldDrawUnitOnMinimap(SelectableUnit unit, BattlefieldVisionController visionController)
        {
            if (unit.Team == UnitTeam.Player)
            {
                return true;
            }

            return visionController == null || visionController.IsWorldVisible(unit.transform.position);
        }

        private static Color GetNodeColor(ControlNode node)
        {
            return node.OwnerTeam switch
            {
                UnitTeam.Player => new Color(0.28f, 0.9f, 1f),
                UnitTeam.Enemy => new Color(1f, 0.42f, 0.22f),
                _ => new Color(0.78f, 0.74f, 0.62f)
            };
        }

        private static string GetPhaseLabel(BaseStructure baseStructure)
        {
            return baseStructure != null ? $"P{baseStructure.CurrentPhase} {baseStructure.PhaseLabel}" : "Lost";
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
