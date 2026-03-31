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
        private GUIStyle eventBadgeTextStyle;
        private GUIStyle minimapGroupLabelStyle;
        private GUIStyle minimapGroupHighlightLabelStyle;
        private GUIStyle minimapGroupCountStyle;
        private GUIStyle minimapCommandLabelStyle;
        private Texture2D panelTexture;
        private Texture2D overlayTexture;
        private Texture2D whiteTexture;
        private Texture2D circularMarkerTexture;
        private PrototypeMatchController cachedMatchController;
        private BattlefieldMapProfile cachedMapProfile;
        private BattlefieldVisionController cachedVisionController;
        private RTSCameraController cachedCameraController;
        private readonly Vector2[] minimapViewportCorners = new Vector2[4];
        private readonly Vector3[] cameraViewportWorldCorners = new Vector3[4];
        private readonly List<PrototypeSelectionController.ControlGroupMarkerInfo> controlGroupMarkers = new();
        private readonly List<PrototypeSelectionController.SelectedControlGroupInfo> selectedControlGroupInfos = new();

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

            DrawMinimap(minimapRect, playerBase, playerProductions, controlNodes, selectionController != null ? selectionController.SelectedUnits : null);
            DrawOverviewPanel(overviewRect, playerBase, enemyBase, controlNodes, playerUnits, enemyUnits, matchController, directiveController);
            DrawProductionPanel(productionRect, playerProductions);
            DrawSelectionPanel(selectionRect, selectionController);
        }

        private void DrawMinimap(
            Rect rect,
            BaseStructure playerBase,
            List<ProductionStructure> playerProductions,
            List<ControlNode> controlNodes,
            IReadOnlyList<SelectableUnit> selectedUnits)
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
                DrawPlayerRallyNetwork(mapRect, mapProfile, playerBase, playerProductions);
                DrawCameraViewport(mapRect, mapProfile);
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

            DrawControlGroupMarkers(mapRect, mapProfile);
            DrawCommandMarkerOverlay(mapRect, mapProfile, selectedUnits);
            DrawSelectedUnitsOverlay(mapRect, mapProfile, selectedUnits);
            GUI.Label(new Rect(rect.x + 88f, rect.y + 6f, rect.width - 96f, 18f), "LMB drag / jump", tinyStyle);
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

        private void DrawSelectionPanel(Rect rect, PrototypeSelectionController selectionController)
        {
            IReadOnlyList<SelectableUnit> selectedUnits = selectionController != null ? selectionController.SelectedUnits : null;
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, 120f, 18f), "Selection", titleStyle);

            if (selectedUnits == null || selectedUnits.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 32f, rect.width - 16f, 18f), "No unit selected.", labelStyle);
                GUI.Label(new Rect(rect.x + 8f, rect.y + 54f, rect.width - 16f, 18f), "Select units to see command and ability info.", tinyStyle);
                DrawSelectionEventBadges(new Rect(rect.x + 8f, rect.y + 78f, rect.width - 16f, 18f), selectionController);
                GUI.Label(new Rect(rect.x + 8f, rect.y + 118f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildControlGroupLine(selectionController), tinyStyle);
                GUI.Label(new Rect(rect.x + 8f, rect.y + 136f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildCommandControlLine(selectionController), tinyStyle);
                return;
            }

            GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 18f), $"Selected {selectedUnits.Count}", labelStyle);
            DrawSelectionEventBadges(new Rect(rect.x + 102f, rect.y + 28f, rect.width - 110f, 18f), selectionController);
            DrawSelectionBadges(new Rect(rect.x + 8f, rect.y + 50f, rect.width - 16f, 42f), selectedUnits);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 100f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildSelectionOrdersSummary(selectedUnits), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 120f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildSelectionAbilitySummary(selectedUnits), tinyStyle);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 136f, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildCommandControlLine(selectionController), tinyStyle);
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

        private void DrawSelectionEventBadges(Rect rect, PrototypeSelectionController selectionController)
        {
            if (selectionController == null || rect.width <= 36f)
            {
                return;
            }

            List<(string label, Color fill, Color outline)> eventBadges = new(3);

            if (!string.IsNullOrWhiteSpace(selectionController.MoveMarkerLabel))
            {
                Color commandColor = selectionController.MoveMarkerColor;
                eventBadges.Add((
                    selectionController.MoveMarkerLabel,
                    new Color(commandColor.r, commandColor.g, commandColor.b, 0.22f),
                    new Color(commandColor.r, commandColor.g, commandColor.b, 0.88f)));
            }

            if (selectionController.HasRecentControlGroupAssignment)
            {
                eventBadges.Add((
                    $"Set {selectionController.RecentControlGroupAssignmentLabel}",
                    new Color(0.3f, 1f, 0.92f, 0.2f),
                    new Color(0.42f, 1f, 0.92f, 0.88f)));
            }
            else if (selectionController.HasRecentControlGroupRecall)
            {
                eventBadges.Add((
                    $"Focus {selectionController.RecentControlGroupRecallLabel}",
                    new Color(1f, 0.95f, 0.52f, 0.2f),
                    new Color(1f, 0.95f, 0.62f, 0.88f)));
            }

            selectionController.GetSelectedControlGroupInfos(selectedControlGroupInfos);

            foreach (PrototypeSelectionController.SelectedControlGroupInfo groupInfo in selectedControlGroupInfos)
            {
                string groupLabel = $"F{groupInfo.GroupIndex} {groupInfo.SelectedCount}/{groupInfo.TotalCount}";
                Color fillColor = groupInfo.IsFullySelected
                    ? new Color(0.4f, 0.72f, 1f, 0.24f)
                    : new Color(0.34f, 0.6f, 1f, 0.18f);
                Color outlineColor = groupInfo.IsFullySelected
                    ? new Color(0.62f, 0.82f, 1f, 0.92f)
                    : new Color(0.48f, 0.72f, 1f, 0.84f);
                eventBadges.Add((
                    groupLabel,
                    fillColor,
                    outlineColor));
            }

            if (eventBadges.Count == 0)
            {
                return;
            }

            float badgeX = rect.x;
            const float badgeHeight = 16f;
            const float gap = 6f;

            foreach ((string label, Color fill, Color outline) eventBadge in eventBadges)
            {
                Vector2 textSize = eventBadgeTextStyle.CalcSize(new GUIContent(eventBadge.label));
                float badgeWidth = Mathf.Max(42f, textSize.x + 14f);

                if (badgeX + badgeWidth > rect.xMax)
                {
                    break;
                }

                Rect badgeRect = new(badgeX, rect.y, badgeWidth, badgeHeight);
                DrawSolidRect(badgeRect, eventBadge.fill);
                DrawRectOutline(badgeRect, eventBadge.outline, 1f);
                GUI.Label(badgeRect, eventBadge.label, eventBadgeTextStyle);
                badgeX += badgeWidth + gap;
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
            circularMarkerTexture = MakeCircleTexture(32);
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

            eventBadgeTextStyle = new GUIStyle(tinyStyle);
            eventBadgeTextStyle.fontSize = 10;
            eventBadgeTextStyle.fontStyle = FontStyle.Bold;
            eventBadgeTextStyle.alignment = TextAnchor.MiddleCenter;
            eventBadgeTextStyle.normal.textColor = new Color(0.98f, 0.96f, 0.92f);

            minimapGroupLabelStyle = new GUIStyle(tinyStyle);
            minimapGroupLabelStyle.alignment = TextAnchor.MiddleCenter;
            minimapGroupLabelStyle.fontStyle = FontStyle.Bold;
            minimapGroupLabelStyle.normal.textColor = new Color(0.93f, 0.97f, 1f);

            minimapGroupHighlightLabelStyle = new GUIStyle(minimapGroupLabelStyle);
            minimapGroupHighlightLabelStyle.normal.textColor = new Color(0.18f, 0.14f, 0.08f);

            minimapGroupCountStyle = new GUIStyle(tinyStyle);
            minimapGroupCountStyle.fontSize = 9;
            minimapGroupCountStyle.alignment = TextAnchor.MiddleCenter;
            minimapGroupCountStyle.normal.textColor = new Color(0.96f, 0.96f, 0.9f, 0.94f);

            minimapCommandLabelStyle = new GUIStyle(tinyStyle);
            minimapCommandLabelStyle.fontSize = 10;
            minimapCommandLabelStyle.fontStyle = FontStyle.Bold;
            minimapCommandLabelStyle.alignment = TextAnchor.MiddleCenter;
            minimapCommandLabelStyle.normal.textColor = new Color(0.98f, 0.95f, 0.9f);
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D MakeCircleTexture(int size)
        {
            Texture2D texture = new(size, size);
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = distance <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

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

        private void DrawCircularMarker(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, circularMarkerTexture);
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

        private void DrawCameraViewport(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            RTSCameraController cameraController = GetCameraController();
            if (cameraController == null || mapProfile == null)
            {
                return;
            }

            Vector2 minPoint = new(float.MaxValue, float.MaxValue);
            Vector2 maxPoint = new(float.MinValue, float.MinValue);

            if (!cameraController.TryGetViewportGroundCorners(cameraViewportWorldCorners))
            {
                return;
            }

            for (int i = 0; i < cameraViewportWorldCorners.Length; i++)
            {
                Vector3 worldPoint = mapProfile.ClampWorldPoint(cameraViewportWorldCorners[i], 2f);
                Vector2 normalized = mapProfile.WorldToNormalized(worldPoint);
                Vector2 minimapPoint = new(
                    mapRect.x + normalized.x * mapRect.width,
                    mapRect.y + normalized.y * mapRect.height);

                minimapViewportCorners[i] = minimapPoint;
                minPoint.x = Mathf.Min(minPoint.x, minimapPoint.x);
                minPoint.y = Mathf.Min(minPoint.y, minimapPoint.y);
                maxPoint.x = Mathf.Max(maxPoint.x, minimapPoint.x);
                maxPoint.y = Mathf.Max(maxPoint.y, minimapPoint.y);
            }

            Rect fillRect = Rect.MinMaxRect(minPoint.x, minPoint.y, maxPoint.x, maxPoint.y);
            DrawSolidRect(fillRect, new Color(1f, 1f, 1f, 0.05f));

            for (int i = 0; i < minimapViewportCorners.Length; i++)
            {
                Vector2 start = minimapViewportCorners[i];
                Vector2 end = minimapViewportCorners[(i + 1) % minimapViewportCorners.Length];
                DrawLine(start, end, new Color(1f, 1f, 1f, 0.9f), 1.75f);
            }
        }

        private void DrawPlayerRallyNetwork(Rect mapRect, BattlefieldMapProfile mapProfile, BaseStructure playerBase, List<ProductionStructure> playerProductions)
        {
            if (mapProfile == null)
            {
                return;
            }

            Color baseLineColor = new(0.42f, 0.96f, 1f, 0.82f);
            Color basePointColor = new(0.62f, 1f, 1f, 0.96f);

            if (playerBase != null && playerBase.IsAlive && playerBase.HasRallyPoint)
            {
                DrawMapLine(mapRect, playerBase.transform.position, playerBase.RallyPoint, mapProfile, baseLineColor, 1.8f);
                DrawMapPoint(mapRect, playerBase.RallyPoint, mapProfile, basePointColor, 6f);
            }

            if (playerProductions == null)
            {
                return;
            }

            for (int i = 0; i < playerProductions.Count; i++)
            {
                ProductionStructure structure = playerProductions[i];
                if (structure == null || !structure.IsAlive || !structure.HasRallyPoint)
                {
                    continue;
                }

                Color rallyLineColor = i % 2 == 0
                    ? new Color(0.3f, 0.9f, 0.78f, 0.78f)
                    : new Color(0.4f, 0.82f, 1f, 0.78f);
                Color rallyPointColor = i % 2 == 0
                    ? new Color(0.56f, 1f, 0.88f, 0.94f)
                    : new Color(0.6f, 0.9f, 1f, 0.94f);

                DrawMapLine(mapRect, structure.transform.position, structure.RallyPoint, mapProfile, rallyLineColor, 1.3f);
                DrawMapPoint(mapRect, structure.RallyPoint, mapProfile, rallyPointColor, 4.5f);
            }
        }

        private void DrawSelectedUnitsOverlay(Rect mapRect, BattlefieldMapProfile mapProfile, IReadOnlyList<SelectableUnit> selectedUnits)
        {
            if (mapProfile == null || selectedUnits == null || selectedUnits.Count == 0)
            {
                return;
            }

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);
            int validCount = 0;

            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                validCount++;
                Vector2 normalized = mapProfile.WorldToNormalized(unit.transform.position);
                Vector2 minimapPoint = new(
                    mapRect.x + normalized.x * mapRect.width,
                    mapRect.y + normalized.y * mapRect.height);

                min.x = Mathf.Min(min.x, minimapPoint.x);
                min.y = Mathf.Min(min.y, minimapPoint.y);
                max.x = Mathf.Max(max.x, minimapPoint.x);
                max.y = Mathf.Max(max.y, minimapPoint.y);
                DrawSolidRect(new Rect(minimapPoint.x - 2f, minimapPoint.y - 2f, 4f, 4f), new Color(0.92f, 1f, 0.4f, 0.98f));
            }

            if (validCount <= 0)
            {
                return;
            }

            if (validCount == 1)
            {
                Rect singleRect = Rect.MinMaxRect(min.x - 5f, min.y - 5f, max.x + 5f, max.y + 5f);
                DrawRectOutline(singleRect, new Color(0.95f, 1f, 0.55f, 0.95f), 1.4f);
                return;
            }

            Rect selectionBounds = Rect.MinMaxRect(min.x - 4f, min.y - 4f, max.x + 4f, max.y + 4f);
            DrawSolidRect(selectionBounds, new Color(0.9f, 1f, 0.45f, 0.04f));
            DrawRectOutline(selectionBounds, new Color(0.94f, 1f, 0.52f, 0.92f), 1.35f);
        }

        private void DrawCommandMarkerOverlay(Rect mapRect, BattlefieldMapProfile mapProfile, IReadOnlyList<SelectableUnit> selectedUnits)
        {
            if (mapProfile == null)
            {
                return;
            }

            PrototypeSelectionController selectionController = PrototypeSelectionController.Instance;
            if (selectionController == null || !selectionController.HasVisibleMoveMarker)
            {
                return;
            }

            Color markerColor = selectionController.MoveMarkerColor;

            if (selectedUnits != null && selectedUnits.Count > 0)
            {
                Vector3 selectionCenter = PrototypeSelectionUtility.GetSelectionCenter(selectedUnits);
                DrawMapLine(mapRect, selectionCenter, selectionController.MoveMarkerWorldPosition, mapProfile, new Color(markerColor.r, markerColor.g, markerColor.b, 0.55f), 1.15f);
            }

            DrawMapPoint(mapRect, selectionController.MoveMarkerWorldPosition, mapProfile, markerColor, 7f);
            Vector2 normalized = mapProfile.WorldToNormalized(selectionController.MoveMarkerWorldPosition);
            Vector2 center = new(
                mapRect.x + normalized.x * mapRect.width,
                mapRect.y + normalized.y * mapRect.height);
            Rect markerRect = Rect.MinMaxRect(center.x - 6f, center.y - 6f, center.x + 6f, center.y + 6f);
            DrawRectOutline(markerRect, markerColor, 1.2f);

            string markerLabel = selectionController.MoveMarkerLabel;
            if (!string.IsNullOrWhiteSpace(markerLabel))
            {
                Vector2 labelSize = minimapCommandLabelStyle.CalcSize(new GUIContent(markerLabel));
                float labelWidth = Mathf.Max(30f, labelSize.x + 12f);
                float labelHeight = 16f;
                float labelX = Mathf.Clamp(center.x - labelWidth * 0.5f, mapRect.xMin, mapRect.xMax - labelWidth);
                float preferredY = center.y - 20f;
                float fallbackY = center.y + 8f;
                float labelY = preferredY >= mapRect.yMin ? preferredY : Mathf.Min(fallbackY, mapRect.yMax - labelHeight);
                Rect labelRect = new(labelX, labelY, labelWidth, labelHeight);
                DrawSolidRect(labelRect, new Color(0.12f, 0.09f, 0.06f, 0.82f));
                DrawRectOutline(labelRect, new Color(markerColor.r, markerColor.g, markerColor.b, 0.9f), 1f);
                GUI.Label(labelRect, markerLabel, minimapCommandLabelStyle);
            }
        }

        private void DrawControlGroupMarkers(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            if (mapProfile == null)
            {
                return;
            }

            PrototypeSelectionController selectionController = PrototypeSelectionController.Instance;
            if (selectionController == null)
            {
                return;
            }

            selectionController.GetControlGroupMarkers(controlGroupMarkers);
            if (controlGroupMarkers.Count == 0)
            {
                return;
            }

            foreach (PrototypeSelectionController.ControlGroupMarkerInfo marker in controlGroupMarkers)
            {
                Vector2 normalized = mapProfile.WorldToNormalized(marker.WorldCenter);
                Vector2 center = new(
                    mapRect.x + normalized.x * mapRect.width,
                    mapRect.y + normalized.y * mapRect.height);

                float markerSize = marker.IsActive ? 15f : 12f;
                Rect markerRect = Rect.MinMaxRect(
                    center.x - markerSize * 0.5f,
                    center.y - markerSize * 0.5f,
                    center.x + markerSize * 0.5f,
                    center.y + markerSize * 0.5f);

                Color markerColor;
                if (marker.IsFullySelected)
                {
                    markerColor = new Color(1f, 0.95f, 0.52f, 0.96f);
                }
                else if (marker.HasSelectedMembers)
                {
                    float selectionMix = Mathf.Lerp(0.25f, 0.72f, marker.SelectionCoverage);
                    markerColor = Color.Lerp(
                        new Color(0.28f, 0.72f, 1f, 0.9f),
                        new Color(1f, 0.95f, 0.52f, 0.96f),
                        selectionMix);
                }
                else
                {
                    markerColor = new Color(0.28f, 0.72f, 1f, 0.88f);
                }

                if (marker.RecallEmphasis > 0f)
                {
                    float pulseExpansion = Mathf.Lerp(10f, 3f, marker.RecallEmphasis);
                    float pulseSize = markerSize + pulseExpansion;
                    Rect pulseRect = Rect.MinMaxRect(
                        center.x - pulseSize * 0.5f,
                        center.y - pulseSize * 0.5f,
                        center.x + pulseSize * 0.5f,
                        center.y + pulseSize * 0.5f);
                    Color pulseColor = new Color(markerColor.r, markerColor.g, markerColor.b, 0.18f + marker.RecallEmphasis * 0.45f);
                    DrawRectOutline(pulseRect, pulseColor, 1.25f);
                }

                if (marker.AssignmentEmphasis > 0f)
                {
                    float glowExpansion = Mathf.Lerp(8f, 2f, marker.AssignmentEmphasis);
                    float glowSize = markerSize + glowExpansion;
                    Rect glowRect = Rect.MinMaxRect(
                        center.x - glowSize * 0.5f,
                        center.y - glowSize * 0.5f,
                        center.x + glowSize * 0.5f,
                        center.y + glowSize * 0.5f);
                    DrawCircularMarker(glowRect, new Color(0.3f, 1f, 0.92f, 0.12f + marker.AssignmentEmphasis * 0.28f));
                }

                DrawCircularMarker(markerRect, markerColor);
                GUI.Label(
                    markerRect,
                    $"F{marker.GroupIndex}",
                    marker.IsActive ? minimapGroupHighlightLabelStyle : minimapGroupLabelStyle);

                if (marker.HasSelectedMembers)
                {
                    string countLabel = $"{marker.SelectedCount}/{marker.UnitCount}";
                    float countWidth = Mathf.Max(24f, minimapGroupCountStyle.CalcSize(new GUIContent(countLabel)).x + 8f);
                    Rect countRect = new(
                        center.x - countWidth * 0.5f,
                        markerRect.yMax + 1f,
                        countWidth,
                        12f);
                    DrawSolidRect(countRect, new Color(0.08f, 0.07f, 0.05f, 0.72f));
                    GUI.Label(countRect, countLabel, minimapGroupCountStyle);
                }
            }
        }

        private void TryHandleMinimapInput(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.button != 0)
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

            if (!IsMinimapNavigationEvent(currentEvent))
            {
                return;
            }

            RTSCameraController cameraController = GetCameraController();
            if (cameraController == null)
            {
                return;
            }

            cameraController.SnapToWorldPoint(worldPoint);
            currentEvent.Use();
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            float length = Vector2.Distance(start, end);
            if (length <= 0.01f)
            {
                return;
            }

            Color previousColor = GUI.color;
            Matrix4x4 previousMatrix = GUI.matrix;
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y - thickness * 0.5f, length, thickness), whiteTexture);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private void DrawMapLine(Rect mapRect, Vector3 worldStart, Vector3 worldEnd, BattlefieldMapProfile mapProfile, Color color, float thickness)
        {
            if (mapProfile == null)
            {
                return;
            }

            Vector2 normalizedStart = mapProfile.WorldToNormalized(worldStart);
            Vector2 normalizedEnd = mapProfile.WorldToNormalized(worldEnd);
            Vector2 start = new(
                mapRect.x + normalizedStart.x * mapRect.width,
                mapRect.y + normalizedStart.y * mapRect.height);
            Vector2 end = new(
                mapRect.x + normalizedEnd.x * mapRect.width,
                mapRect.y + normalizedEnd.y * mapRect.height);
            DrawLine(start, end, color, thickness);
        }

        private void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            Vector2 topLeft = new(rect.xMin, rect.yMin);
            Vector2 topRight = new(rect.xMax, rect.yMin);
            Vector2 bottomRight = new(rect.xMax, rect.yMax);
            Vector2 bottomLeft = new(rect.xMin, rect.yMax);
            DrawLine(topLeft, topRight, color, thickness);
            DrawLine(topRight, bottomRight, color, thickness);
            DrawLine(bottomRight, bottomLeft, color, thickness);
            DrawLine(bottomLeft, topLeft, color, thickness);
        }

        private static bool IsMinimapNavigationEvent(Event currentEvent)
        {
            return currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag;
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

        private RTSCameraController GetCameraController()
        {
            if (cachedCameraController == null && Camera.main != null)
            {
                cachedCameraController = Camera.main.GetComponent<RTSCameraController>();
            }

            return cachedCameraController;
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
