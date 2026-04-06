using System.Collections.Generic;
using Game.CameraSystem;
using Game.Prototype;
using Game.Selection;
using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    public class BattleAcesMinimap : MonoBehaviour
    {
        private const float MapPixelSize = 208f;
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

        private readonly List<MinimapExtraDot> extraDots = new List<MinimapExtraDot>(8);
        private static Texture2D boxSelectTexture;

        private struct MinimapExtraDot
        {
            public Vector3 World;
            public Color Color;
            public float Size;
        }

        public void Bind(
            Vector2 xzWorldMin,
            Vector2 xzWorldMax,
            BattleAcesCore player,
            BattleAcesCore enemy,
            BattleAcesMatchController match = null)
        {
            worldMin = xzWorldMin;
            worldMax = xzWorldMax;
            playerCore = player;
            enemyCore = enemy;
            matchController = match;
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

        private void OnGUI()
        {
            mapRect = new Rect(Screen.width - MapPixelSize - Margin, Screen.height - MapPixelSize - Margin, MapPixelSize, MapPixelSize);

            float w = worldMax.x - worldMin.x;
            float h = worldMax.y - worldMin.y;
            if (w > 0.01f && h > 0.01f)
            {
                HandleMinimapPanAndClick(w, h);
            }

            ImGuiGameUi.DrawFilledRect(mapRect, ImGuiGameUi.PanelBgDeep);
            DrawBorder(mapRect, ImGuiGameUi.BorderCool);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(mapRect.x, mapRect.y - 22f, mapRect.width, 20f), "TACTICAL MAP | Click jump | Drag pan");
            GUI.color = Color.white;

            if (w <= 0.01f || h <= 0.01f)
            {
                return;
            }

            if (playerCore != null && playerCore.Health != null && playerCore.Health.IsAlive)
            {
                DrawWorldDot(playerCore.transform.position, new Color(0.25f, 0.75f, 1f), 9f, w, h);
            }

            if (enemyCore != null && enemyCore.Health != null && enemyCore.Health.IsAlive)
            {
                DrawWorldDot(enemyCore.transform.position, new Color(1f, 0.32f, 0.22f), 9f, w, h);
            }

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

                Color c = unit.Team == UnitTeam.Player
                    ? new Color(0.35f, 0.9f, 0.45f, 0.95f)
                    : new Color(1f, 0.6f, 0.2f, 0.95f);
                DrawWorldDot(unit.transform.position, c, 4.5f, w, h);
            }

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

        private void HandleMinimapPanAndClick(float worldW, float worldH)
        {
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

            Event e = Event.current;
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
                if (e.type == EventType.MouseDown && mapRect.Contains(e.mousePosition))
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

            if (e.type == EventType.MouseDown && mapRect.Contains(e.mousePosition) && (e.button == 0 || e.button == 1))
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
            for (int i = 0; i < 4; i++)
            {
                Ray ray = cam.ScreenPointToRay(scr[i]);
                if (plane.Raycast(ray, out float dist))
                {
                    Vector3 hit = ray.GetPoint(dist);
                    corners[i] = WorldToMapPixels(hit, worldW, worldH);
                }
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
