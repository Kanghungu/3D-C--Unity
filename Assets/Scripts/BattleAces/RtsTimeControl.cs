using Game.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// P 일시정지, [ / ] (및 숫자패드 - / +) 로 배속 단계. (카메라 Q/E 회전은 RTS 데모에서 제거됨)
    /// </summary>
    public sealed class RtsTimeControl : MonoBehaviour
    {
        public static RtsTimeControl Instance { get; private set; }

        [SerializeField] private float[] speedSteps = { 1f, 1.5f, 2f };

        private int speedIndex;
        private bool paused;
        private bool allowControl = true;

        public bool IsPaused => paused;
        public float CurrentSpeedStep => speedSteps != null && speedSteps.Length > 0
            ? speedSteps[Mathf.Clamp(speedIndex, 0, speedSteps.Length - 1)]
            : 1f;

        /// <summary>좌측 패널 하단 한 줄 — 일시정지·배속(한·영, 중점 구분 통일)</summary>
        public string GetHudTimeStatusLine()
        {
            if (!allowControl)
            {
                return string.Empty;
            }

            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            if (paused)
            {
                return ko
                    ? "시간 · 일시정지 (P) · [ ] 배속 · 숫자패드 - +"
                    : "Time · paused (P) · [ ] speed · numpad - +";
            }

            float s = CurrentSpeedStep;
            return ko
                ? $"시간 · 배속 {s:0.##}× (P 일시정지 · [ 느리게 · ] 빠르게)"
                : $"Time · speed {s:0.##}× (P pause · [ slower · ] faster)";
        }

        /// <summary>호환용 — <see cref="GetHudTimeStatusLine"/> 와 동일</summary>
        public string GetStatusLineKo() => GetHudTimeStatusLine();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!allowControl)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (kb.pKey.wasPressedThisFrame)
            {
                paused = !paused;
                ApplyTimeScale();
            }

            // [ ] 및 숫자패드 ± — 배속 전용
            bool slower = kb[Key.LeftBracket].wasPressedThisFrame || kb[Key.NumpadMinus].wasPressedThisFrame;
            bool faster = kb[Key.RightBracket].wasPressedThisFrame || kb[Key.NumpadPlus].wasPressedThisFrame;
            if (slower || faster)
            {
                int max = speedSteps != null && speedSteps.Length > 0 ? speedSteps.Length - 1 : 0;
                if (slower && !faster)
                {
                    speedIndex = Mathf.Max(0, speedIndex - 1);
                }
                else if (faster && !slower)
                {
                    speedIndex = Mathf.Min(max, speedIndex + 1);
                }

                paused = false;
                ApplyTimeScale();
            }
        }

        public void SetControlEnabled(bool enabled)
        {
            allowControl = enabled;
            if (!enabled)
            {
                return;
            }

            ApplyTimeScale();
        }

        public void ForcePauseGameplay()
        {
            paused = true;
            Time.timeScale = 0f;
        }

        public void ResumeFromPause(float scale)
        {
            paused = false;
            Time.timeScale = Mathf.Max(0.01f, scale);
        }

        private void ApplyTimeScale()
        {
            if (paused)
            {
                Time.timeScale = 0f;
                return;
            }

            float s = CurrentSpeedStep;
            Time.timeScale = Mathf.Max(0.05f, s);
        }
    }
}
