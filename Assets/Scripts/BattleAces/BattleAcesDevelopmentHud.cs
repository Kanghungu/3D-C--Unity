// =============================================================================
// [Scripts 위치: Battle Aces]
// - 에디터·Development 빌드 전용 진단 패널 — BattleAcesHudOverlay 와 동일 IMGUI 스케일·팔레트.
// =============================================================================
using Game.Settings;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// 에디터·Development 빌드만 — F10으로 우상단 진단 패널(버전·씬·FPS·그래픽 프리셋).
    /// 릴리스(비개발) Player 빌드에는 생성되지 않음.
    /// </summary>
    public sealed class BattleAcesDevelopmentHud : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool panelVisible = true;
        private float fps;
        private float fpsAccum;
        private int fpsFrames;
        private const float FpsRefreshSeconds = 0.35f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            // AfterSceneLoad 는 씬 전환 시에도 호출될 수 있음 — DDOL HUD 중복 방지
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
            if (kb != null && kb.f10Key.wasPressedThisFrame)
            {
                panelVisible = !panelVisible;
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
            const float panelHeight = 118f;
            Rect r = new Rect(Screen.width - panelWidth - 6f, 6f, panelWidth, panelHeight);
            ImGuiGameUi.DrawHudCardWithLeftStripe(
                r,
                ImGuiGameUi.PanelBgHud,
                ImGuiGameUi.BorderCool,
                ImGuiGameUi.HudStripeTactical,
                3f);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                richText = false,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = ImGuiGameUi.TextTitle }
            };

            string body =
                "Battle Aces 진단 (F10 숨김)\n" +
                "ver " + Application.version +
                " · " + Application.platform +
                "\n씬 " + SceneManager.GetActiveScene().name +
                "\nFPS " + fps.ToString("0") +
                " · timeScale " + Time.timeScale.ToString("0.###") +
                "\n그래픽 " + GameUserSettings.GraphicQualityPreset +
                " · " + Screen.width + "×" + Screen.height +
                (Screen.fullScreen ? " 전체" : " 창");
            GUI.Label(new Rect(r.x + 8f, r.y + 6f, r.width - 16f, r.height - 12f), body, labelStyle);

            GUI.color = Color.white;
            ImGuiGameUi.EndScaledGui();
        }
#else
        // 릴리스(비개발) 빌드: 타입만 유지(부트스트랩·리플렉션 대비 빈 MonoBehaviour)
#endif
    }
}
