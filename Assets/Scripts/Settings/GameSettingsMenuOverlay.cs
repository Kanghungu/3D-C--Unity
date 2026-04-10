using System;
using Game.Audio;
using Game.BattleAces;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Settings
{
    /// <summary>
    /// In-game settings overlay opened with O. Groups master audio, battle visuals,
    /// graphics presets, and accessibility settings for the Battle Aces demo.
    /// </summary>
    public sealed class GameSettingsMenuOverlay : MonoBehaviour
    {
        private const float PanelWidth = 468f;
        private const float PanelHeightPreferred = 620f;
        private const float PanelLeft = 24f;
        private const float PanelBottomMargin = 16f;
        private const float ContentHeightApprox = 860f;

        private bool panelOpen;

        /// <summary>Short cooldown that prevents slider tick audio from spamming.</summary>
        private float lastSettingsSliderAudioUnscaled = -999f;

        /// <summary>Scroll position for the long settings panel content.</summary>
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

                // --- Master ---
                DrawSectionTitle(ref y, innerPadX, innerW, "마스터");
                DrawSectionHint(ref y, innerPadX, innerW, "게임 전체에 적용되는 출력 볼륨입니다.");

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
                    TryPlaySettingsSliderTick();
                }

                y += 38f;

                // --- Battle only ---
                DrawSectionTitle(ref y, innerPadX, innerW, "전투 전용");
                DrawSectionHint(ref y, innerPadX, innerW, "전투 중 믹서 그룹만 조절합니다. BGM과 결과 스팅은 별도입니다.");

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
                    TryPlaySettingsSliderTick();
                }

                y += 36f;
                GUI.Label(new Rect(innerPadX, y, 170f, 22f), "결과 스팅");
                float st = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.ResultStingVolume01,
                    0f,
                    1f);
                if (!Mathf.Approximately(st, GameUserSettings.ResultStingVolume01))
                {
                    GameUserSettings.SetResultStingVolume01(st);
                    TryPlaySettingsSliderTick();
                }

                y += 38f;

                // --- Battle fog ---
                DrawSectionTitle(ref y, innerPadX, innerW, "전투 안개 (선택)");
                DrawSectionHint(ref y, innerPadX, innerW, "Classic Duel 계열 데모 전장에서만 안개·가시감 값을 조정합니다.");

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
                    TryPlaySettingsSliderTick();
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
                    TryPlaySettingsSliderTick();
                }

                y += 40f;

                // --- Screen ---
                DrawSectionTitle(ref y, innerPadX, innerW, "화면");
                DrawSectionHint(ref y, innerPadX, innerW, "카메라 감도, 전체 화면, HUD(IMGUI) 표시 크기를 조절합니다.");

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
                    TryPlaySettingsSliderTick();
                }

                y += 36f;
                bool fs = Screen.fullScreen;
                bool newFs = GUI.Toggle(new Rect(innerPadX, y, innerW, 22f), fs, "전체 화면");
                if (newFs != fs)
                {
                    GameUserSettings.SetFullscreen(newFs);
                }

                y += 32f;

                // --- Graphics ---
                DrawSectionTitle(ref y, innerPadX, innerW, "그래픽");
                DrawSectionHint(ref y, innerPadX, innerW, DemoPresentationCopy.SettingsGraphicPresetSectionHint);

                DemoGraphicQualityPreset[] graphicPresetGridOrder =
                {
                    DemoGraphicQualityPreset.Performance,
                    DemoGraphicQualityPreset.Balanced,
                    DemoGraphicQualityPreset.High
                };
                int graphicGridIndex = Array.IndexOf(graphicPresetGridOrder, GameUserSettings.GraphicQualityPreset);
                if (graphicGridIndex < 0)
                {
                    graphicGridIndex = 1;
                }

                int prevBtn = GUI.skin.button.fontSize;
                GUI.skin.button.fontSize = 13;
                int newGraphicGridIndex = GUI.SelectionGrid(
                    new Rect(innerPadX, y, innerW, 78f),
                    graphicGridIndex,
                    DemoPresentationCopy.SettingsGraphicPresetGridLabels,
                    1);
                GUI.skin.button.fontSize = prevBtn;
                if (newGraphicGridIndex != graphicGridIndex)
                {
                    GameUserSettings.SetGraphicQualityPreset(graphicPresetGridOrder[newGraphicGridIndex]);
                    // Keep fog presentation in sync when the preset changes on a Classic Duel stage.
                    BattleAcesWorldPresentation.RefreshBattleFogIfClassicDuelActive();
                }

                y += 82f;
                GUI.Label(new Rect(innerPadX, y, 160f, 22f), "UI 크기 (IMGUI)");
                float uiSc = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.UiScale01,
                    0.75f,
                    1.35f);
                if (!Mathf.Approximately(uiSc, GameUserSettings.UiScale01))
                {
                    GameUserSettings.SetUiScale01(uiSc);
                    TryPlaySettingsSliderTick();
                }

                y += 40f;

                // --- Input hint ---
                DrawSectionTitle(ref y, innerPadX, innerW, "입력 안내");
                DrawSectionHint(ref y, innerPadX, innerW, DemoPresentationCopy.SettingsInputNoRebindHint);

                y += 8f;

                // --- Accessibility ---
                DrawSectionTitle(ref y, innerPadX, innerW, "접근성");
                DrawSectionHint(
                    ref y,
                    innerPadX,
                    innerW,
                    "미니맵 색약 모드: 아군 티얼 축과 적 앰버 축을 유지하면서 명도와 채도를 더 벌립니다. 자세한 기준은 BATTLE_ACES_READABILITY.md를 참고하십시오.");

                bool cb = GameUserSettings.ColorblindFriendlyMinimap;
                bool cbNew = GUI.Toggle(new Rect(innerPadX, y, innerW, 24f), cb, "미니맵 색약 모드 (아군 청록 / 적 앰버 강조)");
                if (cbNew != cb)
                {
                    GameUserSettings.SetColorblindFriendlyMinimap(cbNew);
                }

                y += 32f;
                GUI.skin.label.fontSize = 13;
                GUI.color = Color.white;
                GUI.Label(new Rect(innerPadX, y, 200f, 22f), "브리핑/결과 대화 속도 (글자/초)");
                float dCps = GUI.HorizontalSlider(
                    new Rect(sliderLeft, y + 4f, sliderW, 18f),
                    GameUserSettings.DialogueRevealCharsPerSecond,
                    28f,
                    280f);
                if (!Mathf.Approximately(dCps, GameUserSettings.DialogueRevealCharsPerSecond))
                {
                    GameUserSettings.SetDialogueRevealCharsPerSecond(dCps);
                    TryPlaySettingsSliderTick();
                }

                y += 38f;
                if (GUI.Button(new Rect(innerPadX, y, 168f, 28f), "설정 저장"))
                {
                    GameUserSettings.Save();
                    ProceduralAudioUtility.PlayUiMenuAck();
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

        /// <summary>Plays a restrained slider tick so rapid dragging does not spam audio.</summary>
        private void TryPlaySettingsSliderTick()
        {
            const float cooldownUnscaled = 0.05f;
            if (Time.unscaledTime - lastSettingsSliderAudioUnscaled < cooldownUnscaled)
            {
                return;
            }

            lastSettingsSliderAudioUnscaled = Time.unscaledTime;
            ProceduralAudioUtility.PlayUiSliderTick();
        }

        /// <summary>Draws a compact section title with accent color.</summary>
        private static void DrawSectionTitle(ref float y, float padX, float innerW, string title)
        {
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(padX, y, innerW, 22f), title);
            y += 22f;
        }

        /// <summary>Draws the muted helper line shown under each section title.</summary>
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
