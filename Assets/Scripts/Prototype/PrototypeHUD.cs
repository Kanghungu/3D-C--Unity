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
        private bool showStatsPanel;
        private PrototypeMatchController cachedMatchController;
        private BattlefieldMapProfile cachedMapProfile;
        private BattlefieldVisionController cachedVisionController;
        private RTSCameraController cachedCameraController;
        private readonly Vector2[] minimapViewportCorners = new Vector2[4];
        private readonly Vector3[] cameraViewportWorldCorners = new Vector3[4];
        private readonly List<PrototypeSelectionController.ControlGroupMarkerInfo> controlGroupMarkers = new();
        private readonly List<PrototypeSelectionController.SelectedControlGroupInfo> selectedControlGroupInfos = new();
        private readonly List<Vector3> combatClusterBuffer = new();

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
            {
                showStatsPanel = !showStatsPanel;
            }
        }

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

            DrawTopInfoBar(playerUnits, enemyUnits, playerBase, enemyBase);
            DrawBottomBar(playerBase, enemyBase, playerProductions, controlNodes, playerUnits, enemyUnits, matchController, selectionController, directiveController);
            DrawTopRightNews(directiveController);

            if (showStatsPanel)
            {
                DrawStatsPanel();
            }

            if (matchController != null && matchController.IsFinished)
            {
                DrawMatchOverlay(matchController.Result, playerBase, enemyBase, playerUnits, enemyUnits);
            }
        }

        private void DrawStatsPanel()
        {
            BattleStatsTracker stats = BattleStatsTracker.Instance;
            if (stats == null) return;

            int elapsed = Mathf.FloorToInt(stats.ElapsedSeconds);
            string timeLabel = $"{elapsed / 60:D2}:{elapsed % 60:D2}";

            const float panelW = 420f;
            const float panelH = 340f;
            float px = (Screen.width - panelW) * 0.5f;
            float py = (Screen.height - panelH) * 0.5f;
            Rect panel = new Rect(px, py, panelW, panelH);

            // 배경
            DrawSolidRect(panel, new Color(0.06f, 0.06f, 0.10f, 0.94f));
            DrawRectOutline(panel, new Color(0.55f, 0.50f, 0.28f, 0.85f), 2f);

            // 타이틀
            GUI.Label(new Rect(px + 16f, py + 12f, panelW - 32f, 24f), "전투 통계   [Tab 닫기]", titleStyle);
            GUI.Label(new Rect(px + 16f, py + 36f, panelW - 32f, 18f), $"경과 시간   {timeLabel}", tinyStyle);

            // 헤더 구분선
            DrawSolidRect(new Rect(px + 12f, py + 56f, panelW - 24f, 1f), new Color(0.55f, 0.50f, 0.28f, 0.5f));

            // 컬럼 헤더
            float col0 = px + 16f;
            float col1 = px + 180f;
            float col2 = px + 300f;
            float rowH = 22f;
            float rowY = py + 64f;

            GUI.Label(new Rect(col0, rowY, 160f, rowH), "병종", titleStyle);
            GUI.Label(new Rect(col1, rowY, 110f, rowH), "아군 처치", titleStyle);
            GUI.Label(new Rect(col2, rowY, 110f, rowH), "적 처치", titleStyle);
            rowY += rowH + 2f;
            DrawSolidRect(new Rect(px + 12f, rowY, panelW - 24f, 1f), new Color(0.4f, 0.4f, 0.4f, 0.4f));
            rowY += 4f;

            // 병종별 행
            (UnitArchetype arch, string label)[] rows =
            {
                (UnitArchetype.Spearman,       "창병"),
                (UnitArchetype.ShieldInfantry, "방패 보병"),
                (UnitArchetype.Rifleman,       "총병"),
                (UnitArchetype.Artillery,      "포병"),
                (UnitArchetype.SpecialWarrior, "특수 전사"),
                (UnitArchetype.RoyalGuard,     "친위대"),
                (UnitArchetype.Fighter,        "전투기"),
                (UnitArchetype.MobileFortress, "이동 거점"),
                (UnitArchetype.AirborneCitadel,"비행 거점"),
            };

            foreach ((UnitArchetype arch, string label) row in rows)
            {
                stats.PlayerKills.TryGetValue(row.arch, out int pKill);
                stats.EnemyKills.TryGetValue(row.arch, out int eKill);
                if (pKill == 0 && eKill == 0) continue;

                GUI.Label(new Rect(col0, rowY, 160f, rowH), row.label, labelStyle);

                Color pColor = pKill > 0 ? new Color(0.4f, 1f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);
                Color eColor = eKill > 0 ? new Color(1f, 0.45f, 0.25f) : new Color(0.5f, 0.5f, 0.5f);
                GUIStyle pStyle = new GUIStyle(labelStyle) { normal = { textColor = pColor } };
                GUIStyle eStyle = new GUIStyle(labelStyle) { normal = { textColor = eColor } };

                GUI.Label(new Rect(col1, rowY, 110f, rowH), pKill.ToString(), pStyle);
                GUI.Label(new Rect(col2, rowY, 110f, rowH), eKill.ToString(), eStyle);
                rowY += rowH;
            }

            // 합계 구분선
            DrawSolidRect(new Rect(px + 12f, rowY + 2f, panelW - 24f, 1f), new Color(0.55f, 0.50f, 0.28f, 0.5f));
            rowY += 8f;

            GUIStyle totalStyle = new GUIStyle(titleStyle);
            GUI.Label(new Rect(col0, rowY, 160f, rowH), "합계", totalStyle);
            GUI.Label(new Rect(col1, rowY, 110f, rowH), stats.TotalPlayerKills.ToString(), totalStyle);
            GUI.Label(new Rect(col2, rowY, 110f, rowH), stats.TotalEnemyKills.ToString(), totalStyle);

            // 살아있는 유닛 수
            rowY += rowH + 6f;
            int alive = PrototypeRuntimeQuery.CountUnits(UnitTeam.Player);
            int aliveEnemy = PrototypeRuntimeQuery.CountUnits(UnitTeam.Enemy);
            GUI.Label(new Rect(col0, rowY, panelW - 32f, rowH),
                $"현재 생존   아군 {alive}  /  적 {aliveEnemy}", tinyStyle);
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
            float barHeight = 210f;
            float innerH    = barHeight - 24f;
            Rect barRect = new Rect(12f, Screen.height - barHeight - 12f, Screen.width - 24f, barHeight);
            GUI.Box(barRect, GUIContent.none, panelStyle);
            DrawRectOutline(barRect, ColBorder, 2f);

            Rect minimapRect    = new Rect(barRect.x + 12f,            barRect.y + 12f, 210f,  innerH);
            Rect overviewRect   = new Rect(minimapRect.xMax + 10f,     barRect.y + 12f, 310f,  innerH);
            Rect productionRect = new Rect(overviewRect.xMax + 10f,    barRect.y + 12f, 400f,  innerH);
            Rect selectionRect  = new Rect(productionRect.xMax + 10f,  barRect.y + 12f,
                Mathf.Max(220f, barRect.xMax - productionRect.xMax - 22f), innerH);

            DrawMinimap(minimapRect, playerBase, enemyBase, playerProductions, controlNodes, selectionController != null ? selectionController.SelectedUnits : null);
            DrawOverviewPanel(overviewRect, playerBase, enemyBase, playerProductions, controlNodes, playerUnits, enemyUnits, matchController, directiveController);
            DrawProductionPanel(productionRect, playerProductions, directiveController, controlNodes);
            DrawSelectionPanel(selectionRect, selectionController);
        }

        private void DrawMinimap(
            Rect rect,
            BaseStructure playerBase,
            BaseStructure enemyBase,
            List<ProductionStructure> playerProductions,
            List<ControlNode> controlNodes,
            IReadOnlyList<SelectableUnit> selectedUnits)
        {
            DrawPanelFrame(rect, "미니맵");

            Rect mapRect = new Rect(rect.x + 8f, rect.y + 34f, rect.width - 16f, rect.height - 42f);
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

            DrawUrgentMinimapAlerts(mapRect, mapProfile, playerBase, enemyBase, controlNodes);
            DrawCombatAlerts(mapRect, mapProfile);
            DrawControlGroupMarkers(mapRect, mapProfile);
            DrawCommandMarkerOverlay(mapRect, mapProfile, selectedUnits);
            DrawSelectedUnitsOverlay(mapRect, mapProfile, selectedUnits);
            GUI.Label(new Rect(rect.x + 80f, rect.y + 10f, rect.width - 90f, 16f), "클릭 이동  /  드래그", tinyStyle);
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

        private void DrawOverviewPanel(Rect rect, BaseStructure playerBase, BaseStructure enemyBase, List<ProductionStructure> playerProductions, List<ControlNode> controlNodes, int playerUnits, int enemyUnits, PrototypeMatchController matchController, BattleDirectiveController directiveController)
        {
            DrawPanelFrame(rect, "전장 상황");

            float cx = rect.x + 10f;
            float cw = rect.width - 20f;
            float y  = rect.y + 36f;

            // ── 본진 HP 바 ────────────────────────────────────────────────
            float pHp = playerBase != null ? playerBase.HealthNormalized : 0f;
            float eHp = enemyBase  != null ? enemyBase.HealthNormalized  : 0f;

            GUIStyle pLabelStyle = new GUIStyle(tinyStyle) { normal = { textColor = ColPlayer } };
            GUIStyle eLabelStyle = new GUIStyle(tinyStyle) { normal = { textColor = ColEnemy  } };

            GUI.Label(new Rect(cx, y, 36f, 14f), "본진", tinyStyle);
            float barX = cx + 38f;
            float barW = (cw - 38f - 6f) * 0.5f;
            DrawHpBar(new Rect(barX,           y + 2f, barW, 10f), pHp, ColPlayer);
            DrawHpBar(new Rect(barX + barW + 6f, y + 2f, barW, 10f), eHp, ColEnemy);
            GUI.Label(new Rect(barX, y + 13f, barW, 13f), $"아군  {Mathf.RoundToInt(pHp * 100f)}%", pLabelStyle);
            GUI.Label(new Rect(barX + barW + 6f, y + 13f, barW, 13f), $"적  {Mathf.RoundToInt(eHp * 100f)}%", eLabelStyle);
            y += 32f;

            DrawSolidRect(new Rect(cx, y, cw, 1f), ColDivider);
            y += 6f;

            // ── 병력 수 ───────────────────────────────────────────────────
            GUI.Label(new Rect(cx, y, 36f, 18f), "병력", tinyStyle);
            GUIStyle pBigStyle = new GUIStyle(labelStyle) { normal = { textColor = ColPlayer }, fontStyle = FontStyle.Bold };
            GUIStyle eBigStyle = new GUIStyle(labelStyle) { normal = { textColor = ColEnemy  }, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(cx + 40f, y, 70f, 18f), $"아군  {playerUnits}", pBigStyle);
            GUI.Label(new Rect(cx + 120f, y, 60f, 18f), $"적  {enemyUnits}", eBigStyle);
            int delta = playerUnits - enemyUnits;
            string deltaStr = delta > 0 ? $"+{delta}" : delta.ToString();
            Color deltaColor = delta > 0 ? new Color(0.42f, 1f, 0.55f) : delta < 0 ? ColEnemy : ColMuted;
            GUIStyle deltaStyle = new GUIStyle(tinyStyle) { normal = { textColor = deltaColor } };
            GUI.Label(new Rect(cx + 185f, y + 2f, 60f, 14f), deltaStr, deltaStyle);
            y += 22f;

            // ── 거점 수 ───────────────────────────────────────────────────
            GUI.Label(new Rect(cx, y, 36f, 18f), "거점", tinyStyle);
            int total = 0, pNodes = 0, eNodes = 0;
            if (controlNodes != null)
            {
                foreach (ControlNode n in controlNodes)
                {
                    if (n == null) continue;
                    total++;
                    if (n.OwnerTeam == UnitTeam.Player) pNodes++;
                    else if (n.OwnerTeam == UnitTeam.Enemy) eNodes++;
                }
            }
            GUI.Label(new Rect(cx + 40f, y, 80f, 18f), $"아군  {pNodes}/{total}", pBigStyle);
            GUI.Label(new Rect(cx + 130f, y, 70f, 18f), $"적  {eNodes}/{total}", eBigStyle);
            y += 22f;

            DrawSolidRect(new Rect(cx, y, cw, 1f), ColDivider);
            y += 5f;

            // ── 상태 / 압박 ───────────────────────────────────────────────
            string status = PrototypeHudTextUtility.BuildStatus(matchController, playerBase, enemyBase, playerUnits, enemyUnits);
            GUI.Label(new Rect(cx, y, cw, 14f), status, tinyStyle);
            y += 16f;
            GUI.Label(new Rect(cx, y, cw, 14f), PrototypeHudTextUtility.BuildBaseDefenseLine(playerBase), tinyStyle);
        }

        private void DrawProductionPanel(Rect rect, List<ProductionStructure> playerProductions, BattleDirectiveController directiveController, List<ControlNode> controlNodes)
        {
            DrawPanelFrame(rect, "생산");

            float lineY = rect.y + 36f;
            float hintY = rect.yMax - 18f;

            if (playerProductions.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), "활성 생산 건물이 없습니다.", labelStyle);
                return;
            }

            foreach (ProductionStructure structure in playerProductions)
            {
                if (lineY + 36f > hintY - 2f)
                {
                    break;
                }

                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildProductionLine(structure), tinyStyle);
                lineY += 18f;
                GUI.Label(new Rect(rect.x + 8f, lineY, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildProductionDetailLine(structure), tinyStyle);
                lineY += 18f;
            }

            GUI.Label(new Rect(rect.x + 8f, hintY, rect.width - 16f, 18f), PrototypeHudTextUtility.BuildProductionControlHint(directiveController, controlNodes), tinyStyle);
        }

        private void DrawSelectionPanel(Rect rect, PrototypeSelectionController selectionController)
        {
            IReadOnlyList<SelectableUnit> selectedUnits = selectionController != null ? selectionController.SelectedUnits : null;
            DrawPanelFrame(rect, "선택");

            if (selectedUnits == null || selectedUnits.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 10f, rect.y + 38f, rect.width - 20f, 18f), "선택된 부대가 없습니다.", labelStyle);
                GUI.Label(new Rect(rect.x + 10f, rect.y + 58f, rect.width - 20f, 18f), "드래그 또는 클릭으로 부대 선택", tinyStyle);
                DrawSelectionEventBadges(new Rect(rect.x + 10f, rect.y + 82f, rect.width - 20f, 18f), selectionController);
                GUI.Label(new Rect(rect.x + 10f, rect.y + rect.height - 36f, rect.width - 20f, 16f), PrototypeHudTextUtility.BuildControlGroupLine(selectionController), tinyStyle);
                GUI.Label(new Rect(rect.x + 10f, rect.y + rect.height - 18f, rect.width - 20f, 16f), PrototypeHudTextUtility.BuildCommandControlLine(selectionController), tinyStyle);
                return;
            }

            GUI.Label(new Rect(rect.x + 10f, rect.y + 36f, rect.width - 20f, 20f), $"선택 부대  {selectedUnits.Count}", labelStyle);
            DrawSelectionEventBadges(new Rect(rect.x + 110f, rect.y + 36f, rect.width - 120f, 20f), selectionController);
            DrawSelectionIcons(new Rect(rect.x + 8f, rect.y + 58f, rect.width - 16f, 90f), selectedUnits);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 152f, rect.width - 20f, 16f), PrototypeHudTextUtility.BuildSelectionOrdersSummary(selectedUnits), tinyStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + rect.height - 18f, rect.width - 20f, 16f), PrototypeHudTextUtility.BuildCommandControlLine(selectionController), tinyStyle);
        }

        private void DrawSelectionIcons(Rect rect, IReadOnlyList<SelectableUnit> selectedUnits)
        {
            if (selectedUnits == null || selectedUnits.Count == 0) return;

            const float iconSize = 40f;
            const float gap = 4f;
            const float step = iconSize + gap;

            int cols = Mathf.Max(1, Mathf.FloorToInt((rect.width + gap) / step));
            int maxVisible = cols * 2; // 최대 2행

            GUIStyle iconLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.95f) }
            };
            GUIStyle countStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerRight,
                normal = { textColor = new Color(1f, 1f, 0.6f, 1f) }
            };

            int drawn = 0;
            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit == null) continue;
                if (drawn >= maxVisible) break;

                int col = drawn % cols;
                int row = drawn / cols;
                Rect iconRect = new Rect(rect.x + col * step, rect.y + row * step, iconSize, iconSize);

                // 배경: 병종별 색상
                Color bg = GetArchetypeIconColor(unit.Archetype);
                DrawSolidRect(iconRect, bg);
                DrawRectOutline(iconRect, new Color(1f, 1f, 1f, 0.25f), 1f);

                // 체력바 (하단 4px)
                UnitHealth health = unit.GetComponent<UnitHealth>();
                if (health != null)
                {
                    Rect hpBg = new Rect(iconRect.x, iconRect.yMax - 4f, iconSize, 4f);
                    DrawSolidRect(hpBg, new Color(0.1f, 0.1f, 0.1f, 0.8f));
                    Color hpColor = health.Normalized <= 0.35f
                        ? new Color(0.9f, 0.2f, 0.15f)
                        : new Color(0.22f, 0.85f, 0.3f);
                    DrawSolidRect(new Rect(hpBg.x, hpBg.y, hpBg.width * health.Normalized, hpBg.height), hpColor);
                }

                // 선택 하이라이트
                if (unit.IsSelected)
                {
                    DrawRectOutline(iconRect, new Color(0.2f, 0.9f, 1f, 0.9f), 2f);
                }

                // 병종 심볼
                GUI.Label(new Rect(iconRect.x, iconRect.y, iconRect.width, iconRect.height - 4f),
                    GetArchetypeSymbol(unit.Archetype), iconLabelStyle);

                drawn++;
            }

            // 넘치는 유닛 표시
            int overflow = 0;
            foreach (SelectableUnit unit in selectedUnits)
            {
                if (unit != null) overflow++;
            }
            overflow -= drawn;

            if (overflow > 0)
            {
                int lastCol = (drawn - 1) % cols;
                int lastRow = (drawn - 1) / cols;
                // 마지막 자리 다음 칸
                int nextCol = drawn % cols;
                int nextRow = drawn / cols;
                if (nextRow < 2)
                {
                    Rect overRect = new Rect(rect.x + nextCol * step, rect.y + nextRow * step, iconSize, iconSize);
                    DrawSolidRect(overRect, new Color(0.12f, 0.14f, 0.18f, 0.92f));
                    DrawRectOutline(overRect, new Color(0.5f, 0.6f, 0.7f, 0.6f), 1f);
                    GUI.Label(overRect, $"+{overflow}", iconLabelStyle);
                }
            }
        }

        private static Color GetArchetypeIconColor(UnitArchetype archetype)
        {
            return archetype switch
            {
                UnitArchetype.Spearman       => new Color(0.18f, 0.42f, 0.72f, 0.92f),
                UnitArchetype.ShieldInfantry => new Color(0.22f, 0.52f, 0.30f, 0.92f),
                UnitArchetype.Rifleman       => new Color(0.52f, 0.32f, 0.12f, 0.92f),
                UnitArchetype.Artillery      => new Color(0.45f, 0.18f, 0.18f, 0.92f),
                UnitArchetype.SpecialWarrior => new Color(0.38f, 0.18f, 0.52f, 0.92f),
                UnitArchetype.RoyalGuard     => new Color(0.62f, 0.48f, 0.10f, 0.92f),
                UnitArchetype.Fighter        => new Color(0.14f, 0.36f, 0.54f, 0.92f),
                UnitArchetype.MobileFortress => new Color(0.28f, 0.28f, 0.32f, 0.92f),
                UnitArchetype.AirborneCitadel=> new Color(0.20f, 0.28f, 0.44f, 0.92f),
                _                            => new Color(0.20f, 0.22f, 0.26f, 0.92f),
            };
        }

        private static string GetArchetypeSymbol(UnitArchetype archetype)
        {
            return archetype switch
            {
                UnitArchetype.Spearman       => "창",
                UnitArchetype.ShieldInfantry => "방",
                UnitArchetype.Rifleman       => "총",
                UnitArchetype.Artillery      => "포",
                UnitArchetype.SpecialWarrior => "특",
                UnitArchetype.RoyalGuard     => "친",
                UnitArchetype.Fighter        => "기",
                UnitArchetype.MobileFortress => "이",
                UnitArchetype.AirborneCitadel=> "비",
                _                            => "?",
            };
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
                    $"지정 {selectionController.RecentControlGroupAssignmentLabel}",
                    new Color(0.3f, 1f, 0.92f, 0.2f),
                    new Color(0.42f, 1f, 0.92f, 0.88f)));
            }
            else if (selectionController.HasRecentControlGroupRecall)
            {
                eventBadges.Add((
                    $"이동 {selectionController.RecentControlGroupRecallLabel}",
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

        private void DrawTopInfoBar(int playerUnits, int enemyUnits, BaseStructure playerBase, BaseStructure enemyBase)
        {
            float barW = 520f;
            float barH = 28f;
            float barX = (Screen.width - barW) * 0.5f;
            Rect bar   = new Rect(barX, 8f, barW, barH);

            DrawSolidRect(bar, ColPanelBg);
            DrawRectOutline(bar, ColBorder, 1f);

            GUIStyle pStyle = new GUIStyle(labelStyle) { normal = { textColor = ColPlayer }, fontStyle = FontStyle.Bold, fontSize = 13 };
            GUIStyle eStyle = new GUIStyle(labelStyle) { normal = { textColor = ColEnemy  }, fontStyle = FontStyle.Bold, fontSize = 13 };
            GUIStyle cStyle = new GUIStyle(labelStyle) { normal = { textColor = ColTitle  }, fontStyle = FontStyle.Bold, fontSize = 13, alignment = TextAnchor.MiddleCenter };

            float pH = playerBase  != null ? playerBase.HealthNormalized  * 100f : 0f;
            float eH = enemyBase   != null ? enemyBase.HealthNormalized   * 100f : 0f;

            GUI.Label(new Rect(barX + 10f, 9f, 220f, barH), $"아군  {playerUnits}  본진 {Mathf.RoundToInt(pH)}%", pStyle);

            int elapsed = Mathf.FloorToInt(Time.time);
            string timeStr = $"{elapsed / 60:D2}:{elapsed % 60:D2}";
            GUI.Label(new Rect(barX + barW * 0.5f - 30f, 9f, 60f, barH), timeStr, cStyle);

            GUIStyle eRight = new GUIStyle(eStyle) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(barX + barW - 230f, 9f, 220f, barH), $"본진 {Mathf.RoundToInt(eH)}%  적  {enemyUnits}", eRight);
        }

        private void DrawTopRightNews(BattleDirectiveController directiveController)
        {
            if (directiveController == null || !directiveController.HasNews)
            {
                return;
            }

            IReadOnlyList<string> recentNews = directiveController.RecentNews;
            int lineCount = Mathf.Clamp(recentNews.Count, 1, 3);
            const float headerHeight = 24f;
            const float newsLineHeight = 20f;
            float newsHeight = headerHeight + lineCount * newsLineHeight + 10f;
            Rect newsRect = new Rect(Screen.width - 356f, 12f, 344f, newsHeight);
            const float newsPaddingX = 12f;
            const float lineInsetX = 6f;
            GUI.Box(newsRect, GUIContent.none, panelStyle);
            if (directiveController.HasRecentEnemyAssaultStart)
            {
                DrawRectOutline(newsRect, new Color(1f, 0.36f, 0.24f, 0.95f), 2f);
                DrawSolidRect(new Rect(newsRect.x + 4f, newsRect.y + 4f, newsRect.width - 8f, 20f), new Color(0.48f, 0.12f, 0.08f, 0.35f));
            }
            GUI.Label(new Rect(newsRect.x + newsPaddingX + lineInsetX, newsRect.y + 8f, 112f, 18f), "전황 속보", titleStyle);
            for (int index = 0; index < lineCount; index++)
            {
                int newsPriority = directiveController.GetNewsPriority(recentNews[index]);
                string prefix = index == 0
                    ? (directiveController.HasRecentEnemyAssaultStart ? "경보" : "지금")
                    : newsPriority >= 3
                        ? "긴급"
                        : newsPriority >= 2
                            ? "주의"
                            : $"직전 {index}";
                bool isRoutineLine = newsPriority < 2;
                Rect lineRect = new Rect(newsRect.x + newsPaddingX, newsRect.y + headerHeight + 4f + (index * newsLineHeight), newsRect.width - newsPaddingX * 2f, newsLineHeight);
                float prefixWidth = isRoutineLine
                    ? Mathf.Clamp(17f + prefix.Length * 5.9f, 29f, 40f)
                    : Mathf.Clamp(20f + prefix.Length * 7f, 34f, 48f);
                Rect prefixRect = new Rect(lineRect.x + lineInsetX, lineRect.y + (isRoutineLine ? 1.35f : 2f), prefixWidth, lineRect.height - (isRoutineLine ? 2.7f : 4f));
                float bodyInset = isRoutineLine ? 6.5f : 9f;
                float bodyRightPadding = isRoutineLine ? 18f : 21f;
                Rect bodyRect = new Rect(prefixRect.xMax + bodyInset, lineRect.y + (isRoutineLine ? 0.75f : 1f), lineRect.width - prefixWidth - bodyRightPadding, lineRect.height - (isRoutineLine ? 1.5f : 2f));
                if (newsPriority >= 3)
                {
                    DrawSolidRect(lineRect, new Color(0.58f, 0.1f, 0.08f, 0.42f));
                    DrawRectOutline(lineRect, new Color(0.92f, 0.4f, 0.3f, 0.82f), 1f);
                    DrawSolidRect(prefixRect, new Color(0.74f, 0.2f, 0.16f, 0.2f));
                }
                else if (newsPriority >= 2)
                {
                    DrawSolidRect(lineRect, new Color(0.46f, 0.3f, 0.06f, 0.28f));
                    DrawRectOutline(lineRect, new Color(0.9f, 0.78f, 0.4f, 0.76f), 1f);
                    DrawSolidRect(prefixRect, new Color(0.72f, 0.62f, 0.24f, 0.16f));
                }
                else
                {
                    Rect routineLineRect = isRoutineLine
                        ? new Rect(lineRect.x, lineRect.y + 0.5f, lineRect.width, lineRect.height - 1f)
                        : lineRect;
                    Rect routinePrefixRect = isRoutineLine
                        ? new Rect(prefixRect.x, prefixRect.y + 0.25f, prefixRect.width, prefixRect.height - 0.5f)
                        : prefixRect;
                    DrawSolidRect(routineLineRect, new Color(0.18f, 0.18f, 0.2f, 0.08f));
                    DrawSolidRect(routinePrefixRect, new Color(0.3f, 0.32f, 0.36f, 0.09f));
                }

                GUI.Label(prefixRect, prefix, tinyStyle);
                GUI.Label(
                    bodyRect,
                    PrototypeHudTextUtility.Shorten(recentNews[index], isRoutineLine ? 36 : 34),
                    tinyStyle);
            }
        }

        private void DrawMatchOverlay(MatchResult result, BaseStructure playerBase, BaseStructure enemyBase, int playerUnits, int enemyUnits)
        {
            PrototypeMatchController matchController = GetMatchController();
            Rect overlay = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 76f, 420f, 152f);
            GUI.Box(overlay, GUIContent.none, overlayStyle);
            string title = result == MatchResult.Victory ? "승리" : "패배";
            string body = result == MatchResult.Victory
                ? PrototypeHudTextUtility.BuildVictoryBody(matchController, enemyBase, enemyUnits)
                : PrototypeHudTextUtility.BuildDefeatBody(matchController, playerBase, playerUnits);
            string finishRule = matchController != null
                ? matchController.EndReason switch
                {
                    MatchEndReason.EnemyBaseDestroyed => "적 본진을 파괴했습니다.",
                    MatchEndReason.EnemyArmyDestroyed => "적 병력을 전멸시켰습니다.",
                    MatchEndReason.PlayerBaseDestroyed => "아군 본진이 파괴되었습니다.",
                    MatchEndReason.PlayerArmyDestroyed => "아군 병력이 전멸했습니다.",
                    MatchEndReason.MutualAnnihilation => "양측이 동시에 붕괴했습니다.",
                    _ => "전투 종료 조건이 충족되었습니다."
                }
                : "전투 종료 조건이 충족되었습니다.";

            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 24f, 320f, 24f), title, titleStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 56f, 360f, 20f), body, labelStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 84f, 360f, 20f), finishRule, labelStyle);
            GUI.Label(new Rect(overlay.x + 24f, overlay.y + 108f, 360f, 20f), "R 키로 전투를 다시 시작합니다.", labelStyle);
        }

        // ── 팔레트 (StarCraft 기반) ────────────────────────────────────────────
        private static readonly Color ColPanelBg    = new(0.02f, 0.02f, 0.05f, 0.97f);
        private static readonly Color ColOverlayBg  = new(0.01f, 0.01f, 0.04f, 0.98f);
        private static readonly Color ColBorder     = new(0.22f, 0.52f, 0.78f, 0.95f);
        private static readonly Color ColDivider    = new(0.18f, 0.40f, 0.60f, 0.55f);
        private static readonly Color ColTitle      = new(0.72f, 0.88f, 1.00f, 1.00f);
        private static readonly Color ColLabel      = new(0.84f, 0.90f, 0.94f, 1.00f);
        private static readonly Color ColMuted      = new(0.48f, 0.56f, 0.66f, 1.00f);
        private static readonly Color ColPlayer     = new(0.00f, 0.88f, 0.60f, 1.00f);  // SC 테란 그린
        private static readonly Color ColEnemy      = new(0.95f, 0.20f, 0.18f, 1.00f);  // SC 저그 레드
        private static readonly Color ColBarBg      = new(0.08f, 0.10f, 0.14f, 0.92f);

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            whiteTexture          = MakeTexture(Color.white);
            circularMarkerTexture = MakeCircleTexture(32);
            panelTexture          = MakeTexture(ColPanelBg);
            overlayTexture        = MakeTexture(ColOverlayBg);

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTexture;
            panelStyle.border = new RectOffset(6, 6, 6, 6);

            overlayStyle = new GUIStyle(GUI.skin.box);
            overlayStyle.normal.background = overlayTexture;
            overlayStyle.border = new RectOffset(6, 6, 6, 6);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 13;
            labelStyle.normal.textColor = ColLabel;
            labelStyle.clipping = TextClipping.Clip;

            tinyStyle = new GUIStyle(labelStyle);
            tinyStyle.fontSize = 11;
            tinyStyle.normal.textColor = ColMuted;

            titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = ColTitle;

            badgeStyle = new GUIStyle(GUI.skin.box);
            badgeStyle.normal.background = MakeTexture(new Color(0.10f, 0.12f, 0.22f, 0.95f));
            badgeStyle.border = new RectOffset(5, 5, 5, 5);

            eventBadgeTextStyle = new GUIStyle(tinyStyle);
            eventBadgeTextStyle.fontSize = 10;
            eventBadgeTextStyle.fontStyle = FontStyle.Bold;
            eventBadgeTextStyle.alignment = TextAnchor.MiddleCenter;
            eventBadgeTextStyle.normal.textColor = ColLabel;

            minimapGroupLabelStyle = new GUIStyle(tinyStyle);
            minimapGroupLabelStyle.alignment = TextAnchor.MiddleCenter;
            minimapGroupLabelStyle.fontStyle = FontStyle.Bold;
            minimapGroupLabelStyle.normal.textColor = ColPlayer;

            minimapGroupHighlightLabelStyle = new GUIStyle(minimapGroupLabelStyle);
            minimapGroupHighlightLabelStyle.normal.textColor = new Color(0.10f, 0.08f, 0.04f);

            minimapGroupCountStyle = new GUIStyle(tinyStyle);
            minimapGroupCountStyle.fontSize = 9;
            minimapGroupCountStyle.alignment = TextAnchor.MiddleCenter;
            minimapGroupCountStyle.normal.textColor = new Color(0.90f, 0.92f, 1f, 0.94f);

            minimapCommandLabelStyle = new GUIStyle(tinyStyle);
            minimapCommandLabelStyle.fontSize = 10;
            minimapCommandLabelStyle.fontStyle = FontStyle.Bold;
            minimapCommandLabelStyle.alignment = TextAnchor.MiddleCenter;
            minimapCommandLabelStyle.normal.textColor = ColLabel;
        }

        // ── 공통 패널 헬퍼 ────────────────────────────────────────────────────
        private void DrawPanelFrame(Rect rect, string title)
        {
            GUI.Box(rect, GUIContent.none, panelStyle);
            DrawRectOutline(rect, ColBorder, 1f);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 7f, rect.width - 20f, 20f), title, titleStyle);
            DrawSolidRect(new Rect(rect.x + 8f, rect.y + 30f, rect.width - 16f, 1f), ColDivider);
        }

        private void DrawHpBar(Rect rect, float normalized, Color fillColor)
        {
            DrawSolidRect(rect, ColBarBg);
            if (normalized > 0f)
            {
                DrawSolidRect(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(normalized), rect.height), fillColor);
            }
            DrawRectOutline(rect, new Color(fillColor.r, fillColor.g, fillColor.b, 0.35f), 1f);
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

            if (!cameraController.TryGetViewportGroundCorners(cameraViewportWorldCorners))
            {
                return;
            }

            for (int i = 0; i < cameraViewportWorldCorners.Length; i++)
            {
                Vector3 worldPoint = mapProfile.ClampWorldPoint(cameraViewportWorldCorners[i], 2f);
                Vector2 normalized = mapProfile.WorldToNormalized(worldPoint);
                minimapViewportCorners[i] = new Vector2(
                    mapRect.x + normalized.x * mapRect.width,
                    mapRect.y + normalized.y * mapRect.height);
            }

            // 실제 꼭짓점을 잇는 경계선 (AABB 제거 — 사다리꼴 그대로)
            for (int i = 0; i < minimapViewportCorners.Length; i++)
            {
                Vector2 start = minimapViewportCorners[i];
                Vector2 end = minimapViewportCorners[(i + 1) % minimapViewportCorners.Length];
                DrawLine(start, end, new Color(0.92f, 0.98f, 1f, 0.98f), 1.8f);
            }

            // 코너 브라켓도 실제 꼭짓점 기준으로
            DrawViewportCornerBracketsFromCorners(minimapViewportCorners, new Color(1f, 0.94f, 0.52f, 0.98f), 0.2f, 2.1f);

            // 뷰포트 중심 = 4꼭짓점 평균 (AABB 중심 아님)
            Vector2 center = (minimapViewportCorners[0] + minimapViewportCorners[1]
                            + minimapViewportCorners[2] + minimapViewportCorners[3]) * 0.25f;
            Vector2 forwardMidpoint = (minimapViewportCorners[1] + minimapViewportCorners[2]) * 0.5f;
            DrawLine(center, forwardMidpoint, new Color(1f, 0.94f, 0.52f, 0.92f), 1.2f);
            DrawSolidRect(new Rect(center.x - 2f, center.y - 2f, 4f, 4f), new Color(1f, 1f, 1f, 0.96f));
            DrawSolidRect(new Rect(forwardMidpoint.x - 2.5f, forwardMidpoint.y - 2.5f, 5f, 5f), new Color(1f, 0.94f, 0.52f, 0.98f));
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

        private void DrawUrgentMinimapAlerts(Rect mapRect, BattlefieldMapProfile mapProfile, BaseStructure playerBase, BaseStructure enemyBase, List<ControlNode> controlNodes)
        {
            if (mapProfile == null)
            {
                return;
            }

            float pulse = 0.78f + Mathf.PingPong(Time.time * 2.6f, 0.22f);

            if (playerBase != null && playerBase.IsAlive && playerBase.IsDefenseEmergency)
            {
                DrawMinimapAlertMarker(mapRect, mapProfile, playerBase.transform.position, new Color(1f, 0.28f, 0.18f, 0.92f * pulse), 22f, "본진");
            }
            else if (playerBase != null && playerBase.IsAlive && BattleDirectiveController.Instance != null && BattleDirectiveController.Instance.HasRecentEnemyAssaultStart)
            {
                DrawMinimapAlertMarker(mapRect, mapProfile, playerBase.transform.position, new Color(1f, 0.5f, 0.22f, 0.82f * pulse), 18f, "돌입");
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

            if (urgentNode != null)
            {
                DrawMinimapAlertMarker(mapRect, mapProfile, urgentNode.transform.position, new Color(1f, 0.82f, 0.3f, 0.88f * pulse), 18f, "탈환");
                if (playerBase != null && playerBase.IsAlive)
                {
                    DrawMapLine(mapRect, playerBase.transform.position, urgentNode.transform.position, mapProfile, new Color(1f, 0.82f, 0.3f, 0.55f * pulse), 1.35f);
                }
            }

            if (enemyBase != null && enemyBase.IsAlive && BattleDirectiveController.Instance != null && BattleDirectiveController.Instance.IsTotalAssaultActive(UnitTeam.Player))
            {
                DrawMinimapAlertMarker(mapRect, mapProfile, enemyBase.transform.position, new Color(0.34f, 0.92f, 1f, 0.72f * pulse), 16f, "총공");
            }
        }

        // 아군-적 유닛이 80유닛 이내에 있으면 교전 중으로 판단해 미니맵에 깜박이는 마커 표시
        private void DrawCombatAlerts(Rect mapRect, BattlefieldMapProfile mapProfile)
        {
            if (mapProfile == null)
            {
                return;
            }

            const float contactRange = 80f;
            const float clusterMergeRange = 120f;

            combatClusterBuffer.Clear();

            IReadOnlyList<SelectableUnit> allUnits = PrototypeRuntimeRegistry.GetSelectableUnits();
            foreach (SelectableUnit unit in allUnits)
            {
                if (unit == null || unit.Team != UnitTeam.Enemy)
                {
                    continue;
                }

                Vector3 ePos = unit.transform.position;
                bool inContact = false;
                foreach (SelectableUnit other in allUnits)
                {
                    if (other == null || other.Team != UnitTeam.Player)
                    {
                        continue;
                    }

                    if ((other.transform.position - ePos).sqrMagnitude <= contactRange * contactRange)
                    {
                        inContact = true;
                        break;
                    }
                }

                if (!inContact)
                {
                    continue;
                }

                // 기존 클러스터와 합칠지 결정
                bool merged = false;
                for (int i = 0; i < combatClusterBuffer.Count; i++)
                {
                    if ((combatClusterBuffer[i] - ePos).sqrMagnitude <= clusterMergeRange * clusterMergeRange)
                    {
                        combatClusterBuffer[i] = (combatClusterBuffer[i] + ePos) * 0.5f;
                        merged = true;
                        break;
                    }
                }

                if (!merged)
                {
                    combatClusterBuffer.Add(ePos);
                }
            }

            if (combatClusterBuffer.Count == 0)
            {
                return;
            }

            // 빠른 깜박임 (1.8Hz)
            float flash = 0.65f + Mathf.PingPong(Time.time * 3.6f, 0.35f);
            Color markerColor = new Color(1f, 0.88f, 0.22f, 0.92f * flash);

            foreach (Vector3 clusterPos in combatClusterBuffer)
            {
                DrawCombatMarker(mapRect, mapProfile, clusterPos, markerColor);
            }
        }

        private void DrawCombatMarker(Rect mapRect, BattlefieldMapProfile mapProfile, Vector3 worldPosition, Color color)
        {
            Vector2 normalized = mapProfile.WorldToNormalized(worldPosition);
            Vector2 center = new(
                mapRect.x + normalized.x * mapRect.width,
                mapRect.y + normalized.y * mapRect.height);

            float s = 9f;
            // X자 교전 표시
            DrawLine(new Vector2(center.x - s, center.y - s), new Vector2(center.x + s, center.y + s), color, 1.8f);
            DrawLine(new Vector2(center.x + s, center.y - s), new Vector2(center.x - s, center.y + s), color, 1.8f);
            // 중심 점
            DrawSolidRect(new Rect(center.x - 2f, center.y - 2f, 4f, 4f), color);

            // "교전" 라벨
            float labelWidth = 28f;
            Rect labelRect = new Rect(
                Mathf.Clamp(center.x - labelWidth * 0.5f, mapRect.xMin, mapRect.xMax - labelWidth),
                Mathf.Max(mapRect.yMin, center.y - s - 13f),
                labelWidth, 12f);
            Color prev = GUI.color;
            GUI.color = color;
            GUI.Label(labelRect, "교전", minimapCommandLabelStyle);
            GUI.color = prev;
        }

        private void DrawMinimapAlertMarker(Rect mapRect, BattlefieldMapProfile mapProfile, Vector3 worldPosition, Color color, float size, string label)
        {
            Vector2 normalized = mapProfile.WorldToNormalized(worldPosition);
            Vector2 center = new(
                mapRect.x + normalized.x * mapRect.width,
                mapRect.y + normalized.y * mapRect.height);

            Rect outerRect = Rect.MinMaxRect(
                center.x - size * 0.5f,
                center.y - size * 0.5f,
                center.x + size * 0.5f,
                center.y + size * 0.5f);
            Rect innerRect = Rect.MinMaxRect(
                center.x - size * 0.28f,
                center.y - size * 0.28f,
                center.x + size * 0.28f,
                center.y + size * 0.28f);

            DrawRectOutline(outerRect, color, 1.5f);
            DrawRectOutline(innerRect, new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a + 0.12f)), 1f);
            DrawSolidRect(new Rect(center.x - 1.5f, center.y - 1.5f, 3f, 3f), color);

            float labelWidth = Mathf.Max(22f, minimapCommandLabelStyle.CalcSize(new GUIContent(label)).x + 8f);
            Rect labelRect = new Rect(
                Mathf.Clamp(center.x - labelWidth * 0.5f, mapRect.xMin, mapRect.xMax - labelWidth),
                Mathf.Max(mapRect.yMin, center.y - size * 0.5f - 14f),
                labelWidth,
                12f);
            DrawSolidRect(labelRect, new Color(0.09f, 0.07f, 0.05f, 0.78f));
            GUI.Label(labelRect, label, minimapCommandLabelStyle);
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

            cameraController.CenterViewOnWorldPoint(worldPoint);
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

        private void DrawViewportCornerBrackets(Rect rect, float cornerLength, Color color, float thickness)
        {
            Vector2 topLeft = new(rect.xMin, rect.yMin);
            Vector2 topRight = new(rect.xMax, rect.yMin);
            Vector2 bottomRight = new(rect.xMax, rect.yMax);
            Vector2 bottomLeft = new(rect.xMin, rect.yMax);

            DrawLine(topLeft, topLeft + Vector2.right * cornerLength, color, thickness);
            DrawLine(topLeft, topLeft + Vector2.down * cornerLength, color, thickness);

            DrawLine(topRight, topRight + Vector2.left * cornerLength, color, thickness);
            DrawLine(topRight, topRight + Vector2.down * cornerLength, color, thickness);

            DrawLine(bottomRight, bottomRight + Vector2.left * cornerLength, color, thickness);
            DrawLine(bottomRight, bottomRight + Vector2.up * cornerLength, color, thickness);

            DrawLine(bottomLeft, bottomLeft + Vector2.right * cornerLength, color, thickness);
            DrawLine(bottomLeft, bottomLeft + Vector2.up * cornerLength, color, thickness);
        }

        // 실제 뷰포트 꼭짓점(사다리꼴) 기준 코너 브라켓
        private void DrawViewportCornerBracketsFromCorners(Vector2[] corners, Color color, float edgeFraction, float thickness)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 c    = corners[i];
                Vector2 prev = corners[(i + corners.Length - 1) % corners.Length];
                Vector2 next = corners[(i + 1) % corners.Length];

                float lenA = Mathf.Clamp(Vector2.Distance(c, prev) * edgeFraction, 5f, 18f);
                float lenB = Mathf.Clamp(Vector2.Distance(c, next) * edgeFraction, 5f, 18f);

                DrawLine(c, c + (prev - c).normalized * lenA, color, thickness);
                DrawLine(c, c + (next - c).normalized * lenB, color, thickness);
            }
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
            return baseStructure != null ? $"P{baseStructure.CurrentPhase} {baseStructure.PhaseStatusLabel}" : "Lost";
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

