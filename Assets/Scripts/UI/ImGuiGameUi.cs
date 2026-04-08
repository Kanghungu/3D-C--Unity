using System.Collections.Generic;
using Game.Settings;
using UnityEngine;

namespace Game.UI
{
    public static class ImGuiGameUi
    {
        private static Texture2D cachedWhite;

        public static Texture2D WhitePixel
        {
            get
            {
                if (cachedWhite == null)
                {
                    cachedWhite = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    cachedWhite.hideFlags = HideFlags.HideAndDontSave;
                    cachedWhite.SetPixel(0, 0, Color.white);
                    cachedWhite.Apply(false, true);
                }

                return cachedWhite;
            }
        }

        public static readonly Color DimFullscreen = new Color(0.012f, 0.014f, 0.022f, 0.88f);
        public static readonly Color PanelBgDeep = new Color(0.038f, 0.042f, 0.052f, 0.96f);
        public static readonly Color PanelBgLift = new Color(0.065f, 0.07f, 0.082f, 0.96f);
        public static readonly Color PanelBgHud = new Color(0.022f, 0.025f, 0.034f, 0.93f);
        public static readonly Color PanelBgHudCard = new Color(0.05f, 0.054f, 0.064f, 0.9f);
        public static readonly Color BorderCool = new Color(0.2f, 0.22f, 0.28f, 0.72f);
        public static readonly Color BorderAccent = new Color(0.72f, 0.58f, 0.28f, 0.88f);
        public static readonly Color AccentGold = new Color(0.9f, 0.74f, 0.4f, 1f);
        public static readonly Color AccentCyan = new Color(0.48f, 0.78f, 0.82f, 1f);
        public static readonly Color HudStripeTactical = new Color(0.4f, 0.58f, 0.52f, 0.9f);
        public static readonly Color TextTitle = new Color(0.9f, 0.92f, 0.94f, 1f);
        public static readonly Color TextMuted = new Color(0.52f, 0.58f, 0.66f, 1f);
        public static readonly Color ResourceHighlight = new Color(0.98f, 0.92f, 0.78f, 1f);
        public static readonly Color VictoryTint = new Color(0.45f, 0.9f, 0.55f, 1f);
        public static readonly Color DefeatTint = new Color(0.95f, 0.45f, 0.42f, 1f);

        private static readonly Stack<Matrix4x4> GuiMatrixStack = new Stack<Matrix4x4>(4);

        public static void BeginScaledGui()
        {
            GuiMatrixStack.Push(GUI.matrix);
            float s = Mathf.Clamp(GameUserSettings.UiScale01, 0.75f, 1.35f);
            if (Mathf.Approximately(s, 1f))
            {
                return;
            }

            GUIUtility.ScaleAroundPivot(new Vector2(s, s), new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        }

        public static void EndScaledGui()
        {
            if (GuiMatrixStack.Count == 0)
            {
                return;
            }

            GUI.matrix = GuiMatrixStack.Pop();
        }

        public static void DrawFilledRect(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, WhitePixel, ScaleMode.StretchToFill);
            GUI.color = prev;
        }

        public static void DrawVerticalGradient(Rect r, Color top, Color bottom, int slices = 24)
        {
            int count = Mathf.Max(1, slices);
            float h = r.height / count;
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                DrawFilledRect(new Rect(r.x, r.y + h * i, r.width, h + 1f), Color.Lerp(top, bottom, t));
            }
        }

        public static void DrawScanLines(Rect r, Color c, float gap = 24f, float thickness = 1f)
        {
            for (float y = r.y; y < r.yMax; y += Mathf.Max(6f, gap))
            {
                DrawFilledRect(new Rect(r.x, y, r.width, thickness), c);
            }
        }

        public static void DrawGrid(Rect r, Color c, float cellW = 64f, float cellH = 64f, float thickness = 1f)
        {
            for (float x = r.x; x <= r.xMax; x += Mathf.Max(12f, cellW))
            {
                DrawFilledRect(new Rect(x, r.y, thickness, r.height), c);
            }

            for (float y = r.y; y <= r.yMax; y += Mathf.Max(12f, cellH))
            {
                DrawFilledRect(new Rect(r.x, y, r.width, thickness), c);
            }
        }

        public static void DrawSoftShadow(Rect r, Color c, float spread = 18f)
        {
            float s = Mathf.Max(4f, spread);
            DrawFilledRect(new Rect(r.x + 8f, r.y + 10f, r.width, r.height), new Color(c.r, c.g, c.b, c.a * 0.28f));
            DrawFilledRect(new Rect(r.x + 4f, r.y + 4f, r.width + s * 0.35f, r.height + s * 0.35f), new Color(c.r, c.g, c.b, c.a * 0.12f));
            DrawFilledRect(new Rect(r.x - 2f, r.y - 2f, r.width + s * 0.7f, r.height + s * 0.7f), new Color(c.r, c.g, c.b, c.a * 0.05f));
        }

