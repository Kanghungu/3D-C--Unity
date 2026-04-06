using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 구역 트리거 등에서 띄우는 짧은 내레이션 바(미션 스토리용).
    /// </summary>
    public sealed class BattleAcesStoryBanner : MonoBehaviour
    {
        public static BattleAcesStoryBanner Instance { get; private set; }

        [SerializeField] private float defaultDurationSeconds = 6.5f;

        private string currentLine;
        private float hideAtUnscaled;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>화면 하단에 잠시 표시 — 빈 문자열이면 무시</summary>
        public void ShowLine(string text, float durationSeconds = -1f)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            currentLine = text;
            float dur = durationSeconds > 0f ? durationSeconds : defaultDurationSeconds;
            hideAtUnscaled = Time.unscaledTime + dur;
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(currentLine))
            {
                return;
            }

            if (Time.unscaledTime >= hideAtUnscaled)
            {
                currentLine = null;
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(currentLine))
            {
                return;
            }

            float pad = 18f;
            float boxW = Mathf.Min(920f, Screen.width - pad * 2f);
            float boxH = 72f;
            float x = (Screen.width - boxW) * 0.5f;
            float y = Screen.height - boxH - 28f;
            Rect r = new Rect(x, y, boxW, boxH);

            ImGuiGameUi.DrawPanelFrame(r, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(r.x + 16f, r.y + 10f, r.width - 32f, r.height - 20f), currentLine);
            GUI.color = Color.white;
        }
    }
}
