using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    /// <summary>
    /// P 일시정지, Q/W/E 게임 속도. 승패/브리핑 중에는 적용하지 않는다.
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

        /// <summary>HUD 한 줄 — 일시정지·배속 표시</summary>
        public string GetStatusLineKo()
        {
            if (!allowControl)
            {
                return string.Empty;
            }

            if (paused)
            {
                return "시간: 일시정지 (P) · Q·W·E 배속";
            }

            float s = CurrentSpeedStep;
            return $"시간: 배속 {s:0.##}× (P 일시정지 · Q·W·E 단계)";
        }

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

            if (kb.qKey.wasPressedThisFrame)
            {
                speedIndex = 0;
                paused = false;
                ApplyTimeScale();
            }
            else if (kb.wKey.wasPressedThisFrame)
            {
                speedIndex = 1;
                paused = false;
                ApplyTimeScale();
            }
            else if (kb.eKey.wasPressedThisFrame)
            {
                speedIndex = 2;
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
