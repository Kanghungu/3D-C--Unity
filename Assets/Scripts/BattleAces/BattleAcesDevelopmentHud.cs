// =============================================================================
// [Scripts Location: Battle Aces]
// - Development-build diagnostic HUD that shares the same IMGUI styling as
//   BattleAcesHudOverlay and only appears in editor/development players.
// =============================================================================
using Game.CameraSystem;
using Game.Settings;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// Development-only runtime diagnostics for Battle Aces.
    /// Toggle with F10, with quick capture helpers on F8/F9/F12.
    /// </summary>
    public sealed class BattleAcesDevelopmentHud : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool panelVisible = true;
        private float fps;
        private float fpsAccum;
        private int fpsFrames;
        private const float FpsRefreshSeconds = 0.35f;

        private static GUIStyle cachedLabelStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            // Prevent duplicate DDOL HUD instances when scenes reload in the editor.
            if (Object.FindAnyObjectByType<BattleAcesDevelopmentHud>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            GameObject go = new GameObject("BattleAcesDevelopmentHud");
            DontDestroyOnLoad(go);
            go.AddComponent<BattleAcesDevelopmentHud>();
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.f12Key.wasPressedThisFrame)
            {
                BattleAcesHudCaptureMode.ToggleSuppressCombatChromeForScreenshot();
            }

            if (kb != null && kb.f10Key.wasPressedThisFrame)
            {
                panelVisible = !panelVisible;
            }

            // Store capture camera presets are only useful when an RTS camera exists.
            if (kb != null && kb.f8Key.wasPressedThisFrame)
            {
                BattleAcesStoreCapturePresets.TryApply(BattleAcesStoreCapturePresets.PresetKind.BattlefieldOverview);
            }

            if (kb != null && kb.f9Key.wasPressedThisFrame)
            {
                BattleAcesStoreCapturePresets.TryApply(BattleAcesStoreCapturePresets.PresetKind.PlayerCoreClose);
            }

            fpsAccum += Time.unscaledDeltaTime;
            fpsFrames++;
            if (fpsAccum >= FpsRefreshSeconds)
            {
                fps = fpsFrames / fpsAccum;
                fpsAccum = 0f;
                fpsFrames = 0;
            }
        }

        private void OnGUI()
        {
            if (!panelVisible)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();
            GUI.depth = -2000;

            const float panelWidth = 280f;
            const float panelHeight = 152f;
            Rect r = new Rect(Screen.width - panelWidth - 6f, 6f, panelWidth, panelHeight);
            ImGuiGameUi.DrawHudCardWithLeftStripe(
                r,
                ImGuiGameUi.PanelBgHud,
                ImGuiGameUi.BorderCool,
                ImGuiGameUi.HudStripeTactical,
                3f);

            if (cachedLabelStyle == null)
            {
                cachedLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    richText = false,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft,
                    normal = { textColor = ImGuiGameUi.TextTitle }
                };
            }

            string body =
                "Battle Aces 진단 (F10 토글)\n" +
                "스토어 캡처 카메라: F8 전장 / F9 코어 근접\n" +
                "FoW 격자 미리보기: F11 (개발 빌드 전용)\n" +
                "스크린샷 HUD 억제: F12 (" +
                (BattleAcesHudCaptureMode.SuppressCombatChromeForScreenshot ? "ON" : "OFF") +
                ")\n" +
                "ver " + Application.version +
                " · " + Application.platform +
                "\n씬 " + SceneManager.GetActiveScene().name +
                "\nFPS " + fps.ToString("0") +
                " · timeScale " + Time.timeScale.ToString("0.###") +
                "\n그래픽 " + GameUserSettings.GraphicQualityPreset +
                " · " + Screen.width + "x" + Screen.height +
                (Screen.fullScreen ? " 전체화면" : " 창모드");
            GUI.Label(new Rect(r.x + 8f, r.y + 6f, r.width - 16f, r.height - 12f), body, cachedLabelStyle);

            GUI.color = Color.white;
            ImGuiGameUi.EndScaledGui();
        }
#else
        // Keep the component shape consistent in release players without showing the panel.
#endif
    }
}
