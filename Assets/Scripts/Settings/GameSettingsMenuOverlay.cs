using Game.BattleAces;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Settings
{
    /// <summary>
    /// Play 중 O 키로 여는 설정 패널 — 마스터 / 전투 전용 / 화면 섹션으로 구분.
    /// </summary>
    public sealed class GameSettingsMenuOverlay : MonoBehaviour
    {
        private const float PanelWidth = 468f;
        private const float PanelHeightPreferred = 620f;
        private const float PanelLeft = 24f;
        private const float PanelBottomMargin = 16f;
        private const float ContentHeightApprox = 820f;

        private bool panelOpen;

        /// <summary>스크롤뷰 위치 유지(짧은 화면 대비)</summary>
        private Vector2 settingsScrollPos;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.oKey.wasPressedThisFrame)
            {
                panelOpen = !panelOpen;
            }
        }

        private void OnGUI()
        {
            if (!panelOpen)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();

            float maxPanelH = Mathf.Max(200f, Screen.height - PanelBottomMargin - 8f);
            float panelH = Mathf.Min(PanelHeightPreferred, maxPanelH);
            float panelY = Mathf.Max(8f, Screen.height - panelH - PanelBottomMargin);
            Rect panelRect = new Rect(PanelLeft, panelY, PanelWidth, panelH);

            ImGuiGameUi.DrawPanelFrame(panelRect, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);

            int prevFont = GUI.skin.label.fontSize;
            Color prevGuiColor = GUI.color;

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(panelRect.x + 14f, panelRect.y + 10f, panelRect.width - 28f, 26f), "설정 (O로 닫기)");

            const float headerH = 38f;
            Rect scrollOuter = new Rect(
                panelRect.x + 8f,
                panelRect.y + headerH,
                panelRect.width - 16f,
                Mathf.Max(80f, panelH - headerH - 10f));

            float innerPadX = 8f;
            float innerW = Mathf.Max(40f, scrollOuter.width - 18f);
            float sliderLeft = innerPadX + 168f;
            float sliderW = Mathf.Clamp(innerW - 176f, 32f, Mathf.Max(32f, innerW - innerPadX - 4f));

            settingsScrollPos = GUI.BeginScrollView(
                scrollOuter,
                settingsScrollPos,
                new Rect(0f, 0f, innerW, ContentHeightApprox));

            try
            {
                float y = 4f;

                // --- 마스터 ---
                DrawSectionTitle(ref y, innerPadX, innerW, "마스터");
                DrawSectionHint(ref y, innerPadX, innerW, "게임 전체에 적용되는 출력 음량입니다.");

                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 160f, 22f), "마스터 볼륨");
                float vol = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.MasterVolume01,
                    0f,
                    1f);
                if (!Mathf.Approximately(vol, GameUserSettings.MasterVolume01))
                {
                    GameUserSettings.SetMasterVolume01(vol);
                }

                y += 38f;

                // --- 전투 전용 ---
                DrawSectionTitle(ref y, innerPadX, innerW, "전투 전용");
                DrawSectionHint(ref y, innerPadX, innerW, "전투 씬 믹서 그룹만 조절합니다(BGM·승패 스팅).");

                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 170f, 22f), "전투 앰비언트");
                float amb = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.BattleAmbientVolume01,
                    0f,
                    1f);
                if (!Mathf.Approximately(amb, GameUserSettings.BattleAmbientVolume01))
                {
                    GameUserSettings.SetBattleAmbientVolume01(amb);
                }

                y += 36f;
                GUI.Label(new Rect(innerPadX, y, 170f, 22f), "승·패 스팅");
                float st = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.ResultStingVolume01,
                    0f,
                    1f);
                if (!Mathf.Approximately(st, GameUserSettings.ResultStingVolume01))
                {
                    GameUserSettings.SetResultStingVolume01(st);
                }

                y += 38f;

                // --- 전투 안개 (선택) ---
                DrawSectionTitle(ref y, innerPadX, innerW, "전투 안개 (선택)");
                DrawSectionHint(ref y, innerPadX, innerW, "ClassicDuel 평지 데모 전장만. 스샷·유닛 가독성용 — 저장 후 유지됩니다.");

                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 168f, 22f), "안개 거리");
                float fogDist = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.BattleFogDistanceScale,
                    0.62f,
                    1.42f);
                if (!Mathf.Approximately(fogDist, GameUserSettings.BattleFogDistanceScale))
                {
                    GameUserSettings.SetBattleFogDistanceScale(fogDist);
                    BattleAcesWorldPresentation.RefreshBattleFogIfClassicDuelActive();
                }

                y += 36f;
                GUI.Label(new Rect(innerPadX, y, 168f, 22f), "안개 강도");
                float fogInt = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.BattleFogIntensity01,
                    0f,
                    1f);
                if (!Mathf.Approximately(fogInt, GameUserSettings.BattleFogIntensity01))
                {
                    GameUserSettings.SetBattleFogIntensity01(fogInt);
                    BattleAcesWorldPresentation.RefreshBattleFogIfClassicDuelActive();
                }

                y += 40f;

                // --- 화면 ---
                DrawSectionTitle(ref y, innerPadX, innerW, "화면");
                DrawSectionHint(ref y, innerPadX, innerW, "카메라 감도·창 모드·HUD(IMGUI) 표시 크기입니다.");

                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 160f, 22f), "카메라 감도");
                float sens = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.CameraSensitivityMultiplier,
                    0.35f,
                    2.5f);
                if (!Mathf.Approximately(sens, GameUserSettings.CameraSensitivityMultiplier))
                {
                    GameUserSettings.SetCameraSensitivity(sens);
                }

                y += 36f;
                bool fs = Screen.fullScreen;
                bool newFs = GUI.Toggle(new Rect(innerPadX, y, innerW, 22f), fs, "전체 화면");
                if (newFs != fs)
                {
                    GameUserSettings.SetFullscreen(newFs);
                }

                y += 32f;

                // --- 그래픽 (데모 2단) ---
                DrawSectionTitle(ref y, innerPadX, innerW, "그래픽");
                DrawSectionHint(ref y, innerPadX, innerW, "저사양: 그림자 끔·거리 단축. 균형: 기본값에 가깝게 복구.");

                bool perf = GameUserSettings.GraphicQualityPreset == DemoGraphicQualityPreset.Performance;
                bool perfNew = GUI.Toggle(new Rect(innerPadX, y, innerW, 24f), perf, "저사양 프리셋 (성능 우선)");
                if (perfNew != perf)
                {
                    GameUserSettings.SetGraphicQualityPreset(
                        perfNew ? DemoGraphicQualityPreset.Performance : DemoGraphicQualityPreset.Balanced);
                }

                y += 36f;
                GUI.Label(new Rect(innerPadX, y, 160f, 22f), "UI 크기 (IMGUI)");
                float uiSc = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.UiScale01,
                    0.75f,
                    1.35f);
                if (!Mathf.Approximately(uiSc, GameUserSettings.UiScale01))
                {
                    GameUserSettings.SetUiScale01(uiSc);
                }

                y += 40f;

                // --- 입력 안내(Battle Aces 범위) ---
                DrawSectionTitle(ref y, innerPadX, innerW, "입력 안내");
                DrawSectionHint(ref y, innerPadX, innerW, DemoPresentationCopy.SettingsInputNoRebindHint);

                y += 8f;

                // --- 접근성 ---
                DrawSectionTitle(ref y, innerPadX, innerW, "접근성");
                DrawSectionHint(
                    ref y,
                    innerPadX,
                    innerW,
                    "미니맵 색약: 아트 팔레트(티얼·앰버)와 같은 축에서 파랑·시안/주황만 더 벌림 — BATTLE_ACES_READABILITY.md. 브리핑 속도는 자막과 동일 슬라이더.");

                bool cb = GameUserSettings.ColorblindFriendlyMinimap;
                bool cbNew = GUI.Toggle(new Rect(innerPadX, y, innerW, 24f), cb, "미니맵 색약 (아군 청·시안 / 적 앰버·주황)");
                if (cbNew != cb)
                {
                    GameUserSettings.SetColorblindFriendlyMinimap(cbNew);
                }

                y += 32f;
                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 200f, 22f), "브리핑·대사 속도 (글자/초)");
                float dCps = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.DialogueRevealCharsPerSecond,
                    28f,
                    280f);
                if (!Mathf.Approximately(dCps, GameUserSettings.DialogueRevealCharsPerSecond))
                {
                    GameUserSettings.SetDialogueRevealCharsPerSecond(dCps);
                }

                y += 38f;
                if (GUI.Button(new Rect(innerPadX, y, 168f, 28f), "설정 저장"))
                {
                    GameUserSettings.Save();
                }
            }
            finally
            {
                GUI.EndScrollView();
                GUI.skin.label.fontSize = prevFont;
                GUI.color = prevGuiColor;
            }

            ImGuiGameUi.EndScaledGui();
        }

        /// <summary>섹션 제목 — 금색 강조</summary>
        private static void DrawSectionTitle(ref float y, float padX, float innerW, string title)
        {
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(padX, y, innerW, 22f), title);
            y += 22f;
        }

        /// <summary>섹션 설명 한 줄 — 회색</summary>
        private static void DrawSectionHint(ref float y, float padX, float innerW, string hint)
        {
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(padX, y, innerW, 30f), hint);
            y += 30f;
            GUI.color = Color.white;
        }
    }
}
