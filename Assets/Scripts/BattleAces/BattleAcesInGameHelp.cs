using Game.Campaign;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    public sealed class BattleAcesInGameHelp : MonoBehaviour
    {
        private bool visible;

        /// <summary>자세히 펼침 — 매번 F1으로 열 때는 요약부터(접힌 상태)</summary>
        private bool helpDetailsExpanded;

        /// <summary>이번 세션에서 자동 오픈을 이미 시도했는지(매 프레임 재오픈 방지)</summary>
        private bool autoIntroAttemptedThisSession;

        /// <summary>자동으로 연 F1 패널 — 닫을 때만 진행 저장</summary>
        private bool pendingFirstRunIntroMark;

        private GUIStyle helpSummaryStyle;
        private GUISkin helpSummaryStyleSkin;
        private GUIStyle helpMissionHintStyle;
        private GUISkin helpMissionHintStyleSkin;
        private GUIStyle helpDetailBodyStyle;
        private GUISkin helpDetailBodyStyleSkin;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baMatch) && baMatch.IsFinished)
            {
                visible = false;
                helpDetailsExpanded = false;
                return;
            }

            if (CampaignBattleFlow.Instance != null && CampaignBattleFlow.Instance.IsBriefingBlocking)
            {
                visible = false;
                helpDetailsExpanded = false;
                return;
            }

            TryAutoOpenFirstCampaignIntro();

            if (kb.f1Key.wasPressedThisFrame)
            {
                if (visible && pendingFirstRunIntroMark)
                {
                    CampaignProgressStorage.MarkCampaignF1IntroCompleted();
                    pendingFirstRunIntroMark = false;
                }

                visible = !visible;
                helpDetailsExpanded = false;
            }
        }

        /// <summary>캠페인 최초 1회 — 브리핑 종료 직후 F1 도움말·목표 요약을 자동 표시</summary>
        private void TryAutoOpenFirstCampaignIntro()
        {
            if (autoIntroAttemptedThisSession || !CampaignProgressStorage.ShouldAutoShowCampaignF1Intro())
            {
                return;
            }

            PersistentGameCore core = PersistentGameCore.Instance;
            if (core == null || core.ActiveMission == null)
            {
                autoIntroAttemptedThisSession = true;
                return;
            }

            CampaignBattleFlow flow = CampaignBattleFlow.Instance;
            if (flow == null || !flow.IsGameplayStarted)
            {
                return;
            }

            autoIntroAttemptedThisSession = true;
            visible = true;
            helpDetailsExpanded = false;
            pendingFirstRunIntroMark = true;
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();
            float w = Mathf.Min(580f, Screen.width - 40f);

            PersistentGameCore coreForLayout = PersistentGameCore.Instance;
            MissionDefinition missionForLayout = coreForLayout != null ? coreForLayout.ActiveMission : null;
            float missionExtraH = 0f;
            if (missionForLayout != null)
            {
                missionExtraH = 120f;
                string hintPreview = MissionObjectiveDisplayText.GetGameplayHint(missionForLayout.ObjectiveKind);
                if (!string.IsNullOrEmpty(hintPreview) && hintPreview.Length > 72)
                {
                    missionExtraH += 56f;
                }
            }

            float collapsedCardH = 420f + missionExtraH;
            float expandedCardH = 600f + missionExtraH;
            if (missionForLayout != null && expandedCardH < 640f)
            {
                expandedCardH = 640f;
            }

            float targetH = helpDetailsExpanded ? expandedCardH : collapsedCardH;
            float maxH = Screen.height - 72f;
            float h = Mathf.Min(targetH, maxH);
            if (helpDetailsExpanded)
            {
                h = Mathf.Max(h, Mathf.Min(500f, maxH));
            }
            else
            {
                h = Mathf.Max(h, Mathf.Min(360f, maxH));
            }

            Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);

            int prevLabelFont = GUI.skin.label.fontSize;
            Color prevGuiColor = GUI.color;

            GUI.skin.label.fontSize = 20;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(card.x + 18f, card.y + 14f, card.width - 130f, 32f), "조작 안내");
            float bodyTopY = card.y + 42f;

            PersistentGameCore core = PersistentGameCore.Instance;
            MissionDefinition mission = core != null ? core.ActiveMission : null;
            if (mission != null)
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.VictoryTint;
                GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), "이번 작전 목표");
                bodyTopY += 22f;
                GUI.skin.label.fontSize = 15;
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(
                    new Rect(card.x + 18f, bodyTopY, card.width - 36f, 26f),
                    MissionObjectiveDisplayText.GetPrimaryLine(mission.ObjectiveKind));
                bodyTopY += 28f;
                string hint = MissionObjectiveDisplayText.GetGameplayHint(mission.ObjectiveKind);
                if (!string.IsNullOrEmpty(hint))
                {
                    GUIStyle hintStyle = GetOrCreateMissionHintStyle();
                    float hintH = Mathf.Clamp(
                        hintStyle.CalcHeight(new GUIContent(hint), card.width - 36f),
                        28f,
                        88f);
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, hintH), hint, hintStyle);
                    bodyTopY += hintH + 6f;
                }
            }

            // --- 한 장 요약(5줄) ---
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), "핵심 요약");
            bodyTopY += 24f;

            const string summaryFiveLines =
                "• 1~8 덱 생산 · 우클릭 이동/공격\n" +
                "• P · 1·2·3 일시정지·배속\n" +
                "• O 설정(음량·UI) · Shift+M 미니맵 · F1 이 창\n" +
                "• 미니맵 클릭·Shift+박스 선택 · 우하 색 범례\n" +
                "• T/Y/U 코어 · V 표적 · Space·Home 카메라";

            GUIStyle sumStyle = GetOrCreateHelpSummaryStyle();
            float sumH = sumStyle.CalcHeight(new GUIContent(summaryFiveLines), card.width - 36f);
            sumH = Mathf.Clamp(sumH, 96f, 152f);
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, sumH), summaryFiveLines, sumStyle);
            bodyTopY += sumH + 8f;

            // 자세히 토글 — HUD 버튼 스타일
            Rect detailToggleRect = new Rect(card.x + 18f, bodyTopY, Mathf.Min(340f, card.width - 40f), 32f);
            string toggleLabel = helpDetailsExpanded ? "접기 ▲ (전체 목록 숨김)" : "자세히 보기 ▼ (전체 조작 목록)";
            if (ImGuiGameUi.GameMenuButton(detailToggleRect, toggleLabel))
            {
                helpDetailsExpanded = !helpDetailsExpanded;
            }

            bodyTopY += 38f;
            GUI.color = Color.white;

            if (helpDetailsExpanded)
            {
                GUI.skin.label.fontSize = 13;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(card.x + 18f, bodyTopY, card.width - 36f, 44f),
                    "집에서 처음 켤 때: O 로 볼륨·UI 크기·전체화면을 맞추고 「설정 저장」을 누르세요.");
                bodyTopY += 48f;

                const string bodyFull =
                    "P: 일시정지 토글 · 숫자 1·2·3: 배속(느림·보통·빠름)\n" +
                    "우클릭: 선택 유닛 이동 / 적·목표 공격\n" +
                    "1~8: 덱 슬롯에 맞춰 유닛 생산\n" +
                    "T / Y / U: 코어 업그레이드(생산·장갑·자원)\n" +
                    "Space: 교전 중심으로 카메라 · Home: 아군 코어로 카메라\n" +
                    "미니맵: 클릭 이동 · 드래그 패닝 · Shift+좌 드래그로 아군만 사각 선택 · 우하 색 범례 접기\n" +
                    "V: 자동 표적 모드 전환 · F1: 이 도움말";

                GUIStyle detailStyle = GetOrCreateHelpDetailBodyStyle();
                const float footerReserve = 50f;
                float availableForBody = card.yMax - bodyTopY - footerReserve;
                float bodyRectH = Mathf.Max(0f, Mathf.Min(availableForBody, 320f));
                if (bodyRectH >= 36f)
                {
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, bodyRectH), bodyFull, detailStyle);
                }
            }

            GUI.color = ImGuiGameUi.TextMuted;
            GUI.skin.label.fontSize = 13;
            GUI.Label(new Rect(card.x + 18f, card.yMax - 44f, card.width - 36f, 28f), "다시 F1 을 누르면 닫습니다.");
            GUI.color = Color.white;

            Rect closeBtn = new Rect(card.xMax - 120f, card.y + 12f, 100f, 32f);
            if (ImGuiGameUi.GameMenuButton(closeBtn, "닫기 (F1)"))
            {
                if (pendingFirstRunIntroMark)
                {
                    CampaignProgressStorage.MarkCampaignF1IntroCompleted();
                    pendingFirstRunIntroMark = false;
                }

                visible = false;
                helpDetailsExpanded = false;
            }

            GUI.skin.label.fontSize = prevLabelFont;
            GUI.color = prevGuiColor;

            ImGuiGameUi.EndScaledGui();
        }

        private GUIStyle GetOrCreateHelpSummaryStyle()
        {
            if (helpSummaryStyle != null && helpSummaryStyleSkin == GUI.skin)
            {
                return helpSummaryStyle;
            }

            helpSummaryStyleSkin = GUI.skin;
            helpSummaryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            helpSummaryStyle.normal.textColor = ImGuiGameUi.TextTitle;
            helpSummaryStyle.lineSpacing = 1.15f;
            return helpSummaryStyle;
        }

        /// <summary>미션 목표 힌트 — 2줄 GetGameplayHint 대응</summary>
        private GUIStyle GetOrCreateMissionHintStyle()
        {
            if (helpMissionHintStyle != null && helpMissionHintStyleSkin == GUI.skin)
            {
                return helpMissionHintStyle;
            }

            helpMissionHintStyleSkin = GUI.skin;
            helpMissionHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            helpMissionHintStyle.normal.textColor = ImGuiGameUi.TextMuted;
            helpMissionHintStyle.lineSpacing = 1.12f;
            return helpMissionHintStyle;
        }

        /// <summary>자세히 본문 — 좁은 카드에서 줄바꿈</summary>
        private GUIStyle GetOrCreateHelpDetailBodyStyle()
        {
            if (helpDetailBodyStyle != null && helpDetailBodyStyleSkin == GUI.skin)
            {
                return helpDetailBodyStyle;
            }

            helpDetailBodyStyleSkin = GUI.skin;
            helpDetailBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            helpDetailBodyStyle.normal.textColor = ImGuiGameUi.TextTitle;
            helpDetailBodyStyle.lineSpacing = 1.12f;
            return helpDetailBodyStyle;
        }
    }
}
