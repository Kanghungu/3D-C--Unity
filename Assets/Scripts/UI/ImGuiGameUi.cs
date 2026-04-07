using System.Collections.Generic;
using Game.Settings;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Battle Aces / 캠페인 공용 즉시 모드 GUI(IMGUI) 스타일 — UGUI 전환 전까지 한곳에서 색·패널·버튼 톤 맞춤.
    /// </summary>
    public static class ImGuiGameUi
    {
        private static Texture2D cachedWhite;

        /// <summary>OnGUI DrawTexture용 1×1 화이트(씬마다 중복 생성 방지).</summary>
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

        // --- 팔레트(SF 교단 작전 HUD) ---
        public static readonly Color DimFullscreen = new Color(0.02f, 0.03f, 0.06f, 0.88f);
        public static readonly Color PanelBgDeep = new Color(0.07f, 0.09f, 0.13f, 0.94f);
        public static readonly Color PanelBgLift = new Color(0.1f, 0.12f, 0.16f, 0.96f);
        public static readonly Color BorderCool = new Color(0.38f, 0.48f, 0.58f, 0.9f);
        public static readonly Color BorderAccent = new Color(0.72f, 0.62f, 0.28f, 0.95f);
        public static readonly Color AccentGold = new Color(0.96f, 0.86f, 0.42f, 1f);
        public static readonly Color TextTitle = new Color(0.93f, 0.95f, 0.98f, 1f);
        public static readonly Color TextMuted = new Color(0.65f, 0.72f, 0.8f, 1f);
        public static readonly Color VictoryTint = new Color(0.45f, 0.9f, 0.55f, 1f);
        public static readonly Color DefeatTint = new Color(0.95f, 0.45f, 0.42f, 1f);

        private static readonly Stack<Matrix4x4> GuiMatrixStack = new Stack<Matrix4x4>(4);

        /// <summary>OnGUI 시작부 — PlayerPrefs UI 스케일(전체 IMGUI 공통).</summary>
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

        /// <summary>OnGUI 끝 — BeginScaledGui와 반드시 짝.</summary>
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

        /// <summary>얇은 테두리 프레임(배경 + 4변).</summary>
        public static void DrawPanelFrame(Rect r, Color fill, Color border, float thickness = 2f)
        {
            DrawFilledRect(r, fill);
            float t = Mathf.Max(1f, thickness);
            DrawFilledRect(new Rect(r.x, r.y, r.width, t), border);
            DrawFilledRect(new Rect(r.x, r.yMax - t, r.width, t), border);
            DrawFilledRect(new Rect(r.x, r.y, t, r.height), border);
            DrawFilledRect(new Rect(r.xMax - t, r.y, t, r.height), border);
        }

        /// <summary>상단 강조 줄(목표 바 등).</summary>
        public static void DrawTopAccentBar(float height, Color bg, Color accentLine)
        {
            DrawFilledRect(new Rect(0f, 0f, Screen.width, height), bg);
            DrawFilledRect(new Rect(0f, height - 3f, Screen.width, 3f), accentLine);
        }

        /// <summary>호버·비활성 처리된 메뉴 버튼 — 진짜 게임 메뉴 느낌.</summary>
        public static bool GameMenuButton(Rect r, string text, bool enabled = true)
        {
            Color prevCol = GUI.color;
            Event ev = Event.current;
            bool hover = enabled && ev != null && r.Contains(ev.mousePosition);
            Color bg = !enabled
                ? new Color(0.12f, 0.12f, 0.14f, 0.65f)
                : hover
                    ? new Color(0.16f, 0.2f, 0.28f, 0.98f)
                    : new Color(0.1f, 0.12f, 0.16f, 0.95f);
            Color border = !enabled ? new Color(0.3f, 0.3f, 0.32f, 0.5f) : hover ? AccentGold : BorderCool;

            DrawPanelFrame(r, bg, border, 2f);

            TextAnchor align = GUI.skin.label.alignment;
            int fs = GUI.skin.label.fontSize;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = enabled ? 16 : 14;
            GUI.color = enabled ? TextTitle : TextMuted;
            GUI.Label(r, text);
            GUI.skin.label.fontSize = fs;
            GUI.skin.label.alignment = align;
            GUI.color = prevCol;

            if (!enabled)
            {
                return false;
            }

            // 그린 프레임 위에 투명 버튼으로 클릭 처리
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
    }
}