        public static void DrawSweepLine(Rect r, Color c, float normalizedX, float width = 46f)
        {
            float w = Mathf.Clamp(width, 8f, r.width * 0.35f);
            float x = Mathf.Lerp(r.x - w, r.xMax, Mathf.Clamp01(normalizedX));
            DrawFilledRect(new Rect(x, r.y, w, 2f), c);
        }

        public static void DrawPanelFrame(Rect r, Color fill, Color border, float thickness = 2f)
        {
            DrawFilledRect(r, fill);
            float t = Mathf.Max(1f, thickness);
            DrawFilledRect(new Rect(r.x, r.y, r.width, t), border);
            DrawFilledRect(new Rect(r.x, r.yMax - t, r.width, t), border);
            DrawFilledRect(new Rect(r.x, r.y, t, r.height), border);
            DrawFilledRect(new Rect(r.xMax - t, r.y, t, r.height), border);
        }

        public static void DrawTopAccentBar(float height, Color bg, Color accentLine)
        {
            DrawFilledRect(new Rect(0f, 0f, Screen.width, height), bg);
            DrawFilledRect(new Rect(0f, height - 3f, Screen.width, 3f), accentLine);
        }

        public static void DrawHudCardWithLeftStripe(Rect r, Color fill, Color border, Color stripe, float stripeW = 3f)
        {
            DrawPanelFrame(r, fill, border, 1f);
            float sw = Mathf.Clamp(stripeW, 2f, 8f);
            DrawFilledRect(new Rect(r.x, r.y, sw, r.height), stripe);
        }

        public static void DrawCornerBrackets(Rect r, Color c, float size = 14f, float thickness = 2f)
        {
            float s = Mathf.Clamp(size, 8f, 32f);
            float t = Mathf.Clamp(thickness, 1f, 4f);

            DrawFilledRect(new Rect(r.x, r.y, s, t), c);
            DrawFilledRect(new Rect(r.x, r.y, t, s), c);
            DrawFilledRect(new Rect(r.xMax - s, r.y, s, t), c);
            DrawFilledRect(new Rect(r.xMax - t, r.y, t, s), c);
            DrawFilledRect(new Rect(r.x, r.yMax - t, s, t), c);
            DrawFilledRect(new Rect(r.x, r.yMax - s, t, s), c);
            DrawFilledRect(new Rect(r.xMax - s, r.yMax - t, s, t), c);
            DrawFilledRect(new Rect(r.xMax - t, r.yMax - s, t, s), c);
        }

        public static void DrawGlassPanel(Rect r, Color fill, Color border, Color accent)
        {
            DrawSoftShadow(r, new Color(0f, 0f, 0f, 0.38f), 22f);
            DrawVerticalGradient(r, fill * 1.08f, fill * 0.82f, 12);
            DrawFilledRect(
                new Rect(r.x, r.y, r.width, Mathf.Min(28f, r.height * 0.22f)),
                new Color(accent.r, accent.g, accent.b, 0.08f));
            DrawPanelFrame(r, new Color(0f, 0f, 0f, 0f), border, 1.5f);
            DrawCornerBrackets(r, accent, 14f, 2f);
        }

        public static void DrawHorizontalRule(Rect rowRect, Color c, float thickness = 1f)
        {
            float t = Mathf.Max(0.5f, thickness);
            DrawFilledRect(new Rect(rowRect.x, rowRect.y, rowRect.width, t), c);
        }

        public static bool GameMenuButton(Rect r, string text, bool enabled = true)
        {
            Color prevCol = GUI.color;
            Event ev = Event.current;
            bool hover = enabled && ev != null && r.Contains(ev.mousePosition);
            Color bg = !enabled
                ? new Color(0.12f, 0.12f, 0.14f, 0.65f)
                : hover
                    ? new Color(0.14f, 0.19f, 0.28f, 0.98f)
                    : new Color(0.075f, 0.09f, 0.125f, 0.96f);
            Color border = !enabled ? new Color(0.3f, 0.3f, 0.32f, 0.5f) : hover ? AccentGold : BorderCool;
            Color accent = hover ? AccentGold : AccentCyan;

            DrawGlassPanel(r, bg, border, accent);
            DrawFilledRect(
                new Rect(r.x + 14f, r.y + 10f, Mathf.Max(42f, r.width * 0.18f), 2f),
                new Color(accent.r, accent.g, accent.b, hover ? 0.9f : 0.45f));
            DrawFilledRect(
                new Rect(r.x + 14f, r.y + 11f, r.width - 28f, 10f),
                new Color(accent.r, accent.g, accent.b, hover ? 0.08f : 0.035f));

            TextAnchor align = GUI.skin.label.alignment;
            int fs = GUI.skin.label.fontSize;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            bool korean = GameUserSettings.Language == GameLanguage.Korean;
            GUI.skin.label.fontSize = enabled
                ? (korean ? 13 : 15)
                : (korean ? 12 : 14);
            GUI.color = enabled ? TextTitle : TextMuted;
            GUI.Label(r, text);
            GUI.skin.label.fontSize = fs;
            GUI.skin.label.alignment = align;
            GUI.color = prevCol;

            if (!enabled)
            {
                return false;
            }

            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
    }
}
