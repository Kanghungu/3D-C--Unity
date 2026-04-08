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
                // 상단 바와 같이: 데모 제목 + 목표 + 힌트(긴 미션은 추가 높이)
                missionExtraH = 148f;
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
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(card.x + 18f, card.y + 14f, card.width - 130f, 32f), "조작 안내");
            float bodyTopY = card.y + 42f;

            PersistentGameCore core = PersistentGameCore.Instance;
            MissionDefinition mission = core != null ? core.ActiveMission : null;
            if (mission != null)
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.VictoryTint;
                GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), "이번 작전 (상단 바와 동일)");
                bodyTopY += 22f;

                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.AccentGold;
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

                // 공중 요새 변주 — 이 데모에선 건물 클릭 선택 없음
                if (mission.AirborneCitadelFocus)
                {
                    GUI.skin.label.fontSize = 13;
                    GUI.skin.label.wordWrap = true;
                    GUI.color = ImGuiGameUi.AccentGold;
                    const string airborneNote =
                        "공중 요새(공성) 변주: 적 거점을 마우스로 선택하지 않습니다. " +
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
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, 22f), "핵심 요약");
            bodyTopY += 24f;

            // Battle Aces(NewSampleScene)에 실제로 붙어 있는 조작만 — 없는 시스템은 아래 「이 데모에 없음」
            const string summarySixLines =
                "• 왼쪽 HUD: 자원·덱 1~8·생산 큐·집결(Alt+지면 우클릭)·T/Y/U 본진 강화\n" +
                "• 선택: 좌클릭/드래그(아군만) · 우클릭 이동·공격 · Ctrl+A 전체 · Esc 해제\n" +
                "• 카메라: WASD·화살표·가장자리·휠(회전 없음) · Space/ Home · ,(쉼표) 집결 시야\n" +
                "• 우하단 전술 지도: 클릭·드래그 · Ctrl+클릭·Shift 드래그 선택 · Shift+M 크기\n" +
                "• P 일시정지 · [ ]·숫자패드 ± 배속 · O 설정 · V 자동 표적 · H/G/B(유닛 선택 시)\n" +
                "• F1 이 창 · 승패 화면에서만 R 재시작";

            GUIStyle sumStyle = GetOrCreateHelpSummaryStyle();
            float sumH = sumStyle.CalcHeight(new GUIContent(summarySixLines), card.width - 36f);
            sumH = Mathf.Clamp(sumH, 120f, 220f);
            GUI.Label(new Rect(card.x + 18f, bodyTopY, card.width - 36f, sumH), summarySixLines, sumStyle);
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
                    "— Battle Aces 데모에 있는 것만 —\n" +
                    "P: 일시정지 · [ / ] 또는 숫자패드 - +: 배속 단계\n" +
                    "O: 설정(볼륨·UI 크기·전체화면 등)\n" +
                    "좌클릭·드래그: 아군만 선택 · Ctrl+A: 살아 있는 아군 전체 · Esc: 선택 해제\n" +
                    "우클릭: 이동 / 적·목표 공격\n" +
                    "Alt+지면 우클릭: 집결(랠리) — 청색 링 · ,(쉼표): 랠리로 카메라\n" +
                    "1~8: 덱 생산 주문(자원·큐 제한 시 짧은 거절음)\n" +
                    "T / Y / U: 본진 생산·장갑·자원 강화(왼쪽 HUD 비용 표시)\n" +
                    "H / G / B: 홀드 / 수비(가까운 아군 거점) / 후퇴 — 유닛 선택 시\n" +
                    "카메라: WASD·화살표·가장자리 · 휠 줌(키보드 회전 없음 · Ctrl 누른 채 WASD는 카메라 이동 안 함)\n" +
                    "Space: 교전 쪽 시야 · Home: 아군 코어\n" +
                    "전술 지도: 클릭·드래그 이동 · Ctrl+클릭 근처 아군 · Shift+드래그 박스 선택 · Shift+M 크기\n" +
                    "V: 자동 표적(가까운 적 우선) 토글 · F1: 이 창\n" +
                    "R: 승리/패배 결과 화면에서만 같은 씬 재시작\n" +
                    "Ctrl+F2~F5: 부대 단축 지정 · F2~F5: 불러오기(더블 탭 시 해당 부대로 카메라)\n" +
                    "※ F1은 도움말 전용이라 F1 단축 그룹은 쓰이지 않습니다.\n\n" +
                    "— 이 데모에 없음 —\n" +
                    "멀티플레이, 기술 트리, 본진 외 건물 건설, 거점/건물 마우스 선택 후 명령, 캠페인 외 맵 편집 등";

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
