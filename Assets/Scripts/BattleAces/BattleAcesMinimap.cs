using System.Collections.Generic;
using Game.CameraSystem;
using Game.Campaign.Data;
using Game.Prototype;
using Game.Selection;
using Game.Settings;
using Game.UI;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    public class BattleAcesMinimap : MonoBehaviour
    {
        private const float MapPixelSizeBase = 208f;
        private const float Margin = 14f;

        private Vector2 worldMin;
        private Vector2 worldMax;
        private BattleAcesCore playerCore;
        private BattleAcesCore enemyCore;
        private BattleAcesMatchController matchController;
        private Rect mapRect;

        private bool minimapPointerActive;
        private Vector2 minimapPointerStart;
        private int minimapPointerButton = -1;
        private bool minimapDidDrag;

        private bool shiftMinimapBoxSelectActive;
        private Vector2 minimapBoxStart;
        private Vector2 minimapBoxEnd;

        /// <summary>캠페인 미션일 때만 목표별 범례 한 줄 추가(스커미시는 null).</summary>
        private MissionObjectiveKind? boundObjectiveKind;

        /// <summary>캠페인에서만 미니맵에 집결·추가 표식이 올라가므로 범례에도 표시.</summary>
        private bool showRallyLegendLine;

        private readonly List<MinimapExtraDot> extraDots = new List<MinimapExtraDot>(8);
        private static Texture2D boxSelectTexture;

        private struct MinimapExtraDot
        {
            public Vector3 World;
            public Color Color;
            public float Size;
        }

        private const string LegendCollapsedPrefsKey = "ba_mm_legend_collapsed";

        public void Bind(
            Vector2 xzWorldMin,
            Vector2 xzWorldMax,
            BattleAcesCore player,
            BattleAcesCore enemy,
            BattleAcesMatchController match = null,
            MissionObjectiveKind? objectiveKind = null,
            bool showRallyOnLegend = false)
        {
            worldMin = xzWorldMin;
            worldMax = xzWorldMax;
            playerCore = player;
            enemyCore = enemy;
            matchController = match;
            boundObjectiveKind = objectiveKind;
            showRallyLegendLine = showRallyOnLegend;
        }

        public void ClearExtraMarkers()
        {
            extraDots.Clear();
        }

        public void AddExtraMarker(Vector3 worldPosition, Color color, float sizePixels = 6f)
        {
            extraDots.Add(new MinimapExtraDot
            {
                World = worldPosition,
                Color = color,
                Size = sizePixels
            });
        }

        private static float MapPixelSize => MapPixelSizeBase * GameUserSettings.MinimapScale01;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.mKey.wasPressedThisFrame || !kb.leftShiftKey.isPressed)
            {
                return;
            }

            float cur = GameUserSettings.MinimapScale01;
            float next = cur < 0.92f ? 1f : cur < 1.08f ? 1.22f : 0.85f;
            GameUserSettings.SetMinimapScale(next);
            GameUserSettings.Save();
        }

        private void OnGUI()
        {
            ImGuiGameUi.BeginScaledGui();
            float mapPx = MapPixelSize;
            mapRect = new Rect(Screen.width - mapPx - Margin, Screen.height - mapPx - Margin, mapPx, mapPx);

            float w = worldMax.x - worldMin.x;
            float h = worldMax.y - worldMin.y;
            Rect legendRect = ComputeLegendRect();
            if (w > 0.01f && h > 0.01f)
            {
                HandleMinimapPanAndClick(w, h, legendRect);
            }

            ImGuiGameUi.DrawFilledRect(mapRect, ImGuiGameUi.PanelBgDeep);

            // FoW — 전장과 동일 마스크(유닛 점은 그 위에 그림)
            Texture2D fogMask = BattleAcesFogOfWarDebug.Instance != null
                ? BattleAcesFogOfWarDebug.Instance.FogMaskTexture
                : null;
            // 파괴된 Texture2D는 Unity fake-null — 단순 != null 만으로는 부족할 수 있음
            if (fogMask)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(mapRect, fogMask, ScaleMode.StretchToFill, true);
            }

            DrawBorder(mapRect, ImGuiGameUi.BorderCool);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(mapRect.x, mapRect.y - 22f, mapRect.width, 20f),
                "전술 맵 · 클릭 · 드래그 · Shift 박스 · Shift+M · 우하 색 범례(접기)");
            GUI.color = Color.white;

            if (w <= 0.01f || h <= 0.01f)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            if (playerCore != null && playerCore.Health != null && playerCore.Health.IsAlive)
            {
                float n = playerCore.Health.Normalized;
                Color pc = n <= 0.28f
                    ? new Color(1f, 0.42f, 0.35f, 1f)
                    : new Color(0.25f, 0.75f, 1f, 1f);
                DrawWorldDot(playerCore.transform.position, pc, 9f, w, h);
            }

            if (enemyCore != null && enemyCore.Health != null && enemyCore.Health.IsAlive)
            {
                float en = enemyCore.Health.Normalized;
                bool low = en <= 0.32f;
                Color ec = low
                    ? new Color(1f, 0.52f, 0.12f, 1f)
                    : new Color(1f, 0.32f, 0.22f, 1f);
                DrawWorldDot(enemyCore.transform.position, ec, low ? 10.2f : 9f, w, h);
            }

            SelectableUnit firstSelectedPlayer = null;
            foreach (SelectableUnit unit in PrototypeRuntimeRegistry.GetSelectableUnits())
            {
                if (unit == null)
                {
                    continue;
                }

                CombatTarget ct = unit.GetComponent<CombatTarget>();
                if (ct == null || !ct.IsAlive)
                {
                    continue;
                }

                bool sel = unit.IsSelected;
                if (sel && unit.Team == UnitTeam.Player && firstSelectedPlayer == null)
                {
                    firstSelectedPlayer = unit;
                }

                Color c = unit.Team == UnitTeam.Player
                    ? sel
                        ? new Color(0.55f, 1f, 0.65f, 1f)
                        : new Color(0.35f, 0.9f, 0.45f, 0.95f)
                    : new Color(1f, 0.6f, 0.2f, 0.95f);
                float dot = unit.Team == UnitTeam.Player && sel ? 6.8f : 4.5f;
                DrawWorldDot(unit.transform.position, c, dot, w, h);
            }

            DrawSelectionToCoreLink(firstSelectedPlayer, w, h);

            for (int i = 0; i < extraDots.Count; i++)
            {
                MinimapExtraDot dot = extraDots[i];
                DrawWorldDot(dot.World, dot.Color, dot.Size, w, h);
            }

            DrawCameraOnMap(w, h);

            if (shiftMinimapBoxSelectActive && w > 0.01f && h > 0.01f)
            {
                DrawMinimapBoxSelectOverlay();
            }

            DrawMinimapLegend(legendRect);

            ImGuiGameUi.EndScaledGui();
        }

        private bool IsLegendCollapsed()
        {
            return PlayerPrefs.GetInt(LegendCollapsedPrefsKey, 0) == 1;
        }

        private Rect ComputeLegendRect()
        {
            if (IsLegendCollapsed())
            {
                const float chipW = 56f;
                const float chipH = 22f;
                float yChip = Mathf.Max(mapRect.yMin + 4f, mapRect.yMax - chipH - 5f);
                return new Rect(mapRect.xMin + 5f, yChip, chipW, chipH);
            }

            int lineCount = CountLegendLines();
            const float pad = 5f;
            const float headerH = 20f;
            const float lineH = 15f;
            float w = Mathf.Min(mapRect.width - 10f, 236f);
            float h = pad * 2f + headerH + lineCount * lineH;
            float maxH = Mathf.Max(40f, mapRect.height - 10f);
            h = Mathf.Min(h, maxH);
            float y = mapRect.yMax - h - 5f;
            if (y < mapRect.yMin + 4f)
            {
                y = mapRect.yMin + 4f;
            }

            return new Rect(mapRect.xMin + 5f, y, w, h);
        }

        private int CountLegendLines()
        {
            int n = 4;
            if (showRallyLegendLine)
            {
                n++;
            }

            if (boundObjectiveKind.HasValue)
            {
                switch (boundObjectiveKind.Value)
                {
                    case MissionObjectiveKind.EscortRelic:
                    case MissionObjectiveKind.SeizeRelicOrNode:
                    case MissionObjectiveKind.DestroyHeresyStronghold:
                    case MissionObjectiveKind.RecoverRelicAndEvacuate:
                        n++;
                        break;
                }
            }

            return n;
        }

        private void DrawMinimapLegend(Rect legendRect)
        {
            bool collapsed = IsLegendCollapsed();
            ImGuiGameUi.DrawFilledRect(legendRect, new Color(0.04f, 0.06f, 0.09f, 0.88f));
            DrawBorder(legendRect, new Color(0.35f, 0.45f, 0.55f, 0.75f));

            float innerW = legendRect.width;
            float innerH = legendRect.height;
            GUI.BeginClip(legendRect);

            if (collapsed)
            {
                GUI.skin.label.fontSize = 11;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(0f, 0f, innerW, innerH), "  범례 ▶");
                GUI.color = Color.white;
                GUI.EndClip();
                return;
            }

            GUI.skin.label.fontSize = 11;
            float x = 6f;
            float y = 4f;
            float sw = 9f;
            float textMaxW = Mathf.Max(40f, innerW - x - sw - 8f);

            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y, innerW - 52f, 18f), "색 범례");
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(innerW - 48f, 3f, 44f, 18f), "접기");

            GUI.color = Color.white;
            y += 19f;

            DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(1f, 0.32f, 0.22f, 1f), "적 코어(붉은 큰 점)");
            DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.25f, 0.75f, 1f, 1f), "아군 코어(청록)");
            if (showRallyLegendLine)
            {
                DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.5f, 0.85f, 1f, 1f), "집결 표식(연청)");
            }

            DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.4f, 0.92f, 0.5f, 1f), "아군(초록) · 적(주황 점)");
            DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(1f, 0.92f, 0.25f, 1f), "시야(노란 테·카메라)");

            if (boundObjectiveKind.HasValue)
            {
                switch (boundObjectiveKind.Value)
                {
                    case MissionObjectiveKind.EscortRelic:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.95f, 0.85f, 0.35f, 1f), "성유물(황금) · 목표구역(연녹)");
                        break;
                    case MissionObjectiveKind.RecoverRelicAndEvacuate:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.95f, 0.85f, 0.35f, 1f), "성유물(황금) · 철수구역(연파랑)");
                        break;
                    case MissionObjectiveKind.SeizeRelicOrNode:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.9f, 0.5f, 1f, 1f), "점령 구역(보라)");
                        break;
                    case MissionObjectiveKind.DestroyHeresyStronghold:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(1f, 0.35f, 0.5f, 1f), "이단 본거지(분홍)");
                        break;
                }
            }

            GUI.EndClip();
        }

        /// <summary>BeginClip 안에서 호출 — 좌표는 범례 패널 로컬.</summary>
        private static void DrawLegendRowClipped(
            float rowX,
            ref float y,
            float swatch,
            float textMaxW,
            Color dotColor,
            string text)
        {
            ImGuiGameUi.DrawFilledRect(new Rect(rowX, y + 3f, swatch, swatch), dotColor);
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(rowX + swatch + 6f, y, textMaxW, 16f), text);
            GUI.color = Color.white;
            y += 15f;
        }

        private static Rect GetLegendFoldButtonRect(Rect legendRect)
        {
            return new Rect(legendRect.xMax - 48f, legendRect.yMin + 3f, 44f, 18f);
        }

        private bool TryConsumeLegendMouseDown(Event e, Rect legendRect)
        {
            if (e == null || e.type != EventType.MouseDown || e.button != 0)
            {
                return false;
            }

            if (!mapRect.Contains(e.mousePosition) || !legendRect.Contains(e.mousePosition))
            {
                return false;
            }

            if (IsLegendCollapsed())
            {
                PlayerPrefs.SetInt(LegendCollapsedPrefsKey, 0);
                PlayerPrefs.Save();
                e.Use();
                return true;
            }

            if (GetLegendFoldButtonRect(legendRect).Contains(e.mousePosition))
            {
                PlayerPrefs.SetInt(LegendCollapsedPrefsKey, 1);
                PlayerPrefs.Save();
                e.Use();
                return true;
            }

            e.Use();
            return true;
        }

        /// <summary>선택된 아군 유닛 1기와 아군 코어 사이 링크(전술 유도선)</summary>
        private void DrawSelectionToCoreLink(SelectableUnit selectedPlayerUnit, float worldW, float worldH)
        {
            if (selectedPlayerUnit == null || playerCore == null || playerCore.Health == null || !playerCore.Health.IsAlive)
            {
                return;
            }

            Vector2 a = WorldToMapPixels(selectedPlayerUnit.transform.position, worldW, worldH);
            Vector2 b = WorldToMapPixels(playerCore.transform.position, worldW, worldH);
            DrawLineThick(a, b, new Color(0.32f, 0.95f, 1f, 0.9f), 2.6f);
        }

        private void DrawMinimapBoxSelectOverlay()
        {
            if (boxSelectTexture == null)
            {
                boxSelectTexture = new Texture2D(1, 1);
                boxSelectTexture.SetPixel(0, 0, new Color(0.25f, 0.95f, 0.4f, 0.28f));
                boxSelectTexture.Apply();
            }

            Rect r = GetGuiRectClamped(minimapBoxStart, minimapBoxEnd);
            GUI.DrawTexture(r, boxSelectTexture);
        }

        private static Rect GetGuiRectClamped(Vector2 start, Vector2 end)
        {
            Vector2 min = Vector2.Min(start, end);
            Vector2 max = Vector2.Max(start, end);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private bool TryMapGuiRectToWorldXZ(Rect guiRect, float worldW, float worldH, out float wxMin, out float wxMax, out float wzMin, out float wzMax)
        {
            wxMin = wxMax = wzMin = wzMax = 0f;
            if (worldW <= 0.01f || worldH <= 0.01f || mapRect.width <= 0.01f || mapRect.height <= 0.01f)
            {
                return false;
            }

            float nx0 = Mathf.Clamp01((guiRect.xMin - mapRect.x) / mapRect.width);
            float nx1 = Mathf.Clamp01((guiRect.xMax - mapRect.x) / mapRect.width);
            float ny0 = Mathf.Clamp01((guiRect.yMin - mapRect.y) / mapRect.height);
            float ny1 = Mathf.Clamp01((guiRect.yMax - mapRect.y) / mapRect.height);
            float nz0 = 1f - ny1;
            float nz1 = 1f - ny0;
            wxMin = worldMin.x + Mathf.Min(nx0, nx1) * worldW;
            wxMax = worldMin.x + Mathf.Max(nx0, nx1) * worldW;
            wzMin = worldMin.y + Mathf.Min(nz0, nz1) * worldH;
            wzMax = worldMin.y + Mathf.Max(nz0, nz1) * worldH;
            return wxMax > wxMin && wzMax > wzMin;
        }

        private Rect WorldRectToMapRect(float xMin, float xMax, float zMin, float zMax, float worldW, float worldH)
        {
            Vector2 p0 = WorldToMapPixels(new Vector3(xMin, 0f, zMin), worldW, worldH);
            Vector2 p1 = WorldToMapPixels(new Vector3(xMax, 0f, zMax), worldW, worldH);
            Vector2 min = Vector2.Min(p0, p1);
            Vector2 max = Vector2.Max(p0, p1);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private void HandleMinimapPanAndClick(float worldW, float worldH, Rect legendRect)
        {
            Event e = Event.current;
            if (e != null && TryConsumeLegendMouseDown(e, legendRect))
            {
                return;
            }

            if (matchController != null && matchController.IsFinished)
            {
                return;
            }

            if (CampaignBattleFlow.Instance != null && CampaignBattleFlow.Instance.IsBriefingBlocking)
            {
                return;
            }

            Camera mainCam = Camera.main;
            RTSCameraController rts = mainCam != null ? mainCam.GetComponent<RTSCameraController>() : null;

            if (e == null)
            {
                return;
            }

            const float minBoxDragPixels = 6f;

            if (shiftMinimapBoxSelectActive && e.type == EventType.MouseUp && e.button == 0 && !e.shift)
            {
                shiftMinimapBoxSelectActive = false;
                e.Use();
                return;
            }

            if (shiftMinimapBoxSelectActive && e.type == EventType.MouseDrag)
            {
                minimapBoxEnd = e.mousePosition;
                e.Use();
                return;
            }

            if (e.shift && e.button == 0)
            {
                if (e.type == EventType.MouseDown && mapRect.Contains(e.mousePosition) &&
                    !legendRect.Contains(e.mousePosition))
                {
                    shiftMinimapBoxSelectActive = true;
                    minimapBoxStart = e.mousePosition;
                    minimapBoxEnd = e.mousePosition;
                    e.Use();
                    return;
                }

                if (shiftMinimapBoxSelectActive && e.type == EventType.MouseUp)
                {
                    shiftMinimapBoxSelectActive = false;
                    if (Vector2.Distance(minimapBoxStart, minimapBoxEnd) >= minBoxDragPixels)
                    {
                        Rect sel = GetGuiRectClamped(minimapBoxStart, minimapBoxEnd);
                        Rect overlap = Rect.MinMaxRect(
                            Mathf.Max(sel.xMin, mapRect.xMin),
                            Mathf.Max(sel.yMin, mapRect.yMin),
                            Mathf.Min(sel.xMax, mapRect.xMax),
                            Mathf.Min(sel.yMax, mapRect.yMax));

                        if (overlap.width > 2f && overlap.height > 2f &&
                            TryMapGuiRectToWorldXZ(overlap, worldW, worldH, out float wx0, out float wx1, out float wz0, out float wz1))
                        {
                            PrototypeSelectionController selCtrl = PrototypeSelectionController.Instance;
                            if (selCtrl != null)
                            {
                                selCtrl.SelectPlayerUnitsInWorldXZBounds(wx0, wx1, wz0, wz1);
                            }
                        }
                    }

                    e.Use();
                    return;
                }
            }

            if (e.type == EventType.MouseDown && mapRect.Contains(e.mousePosition) &&
                !legendRect.Contains(e.mousePosition) &&
                (e.button == 0 || e.button == 1))
            {
                minimapPointerActive = true;
                minimapPointerStart = e.mousePosition;
                minimapPointerButton = e.button;
                minimapDidDrag = false;
                e.Use();
                return;
            }

            if (minimapPointerActive && e.type == EventType.MouseDrag && e.button == minimapPointerButton)
            {
                if (rts != null && e.delta.sqrMagnitude > 0.01f)
                {
                    minimapDidDrag = true;
                    float dwx = e.delta.x / mapRect.width * worldW;
                    float dwz = -e.delta.y / mapRect.height * worldH;
                    rts.PanWorldDeltaXZ(new Vector3(dwx, 0f, dwz));
                }

                e.Use();
                return;
            }

            if (minimapPointerActive && e.type == EventType.MouseUp && e.button == minimapPointerButton)
            {
                bool wasInside = mapRect.Contains(e.mousePosition) ||
                                 mapRect.Contains(minimapPointerStart);
                if (wasInside && !minimapDidDrag && rts != null)
                {
                    Vector2 mp = e.mousePosition;
                    float nx = Mathf.Clamp01((mp.x - mapRect.x) / mapRect.width);
                    float ny = Mathf.Clamp01((mp.y - mapRect.y) / mapRect.height);
                    float nz = 1f - ny;
                    float wx = worldMin.x + nx * worldW;
                    float wz = worldMin.y + nz * worldH;
                    rts.CenterViewOnWorldPoint(new Vector3(wx, 0f, wz));
                }

                minimapPointerActive = false;
                minimapPointerButton = -1;
                e.Use();
            }
        }

        private void DrawCameraOnMap(float worldW, float worldH)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Vector3 p = cam.transform.position;
            Vector3 flat = new Vector3(p.x, 0f, p.z);
            DrawWorldDot(flat, new Color(1f, 0.92f, 0.2f, 1f), 7f, worldW, worldH);

            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Vector3[] scr =
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(Screen.width, 0f, 0f),
                new Vector3(Screen.width, Screen.height, 0f),
                new Vector3(0f, Screen.height, 0f)
            };

            Vector2?[] corners = new Vector2?[4];
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            for (int i = 0; i < 4; i++)
            {
                Ray ray = cam.ScreenPointToRay(scr[i]);
                if (plane.Raycast(ray, out float dist))
                {
                    Vector3 hit = ray.GetPoint(dist);
                    corners[i] = WorldToMapPixels(hit, worldW, worldH);
                    minX = Mathf.Min(minX, hit.x);
                    maxX = Mathf.Max(maxX, hit.x);
                    minZ = Mathf.Min(minZ, hit.z);
                    maxZ = Mathf.Max(maxZ, hit.z);
                }
            }

            if (minX < maxX && minZ < maxZ)
            {
                float side = Mathf.Max(maxX - minX, maxZ - minZ);
                float centerX = (minX + maxX) * 0.5f;
                float centerZ = (minZ + maxZ) * 0.5f;
                float half = side * 0.5f;
                Rect cameraSquare = WorldRectToMapRect(centerX - half, centerX + half, centerZ - half, centerZ + half, worldW, worldH);
                DrawBorder(cameraSquare, new Color(1f, 0.9f, 0.25f, 0.82f));
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                if (corners[i].HasValue && corners[j].HasValue)
                {
                    DrawLineThick(corners[i].Value, corners[j].Value, new Color(1f, 0.9f, 0.25f, 0.75f), 1.8f);
                }
            }
        }

        private Vector2 WorldToMapPixels(Vector3 world, float worldW, float worldH)
        {
            float nx = (world.x - worldMin.x) / worldW;
            float nz = (world.z - worldMin.y) / worldH;
            float px = mapRect.x + nx * mapRect.width;
            float py = mapRect.y + (1f - nz) * mapRect.height;
            return new Vector2(px, py);
        }

        private void DrawWorldDot(Vector3 world, Color color, float sizePx, float worldW, float worldH)
        {
            Vector2 p = WorldToMapPixels(world, worldW, worldH);
            DrawFilledRect(new Rect(p.x - sizePx * 0.5f, p.y - sizePx * 0.5f, sizePx, sizePx), color);
        }

        private static void DrawBorder(Rect r, Color c)
        {
            const float t = 2f;
            DrawFilledRect(new Rect(r.x, r.y, r.width, t), c);
            DrawFilledRect(new Rect(r.x, r.yMax - t, r.width, t), c);
            DrawFilledRect(new Rect(r.x, r.y, t, r.height), c);
            DrawFilledRect(new Rect(r.xMax - t, r.y, t, r.height), c);
        }

        private void DrawLineThick(Vector2 a, Vector2 b, Color c, float thickness)
        {
            float len = Vector2.Distance(a, b);
            if (len < 0.5f)
            {
                return;
            }

            int steps = Mathf.Clamp(Mathf.CeilToInt(len / 3f), 4, 120);
            float half = thickness * 0.5f;
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2 p = Vector2.Lerp(a, b, t);
                DrawFilledRect(new Rect(p.x - half, p.y - half, thickness, thickness), c);
            }
        }

        private static void DrawFilledRect(Rect r, Color c)
        {
            ImGuiGameUi.DrawFilledRect(r, c);
        }
    }
}

