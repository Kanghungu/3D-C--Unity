using System.Collections.Generic;
using Game.Audio;
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

        /// <summary>클릭 지점 링 피드백 — GUI 픽셀 + 종료 시각</summary>
        private struct MinimapClickRipple
        {
            public Vector2 CenterGui;
            public float EndUnscaled;
        }

        private readonly List<MinimapClickRipple> clickRipples = new List<MinimapClickRipple>(4);

        /// <summary>시야 이동은 매번 — 링·톤만 짧은 unscaled 쿨다운으로 스팸 방지</summary>
        private float lastMinimapClickAudioRippleUnscaled = -999f;

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
            if (kb == null || !kb.mKey.wasPressedThisFrame || !(kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed))
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

            ImGuiGameUi.DrawFilledRect(mapRect, ImGuiGameUi.PanelBgHud);

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

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            // 온보딩: 한 줄에 역할만(자세한 키는 F1)
            GUI.Label(
                new Rect(mapRect.x, mapRect.y - 17f, mapRect.width, 28f),
                DemoPresentationCopy.MinimapCaptionOneLine);
            GUI.color = Color.white;

            if (w <= 0.01f || h <= 0.01f)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            bool colorblindMm = GameUserSettings.ColorblindFriendlyMinimap;

            if (playerCore != null && playerCore.Health != null && playerCore.Health.IsAlive)
            {
                BattleAcesReadability.GetMinimapPlayerCoreDot(
                    playerCore.Health.Normalized,
                    colorblindMm,
                    out Color pc,
                    out float pcSize);
                DrawWorldDot(playerCore.transform.position, pc, pcSize, w, h);
            }

            if (enemyCore != null && enemyCore.Health != null && enemyCore.Health.IsAlive)
            {
                BattleAcesReadability.GetMinimapEnemyCoreDot(
                    enemyCore.Health.Normalized,
                    colorblindMm,
                    out Color ec,
                    out float ecSize);
                DrawWorldDot(enemyCore.transform.position, ec, ecSize, w, h);
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

                BattleAcesReadability.GetMinimapUnitDot(
                    unit.Team == UnitTeam.Player,
                    sel,
                    colorblindMm,
                    out Color c,
                    out float dot);
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

            DrawMinimapClickRipples();

            DrawMinimapLegend(legendRect);

            ImGuiGameUi.EndScaledGui();
        }

        private bool IsLegendCollapsed()
        {
            // 기본 접힘 — 화면 정리(필요 시 칩 클릭으로 펼침)
            return PlayerPrefs.GetInt(LegendCollapsedPrefsKey, 1) == 1;
        }

        private Rect ComputeLegendRect()
        {
            if (IsLegendCollapsed())
            {
                const float chipW = 44f;
                const float chipH = 20f;
                float xChip = Mathf.Max(4f, mapRect.xMin - chipW - 6f);
                float yChip = Mathf.Max(mapRect.yMin + 4f, mapRect.yMax - chipH - 5f);
                return new Rect(xChip, yChip, chipW, chipH);
            }

            int lineCount = CountLegendLines();
            const float pad = 5f;
            const float headerH = 20f;
            const float lineH = 15f;
            float w = Mathf.Min(mapRect.width - 10f, 236f);
            float h = pad * 2f + headerH + lineCount * lineH;
            float x = mapRect.xMin - w - 6f;
            if (x < 4f)
            {
                x = 4f;
            }

            float y = mapRect.yMax - h;
            y = Mathf.Clamp(y, 4f, Mathf.Max(4f, Screen.height - h - 4f));
            return new Rect(x, y, w, h);
        }

        private int CountLegendLines()
        {
            // 코어·유닛·시야·체력 낮음 안내
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
            bool colorblindMm = GameUserSettings.ColorblindFriendlyMinimap;
            bool collapsed = IsLegendCollapsed();
            ImGuiGameUi.DrawFilledRect(legendRect, ImGuiGameUi.PanelBgHud);
            DrawBorder(legendRect, ImGuiGameUi.BorderCool);

            float innerW = legendRect.width;
            float innerH = legendRect.height;
            GUI.BeginClip(legendRect);

            if (collapsed)
            {
                GUI.skin.label.fontSize = 10;
                GUI.color = ImGuiGameUi.TextMuted;
                // 접힘 상태에서도 무엇인지 알 수 있게 짧은 라벨
                GUI.Label(new Rect(4f, 2f, innerW - 4f, innerH - 2f), "표식");
                GUI.color = Color.white;
                GUI.EndClip();
                return;
            }

            GUI.skin.label.fontSize = 10;
            float x = 5f;
            float y = 3f;
            float sw = 8f;
            float textMaxW = Mathf.Max(36f, innerW - x - sw - 6f);

            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x, y, innerW - 44f, 15f), "표식");
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(innerW - 40f, y, 38f, 15f), "접기");

            GUI.color = Color.white;
            y += 16f;

            bool korean = GameUserSettings.Language == GameLanguage.Korean;
            DrawLegendRowClipped(
                x,
                ref y,
                sw,
                textMaxW,
                BattleAcesArtDirection.PointTeal,
                BattleAcesReadability.BuildMinimapLegendCoreLine(korean, colorblindMm));
            DrawLegendRowClipped(
                x,
                ref y,
                sw,
                textMaxW,
                Color.Lerp(BattleAcesArtDirection.PointTeal, BattleAcesArtDirection.GunmetalLift, 0.25f),
                BattleAcesReadability.BuildMinimapLegendUnitLine(korean));
            DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.95f, 0.82f, 0.35f, 1f), "시야 · 클릭/드래그 · Ctrl·Shift");
            DrawLegendRowClipped(
                x,
                ref y,
                sw,
                textMaxW,
                ImGuiGameUi.DefeatTint,
                korean
                    ? "코어 HP 낮음 · 점 커짐·색 경고(아군≤50%·≤28% …)"
                    : "Low core HP · larger dot (ally warn/crit thresholds)");
            if (showRallyLegendLine)
            {
                DrawLegendRowClipped(
                    x,
                    ref y,
                    sw,
                    textMaxW,
                    Color.Lerp(BattleAcesArtDirection.PointTeal, Color.white, 0.12f),
                    "집결(티얼)");
            }

            if (boundObjectiveKind.HasValue)
            {
                switch (boundObjectiveKind.Value)
                {
                    case MissionObjectiveKind.EscortRelic:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.95f, 0.85f, 0.35f, 1f), "성유물·목표구역");
                        break;
                    case MissionObjectiveKind.RecoverRelicAndEvacuate:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.95f, 0.85f, 0.35f, 1f), "성유물·철수");
                        break;
                    case MissionObjectiveKind.SeizeRelicOrNode:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(0.9f, 0.5f, 1f, 1f), "점령 구역");
                        break;
                    case MissionObjectiveKind.DestroyHeresyStronghold:
                        DrawLegendRowClipped(x, ref y, sw, textMaxW, new Color(1f, 0.35f, 0.5f, 1f), "이단 거점");
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

            if (!legendRect.Contains(e.mousePosition))
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
            DrawLineThick(a, b, new Color(0.42f, 0.72f, 0.62f, 0.75f), 2f);
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

            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
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
            bool shiftHeld = Keyboard.current != null &&
                             (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            bool ctrlHeld = Keyboard.current != null &&
                              (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);

            if (shiftMinimapBoxSelectActive && e.type == EventType.MouseDrag)
            {
                minimapBoxEnd = e.mousePosition;
                e.Use();
                return;
            }

            if (shiftHeld && e.button == 0)
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
                    Vector3 worldXZ = new Vector3(wx, 0f, wz);

                    // 좌클릭 + Ctrl: 해당 지점 근처 아군 선택(Shift 박스와 달리 한 점 기준)
                    if (minimapPointerButton == 0 && ctrlHeld)
                    {
                        float pickRadiusWorld = Mathf.Max(worldW, worldH) * 0.045f;
                        PrototypeSelectionController selCtrl = PrototypeSelectionController.Instance;
                        if (selCtrl != null)
                        {
                            selCtrl.SelectPlayerUnitsNearWorldPoint(worldXZ, pickRadiusWorld);
                            // 근처에 아군이 없으면 카메라만 이동(빈 클릭과 동일하게 느껴지게)
                            if (selCtrl.SelectedUnits.Count == 0)
                            {
                                rts.CenterViewOnWorldPoint(worldXZ);
                            }
                        }
                        else
                        {
                            rts.CenterViewOnWorldPoint(worldXZ);
                        }
                    }
                    else
                    {
                        rts.CenterViewOnWorldPoint(worldXZ);
                    }

                    RegisterMinimapClickFeedback(worldXZ, worldW, worldH);
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
            float gMinX = float.PositiveInfinity;
            float gMaxX = float.NegativeInfinity;
            float gMinY = float.PositiveInfinity;
            float gMaxY = float.NegativeInfinity;
            int hitCount = 0;

            for (int i = 0; i < 4; i++)
            {
                Ray ray = cam.ScreenPointToRay(scr[i]);
                if (!plane.Raycast(ray, out float dist))
                {
                    continue;
                }

                Vector3 hit = ray.GetPoint(dist);
                Vector2 gui = WorldToMapPixels(hit, worldW, worldH);
                corners[i] = gui;
                gMinX = Mathf.Min(gMinX, gui.x);
                gMaxX = Mathf.Max(gMaxX, gui.x);
                gMinY = Mathf.Min(gMinY, gui.y);
                gMaxY = Mathf.Max(gMaxY, gui.y);
                hitCount++;
            }

            Color frameColor = new Color(1f, 0.9f, 0.25f, 0.82f);
            Color lineColor = new Color(1f, 0.9f, 0.25f, 0.75f);

            // 화면 네 모서리 → 지면 교차점의 GUI 바운딩 박스(정사각형 강제 제거: 줌 아웃 시 과대·맵 밖으로 튐 방지)
            if (hitCount >= 2 && gMaxX > gMinX + 0.5f && gMaxY > gMinY + 0.5f)
            {
                Rect viewGui = Rect.MinMaxRect(gMinX, gMinY, gMaxX, gMaxY);
                Rect clipped = ClipGuiRectToMinimap(viewGui);
                if (clipped.width > 1f && clipped.height > 1f)
                {
                    DrawBorder(clipped, frameColor);
                }

                return;
            }

            // 꼭짓점 레이 실패 시: 사변선 폴백 — 미니맵 밖으로 선이 새지 않게 클립
            GUI.BeginClip(mapRect);
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    int j = (i + 1) % 4;
                    if (corners[i].HasValue && corners[j].HasValue)
                    {
                        Vector2 a = corners[i].Value - new Vector2(mapRect.x, mapRect.y);
                        Vector2 b = corners[j].Value - new Vector2(mapRect.x, mapRect.y);
                        DrawLineThick(a, b, lineColor, 1.8f);
                    }
                }
            }
            finally
            {
                GUI.EndClip();
            }
        }

        /// <summary>시야 표시가 전술 지도 사각형 밖으로 그려지지 않게 GUI 좌표를 자름.</summary>
        private Rect ClipGuiRectToMinimap(Rect guiRect)
        {
            float xMin = Mathf.Max(guiRect.xMin, mapRect.xMin);
            float yMin = Mathf.Max(guiRect.yMin, mapRect.yMin);
            float xMax = Mathf.Min(guiRect.xMax, mapRect.xMax);
            float yMax = Mathf.Min(guiRect.yMax, mapRect.yMax);
            if (xMax <= xMin + 0.5f || yMax <= yMin + 0.5f)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
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

        private void RegisterMinimapClickFeedback(Vector3 worldXZ, float worldW, float worldH)
        {
            Vector2 center = WorldToMapPixels(worldXZ, worldW, worldH);
            float now = Time.unscaledTime;
            if (now - lastMinimapClickAudioRippleUnscaled >=
                BattleAcesFeedbackTiming.MinimapClickAudioRippleCooldownUnscaled)
            {
                lastMinimapClickAudioRippleUnscaled = now;
                clickRipples.Add(new MinimapClickRipple
                {
                    CenterGui = center,
                    EndUnscaled = now + 0.42f
                });
                ProceduralAudioUtility.PlayUiMinimapPing();
            }

            BattleAcesFirstPlayGuide.NotifyMinimapClick();
        }

        private void DrawMinimapClickRipples()
        {
            float now = Time.unscaledTime;
            const float duration = 0.42f;
            for (int i = clickRipples.Count - 1; i >= 0; i--)
            {
                float remain = clickRipples[i].EndUnscaled - now;
                if (remain <= 0f)
                {
                    clickRipples.RemoveAt(i);
                    continue;
                }

                float t = 1f - remain / duration;
                float radius = 6f + t * 26f;
                float a = (1f - t) * 0.88f;
                DrawRippleRing(clickRipples[i].CenterGui, radius, new Color(0.35f, 0.92f, 1f, a));
            }
        }

        private void DrawRippleRing(Vector2 center, float radiusPx, Color color)
        {
            const int segments = 28;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector2 p0 = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radiusPx;
                Vector2 p1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radiusPx;
                DrawLineThick(p0, p1, color, 2.1f);
            }
        }
    }
}


