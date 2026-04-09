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

            if (BattleMissionFlow.Instance != null && BattleMissionFlow.Instance.IsBriefingBlocking)
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

            BattleMissionFlow flow = BattleMissionFlow.Instance;
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

            GUI.depth = -5000;
            ImGuiGameUi.BeginScaledGui();
            float w = Mathf.Min(580f, Screen.width - 40f);

            PersistentGameCore coreForLayout = PersistentGameCore.Instance;
            MissionDefinition missionForLayout = coreForLayout != null ? coreForLayout.ActiveMission : null;
            float missionExtraH = 0f;
            if (missionForLayout != null)
            {
                bool compactMission = ShouldCompactF1MissionBlock(missionForLayout);
                if (compactMission)
                {
                    // 상단 UGUI에 1차 목표가 있을 때 — F1 은 보조만(높이 축소)
                    missionExtraH = 102f;
                }
                else
                {
                    missionExtraH = 148f;
                }

                string hintPreview = MissionObjectiveDisplayText.GetGameplayHint(missionForLayout);
                if (!string.IsNullOrEmpty(hintPreview) && hintPreview.Length > 72)
                {
                    missionExtraH += 56f;
                }

                if (missionForLayout.AirborneCitadelFocus)
                {
                    missionExtraH += 52f;
                }
            }

            float collapsedCardH = 458f + missionExtraH;
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
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(new Rect(card.x + 18f, card.y + 14f, card.width - 130f, 32f), DemoPresentationCopy.HelpF1PanelTitle);
            float bodyTopY = card.y + 42f;

            PersistentGameCore core = PersistentGameCore.Instance;
            MissionDefinition mission = core != null ? core.ActiveMission : null;
            if (mission != null)
            {
                bool compactMission = ShouldCompactF1MissionBlock(mission);
                if (compactMission)
                {
                    GUI.skin.label.fontSize = 14;
                    GUI.color = ImGuiGameUi.AccentGold;
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), DemoPresentationCopy.HelpF1SectionRoleLine);
                    bodyTopY += 24f;

                    GUI.skin.label.fontSize = 13;
                    GUI.color = ImGuiGameUi.TextMuted;
                    GUI.Label(
                        new Rect(card.x + 18f, bodyTopY, card.width - 36f, 36f),
                        DemoPresentationCopy.HelpF1PrimaryOnTopBarLine);
                    bodyTopY += 40f;

                    string hintCompact = MissionObjectiveDisplayText.GetGameplayHint(mission);
                    if (!string.IsNullOrEmpty(hintCompact))
                    {
                        GUI.skin.label.fontSize = 14;
                        GUI.color = ImGuiGameUi.AccentCyan;
                        GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 20f), DemoPresentationCopy.HelpF1SecondaryHintsHeader);
                        bodyTopY += 22f;

                        GUIStyle hintStyleC = GetOrCreateMissionHintStyle();
                        float hintHc = Mathf.Clamp(
                            hintStyleC.CalcHeight(new GUIContent(hintCompact), card.width - 36f),
                            28f,
                            96f);
                        GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, hintHc), hintCompact, hintStyleC);
                        bodyTopY += hintHc + 6f;
                    }
                }
                else
                {
                    GUI.skin.label.fontSize = 14;
                    GUI.color = ImGuiGameUi.AccentCyan;
                    GUI.Label(
                        new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f),
                        DemoPresentationCopy.HelpMissionThisOperationHeader);
                    bodyTopY += 22f;

                    GUI.skin.label.fontSize = 14;
                    GUI.color = ImGuiGameUi.TextTitle;
                    string barTitle = BattleAcesObjectiveUgui.FormatTopBarDemoTitle(mission);
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), barTitle);
                    bodyTopY += 24f;

                    GUI.skin.label.fontSize = 15;
                    GUI.color = ImGuiGameUi.TextTitle;
                    GUI.Label(
                        new Rect(card.x + 18f, bodyTopY, card.width - 36f, 24f),
                        MissionObjectiveDisplayText.GetPrimaryLine(mission));
                    bodyTopY += 26f;

                    string hint = MissionObjectiveDisplayText.GetGameplayHint(mission);
                    if (!string.IsNullOrEmpty(hint))
                    {
                        GUIStyle hintStyle = GetOrCreateMissionHintStyle();
                        float hintH = Mathf.Clamp(
                            hintStyle.CalcHeight(new GUIContent(hint), card.width - 36f),
                            28f,
                            96f);
                        GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, hintH), hint, hintStyle);
                        bodyTopY += hintH + 6f;
                    }
                }

                // 공중 요새 변주 — 이 데모에선 건물 클릭 선택 없음
                if (mission.AirborneCitadelFocus)
                {
                    GUI.skin.label.fontSize = 13;
                    GUI.skin.label.wordWrap = true;
                    GUI.color = ImGuiGameUi.TextMuted;
                    const string airborneNote =
                        "공중 요새(공성) 변주: 적 코어를 마우스로 선택하지 않습니다. " +
                        "덱 1~8 생산 후 부대 선택·우클릭 이동/공격만 사용합니다.";
                    float airH = GUI.skin.label.CalcHeight(new GUIContent(airborneNote), card.width - 36f);
                    airH = Mathf.Clamp(airH, 40f, 78f);
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, airH), airborneNote);
                    GUI.color = Color.white;
                    bodyTopY += airH + 8f;
                }
            }

            // --- 한 장 요약(5줄) ---
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), DemoPresentationCopy.HelpSummarySectionTitle);
            bodyTopY += 24f;

            string summarySixLines = DemoPresentationCopy.BuildHelpCoreSummarySixLines();
            GUIStyle sumStyle = GetOrCreateHelpSummaryStyle();
            float sumH = sumStyle.CalcHeight(new GUIContent(summarySixLines), card.width - 36f);
            sumH = Mathf.Clamp(sumH, 120f, 248f);
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, sumH), summarySixLines, sumStyle);
            bodyTopY += sumH + 8f;

            // 자세히 토글 — HUD 버튼 스타일
            Rect detailToggleRect = new Rect(card.x + 18f, bodyTopY, Mathf.Min(340f, card.width - 40f), 32f);
            string toggleLabel = helpDetailsExpanded
                ? DemoPresentationCopy.HelpDetailToggleCollapse
                : DemoPresentationCopy.HelpDetailToggleExpand;
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

                string bodyFull = DemoPresentationCopy.BuildBattleAcesF1ExpandedDetailBody();

                GUIStyle detailStyle = GetOrCreateHelpDetailBodyStyle();
                const float footerReserve = 50f;
                float availableForBody = card.yMax - bodyTopY - footerReserve;
                float bodyRectH = Mathf.Max(0f, Mathf.Min(availableForBody, 380f));
                if (bodyRectH >= 36f)
                {
                    GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, bodyRectH), bodyFull, detailStyle);
                }
            }

            GUI.color = ImGuiGameUi.TextMuted;
            GUI.skin.label.fontSize = 13;
            GUI.Label(new Rect(card.x + 18f, card.yMax - 44f, card.width - 36f, 28f), DemoPresentationCopy.HelpF1FooterCloseHint);
            GUI.color = Color.white;

            Rect closeBtn = new Rect(card.xMax - 120f, card.y + 12f, 100f, 32f);
            if (ImGuiGameUi.GameMenuButton(closeBtn, DemoPresentationCopy.HelpF1CloseButton))
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

        /// <summary>상단 UGUI 목표 바가 켜진 전투 중에는 F1 에서 주 목표 문구를 반복하지 않음(C축)</summary>
        private static bool ShouldCompactF1MissionBlock(MissionDefinition mission)
        {
            if (mission == null)
            {
                return false;
            }

            BattleMissionFlow flow = BattleMissionFlow.Instance;
            if (flow == null || !flow.IsGameplayStarted)
            {
                return false;
            }

            BattleAcesObjectiveUgui ugui = BattleAcesObjectiveUgui.Instance;
            return ugui != null && ugui.HasObjectiveUi;
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
            return helpDetailBodyStyle;
        }
    }
}
