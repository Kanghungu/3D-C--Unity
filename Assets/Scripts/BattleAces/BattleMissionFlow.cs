using Game.Audio;
using Game.Campaign;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Settings;
using Game.UI;
using Game.Units;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// 전투 씬 안에서 미션 브리핑·승패 결과·진행 저장 훅.
    /// 캠페인에서 들어오든 데모 단독 Play든 동일 컴포넌트(파일명만 미션 중심으로 정리).
    /// </summary>
    public sealed class BattleMissionFlow : MonoBehaviour
    {
        /// <summary>승패 카드 공통 안내 — CampaignDialogue_KR 의 result_campaign_next_hint 와 동일 키</summary>
        private const string ResultCampaignNextHintDialogueId = "result_campaign_next_hint";

        private MissionDefinition mission;
        private BattleAcesMatchController match;
        private BattleAcesEconomy economyRef;
        private BattleAcesRunStats runStatsRef;

        /// <summary>이번 판 보조 목표 달성 여부(결과 카드 표시)</summary>
        private bool bonusMetThisRound;

        private bool briefingActive;
        private bool gameplayStarted;
        private float gameplayStartTime;
        private float gameplayStartUnscaledTime;
        private bool showResultOverlay;
        private float resultPlaySeconds;
        private int resultPlayerCredits;

        // 승패 본문 — 기본 IMGUI 라벨보다 줄간격을 넓혀 한글 가독성 개선
        private GUIStyle resultBodyLabelStyle;
        private GUISkin resultBodyLabelStyleSkin;

        // 「다음 미션은 메뉴에서」 통일 안내 — 좁은 카드 폭에서 줄바꿈
        private GUIStyle resultNextStepHintStyle;
        private GUISkin resultNextStepHintStyleSkin;

        /// <summary>브리핑 타이핑 효과 — 접근성 설정 초당 글자 수</summary>
        private string briefingRevealSourceText;
        private float briefingRevealStartUnscaled;

        public bool IsGameplayStarted => gameplayStarted;
        public float GameplayStartTime => gameplayStartTime;

        /// <summary>작전 시작 시각(unscaled) — 데모 첫 30초 안내 등</summary>
        public float GameplayStartUnscaledTime => gameplayStartUnscaledTime;

        public bool IsBriefingBlocking => mission != null && briefingActive;

        /// <summary>승패 결과 카드가 떠 있는 동안(일시정지 도움말 등과 배타)</summary>
        public bool IsShowingBattleResult => showResultOverlay && match != null && match.IsFinished;

        /// <summary>직전 매치 종료 시 기록된 플레이 길이(초, unscaled) — HUD 스커미시 오버레이 등</summary>
        public float LastMatchPlaySecondsUnscaled => resultPlaySeconds;

        public static BattleMissionFlow Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        public void Initialize(
            MissionDefinition m,
            BattleAcesMatchController mc,
            BattleAcesEconomy economy = null,
            BattleAcesRunStats runStats = null)
        {
            mission = m;
            match = mc;
            economyRef = economy;
            runStatsRef = runStats;
            bonusMetThisRound = false;
            if (match != null)
            {
                match.MatchEnded += OnMatchEnded;
            }

            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.enabled = false;
                rtc.SetControlEnabled(false);
                rtc.ForcePauseGameplay();
            }
            else
            {
                Time.timeScale = 0f;
            }

            briefingActive = mission != null && !mission.SkipsCampaignBriefing;
            briefingRevealSourceText = null;
            if (!briefingActive)
            {
                StartGameplayClock();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (match != null)
            {
                match.MatchEnded -= OnMatchEnded;
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!briefingActive || mission == null)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            bool confirmKey = kb != null &&
                              (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame);
            Mouse pointer = Mouse.current;
            bool confirmClick = pointer != null && pointer.leftButton.wasPressedThisFrame;

            if (confirmKey || confirmClick)
            {
                briefingActive = false;
                StartGameplayClock();
            }
        }

        private void StartGameplayClock()
        {
            gameplayStarted = true;
            gameplayStartTime = Time.time;
            gameplayStartUnscaledTime = Time.unscaledTime;
            if (runStatsRef != null)
            {
                runStatsRef.MarkBattleClockStart();
            }
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.enabled = true;
                rtc.SetControlEnabled(true);
                rtc.ResumeFromPause(rtc.CurrentSpeedStep);
            }
            else
            {
                Time.timeScale = 1f;
            }

            ProceduralAudioUtility.PlayUiConfirm();
        }

        private void OnMatchEnded(BattleAcesMatchController.MatchState st)
        {
            resultPlaySeconds = Mathf.Max(0f, Time.unscaledTime - gameplayStartUnscaledTime);
            if (economyRef != null)
            {
                resultPlayerCredits = Mathf.RoundToInt(economyRef.PlayerCredits);
            }

            bonusMetThisRound = false;
            if (st == BattleAcesMatchController.MatchState.Victory &&
                mission != null &&
                !string.IsNullOrEmpty(mission.OptionalBonusObjectiveId) &&
                BattleAcesMissionBonusEvaluator.Evaluate(mission, match != null ? match.PlayerCore : null, runStatsRef))
            {
                bonusMetThisRound = true;
                CampaignProgressStorage.MarkBonusObjectiveCompleted(mission.MissionId);
            }

            ProceduralAudioUtility.PlayResultSting(st == BattleAcesMatchController.MatchState.Victory);
            showResultOverlay = true;
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.SetControlEnabled(false);
            }

            if (st != BattleAcesMatchController.MatchState.Victory || mission == null)
            {
                return;
            }

            if (!mission.CountsForCampaignProgress)
            {
                return;
            }

            PersistentGameCore core = PersistentGameCore.Instance;
            int orderIndex = core != null && core.PendingMissionOrderIndex >= 0
                ? core.PendingMissionOrderIndex
                : mission.CampaignSortOrder;

            CampaignProgressStorage.RegisterMissionWin(orderIndex, mission.MissionId);
        }

        private void OnGUI()
        {
            PersistentGameCore core = PersistentGameCore.Instance;

            if (mission != null && briefingActive)
            {
                DrawBriefingScreen(core);
                return;
            }

            if (mission == null || match == null || !showResultOverlay || !match.IsFinished)
            {
                return;
            }

            DrawResultScreen(core);
        }

        private void DrawBriefingScreen(PersistentGameCore core)
        {
            ImGuiGameUi.BeginScaledGui();
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            string briefId = mission.EffectiveBriefingDialogueId;
            string text = core != null && !string.IsNullOrEmpty(briefId)
                ? core.TryGetDialogue(briefId)
                : null;

            if (string.IsNullOrEmpty(text) && core != null && !string.IsNullOrEmpty(mission.BriefingDialogueId))
            {
                text = core.TryGetDialogue(mission.BriefingDialogueId);
            }

            if (string.IsNullOrEmpty(text))
            {
                text =
                    $"작전명: {MissionObjectiveDisplayText.ResolveMissionDisplayName(mission)}\n\n" +
                    $"작전 목표: {MissionObjectiveDisplayText.GetPrimaryLine(mission)}\n\n" +
                    "전열을 정비하고 적 진영 목표를 돌파하십시오.";
            }

            string textToDraw = BuildBriefingVisibleText(text);

            float cardW = Mathf.Min(760f, Screen.width - 48f);
            // 매우 낮은 해상도(640×480 근처): 상단 바·카드 겹침 스팟 체크
            bool veryLowRes = Screen.height < 520 || Screen.width < 720;
            // 작은 해상도에서 카드가 화면보다 커지지 않게(집 PC·노트북 안전)
            float maxByScreen = Mathf.Max(200f, Screen.height - (veryLowRes ? 112f : 128f));
            bool compact = mission.BriefingUseCompactFont || veryLowRes;
            float cardTargetH = compact ? 500f : 460f;
            float cardH = Mathf.Clamp(Mathf.Min(cardTargetH, maxByScreen), veryLowRes ? 200f : 220f, cardTargetH);
            float cardX = (Screen.width - cardW) * 0.5f;
            float cardY = veryLowRes ? 56f : 72f;
            Rect card = new Rect(cardX, cardY, cardW, cardH);

            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);

            int bodyFont = veryLowRes ? 14 : (compact ? 15 : 16);
            int prevLabelFont = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = bodyFont;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(card.x + 20f, card.y + 16f, card.width - 40f, card.height - 56f), textToDraw);
            GUI.color = Color.white;

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 20f, card.yMax - 44f, card.width - 40f, 32f),
                "Space / Enter / 좌클릭으로 작전 시작");
            GUI.color = Color.white;
            GUI.skin.label.fontSize = prevLabelFont;

            DrawBriefingTopObjectiveBar(mission);
            ImGuiGameUi.EndScaledGui();
        }

        /// <summary>설정의 초당 글자 수로 브리핑 본문을 점진 표시(자막 속도와 동일 슬라이더)</summary>
        private string BuildBriefingVisibleText(string fullText)
        {
            if (string.IsNullOrEmpty(fullText))
            {
                return fullText;
            }

            if (briefingRevealSourceText != fullText)
            {
                briefingRevealSourceText = fullText;
                briefingRevealStartUnscaled = Time.unscaledTime;
            }

            float cps = GameUserSettings.DialogueRevealCharsPerSecond;
            int visible = Mathf.FloorToInt((Time.unscaledTime - briefingRevealStartUnscaled) * cps);
            visible = Mathf.Clamp(visible, 0, fullText.Length);
            return fullText.Substring(0, visible);
        }

        private void DrawResultScreen(PersistentGameCore core)
        {
            ImGuiGameUi.BeginScaledGui();
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            if (mission == null || match == null)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            bool skirmishPractice = !mission.CountsForCampaignProgress;
            // 에디터에서 전투 씬만 연 폴백 데모 — 문구를 스커미시(메뉴 데모)와 구분
            bool isFallbackOneMatchDemo = mission.IsOneMatchBattleDemo;

            bool won = match.State == BattleAcesMatchController.MatchState.Victory;
            string did = won ? mission.EffectiveVictoryDialogueId : mission.EffectiveDefeatDialogueId;
            string body = core != null && !string.IsNullOrEmpty(did) ? core.TryGetDialogue(did) : null;
            if (string.IsNullOrEmpty(body))
            {
                string fallbackId = won ? mission.VictoryDialogueId : mission.DefeatDialogueId;
                body = core != null && !string.IsNullOrEmpty(fallbackId) ? core.TryGetDialogue(fallbackId) : null;
            }

            if (string.IsNullOrEmpty(body))
            {
                if (skirmishPractice)
                {
                    if (isFallbackOneMatchDemo)
                    {
                        body = won
                            ? "Battle Aces 한 판 데모 승리. 본진·덱·적 AI 루프를 익히기에 적합합니다."
                            : "데모 패배. 생산·집결(Alt+우클릭)을 조정해 R 또는 아래 버튼으로 같은 데모를 다시 시작하십시오.";
                    }
                    else
                    {
                        body = won
                            ? "연습 전투 승리. 본진·덱·적 물결 AI를 익히기에 적합합니다."
                            : "연습 전투 패배. 생산·집결(Alt+우클릭)·무대를 바꿔 다시 도전하십시오.";
                    }
                }
                else
                {
                    body = won
                        ? "적 목표를 무너뜨렸습니다. 다음 진격 준비를 시작하십시오."
                        : "전선이 붕괴되었습니다. 병력을 재정비한 뒤 다시 시도하십시오.";
                }
            }

            float cardW = Mathf.Min(700f, Screen.width - 40f);
            bool veryLowRes = Screen.height < 520 || Screen.width < 720;
            // 해상도별로 본문·통계·버튼이 붙지 않게 카드 높이(최소 높이는 본문+푸터가 들어갈 만큼)
            float cardH = Mathf.Clamp(Screen.height * (veryLowRes ? 0.62f : 0.56f), veryLowRes ? 340f : 392f, 600f);
            cardH = Mathf.Min(cardH, Mathf.Max(veryLowRes ? 300f : 320f, Screen.height - (veryLowRes ? 36f : 48f)));
            float cardY = (Screen.height - cardH) * 0.5f;
            if (veryLowRes)
            {
                cardY = Mathf.Clamp(cardY, 8f, Mathf.Max(8f, Screen.height - cardH - 8f));
            }

            Rect card = new Rect((Screen.width - cardW) * 0.5f, cardY, cardW, cardH);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, won ? ImGuiGameUi.BorderAccent : ImGuiGameUi.BorderCool, 2f);

            const float padX = 24f;
            float innerW = card.width - padX * 2f;
            float y = card.y + 18f;

            string title = won ? "작전 승리" : "작전 실패";
            GUI.skin.label.fontSize = 26;
            GUI.color = won ? ImGuiGameUi.VictoryTint : ImGuiGameUi.DefeatTint;
            GUI.Label(new Rect(card.x + padX, y, innerW, 40f), title);
            y += 44f;

            GUI.skin.label.fontSize = 17;
            GUI.color = ImGuiGameUi.AccentGold;
            string rKeyLine = isFallbackOneMatchDemo
                ? "R 키 — 같은 데모 즉시 재시작 (아래 왼쪽 버튼과 동일)"
                : skirmishPractice
                    ? "R 키 — 같은 스커미시 즉시 재시작 (아래 왼쪽 버튼과 동일)"
                    : "R 키 — 같은 미션 즉시 재시작 (아래 왼쪽 버튼과 동일)";
            GUI.Label(new Rect(card.x + padX, y, innerW, 30f), rKeyLine);
            y += 30f;

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            string escLine = skirmishPractice
                ? "Esc 키 — 메인 메뉴로 (아래 오른쪽 「메인 메뉴로」와 동일)"
                : "Esc 키 — 캠페인 메뉴로 (아래 오른쪽 「캠페인 메뉴로」와 동일)";
            GUI.Label(new Rect(card.x + padX, y, innerW, 26f), escLine);
            GUI.color = Color.white;
            y += 30f;

            string nextCampaignLine;
            if (skirmishPractice)
            {
                nextCampaignLine = isFallbackOneMatchDemo
                    ? "한 판 데모입니다. 캠페인 ●○ 진행은 바뀌지 않습니다. 「메인 메뉴로」에서 캠페인·데모를 다시 고르십시오."
                    : "연습 전투입니다. 캠페인 ●○ 진행은 바뀌지 않습니다. 메인에서 캠페인 또는 데모를 다시 고르십시오.";
            }
            else
            {
                nextCampaignLine = core != null
                    ? core.TryGetDialogue(ResultCampaignNextHintDialogueId)
                    : null;
                if (string.IsNullOrEmpty(nextCampaignLine))
                {
                    nextCampaignLine =
                        "캠페인 진행은 메뉴에서 다음 미션을 고르십시오. 아래 「캠페인 메뉴로」로 돌아갑니다.";
                }
            }

            GUIStyle nextStepStyle = GetOrCreateResultNextStepHintStyle();
            float nextStepH = nextStepStyle.CalcHeight(new GUIContent(nextCampaignLine), innerW);
            nextStepH = Mathf.Clamp(nextStepH, 22f, 64f);
            GUI.Label(new Rect(card.x + padX, y, innerW, nextStepH), nextCampaignLine, nextStepStyle);
            y += nextStepH + 8f;

            string endReasonLine = match != null
                ? FormatMatchEndReasonLine(match.LastEndReason, won)
                : string.Empty;
            if (!string.IsNullOrEmpty(endReasonLine))
            {
                GUI.Label(new Rect(card.x + padX, y, innerW, 24f), endReasonLine);
                y += 28f;
            }

            if (!won && core != null && match != null)
            {
                string retryHint = ResolveDefeatRetryHintText(core, mission, match.LastEndReason);
                if (!string.IsNullOrEmpty(retryHint))
                {
                    GUI.skin.label.fontSize = 14;
                    GUI.color = ImGuiGameUi.AccentGold;
                    GUI.Label(new Rect(card.x + padX, y, innerW, 40f), retryHint);
                    y += 44f;
                }
            }

            // 본문 아래: 여백 + 통계 + 안내 + 버튼 영역 — 패배 힌트·상단 다음-미션 안내(2줄) 여유
            float footerBlock = veryLowRes ? 196f : 208f;
            if (runStatsRef != null || (mission != null && !string.IsNullOrEmpty(mission.OptionalBonusObjectiveId)))
            {
                footerBlock += 36f;
            }
            float bodyH = Mathf.Clamp(card.yMax - y - footerBlock, 40f, 900f);
            GUIStyle bodyStyle = GetOrCreateResultBodyLabelStyle();
            GUI.Label(new Rect(card.x + padX, y, innerW, bodyH), body, bodyStyle);

            float afterBody = y + bodyH + 10f;
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextMuted;
            // 통계 한 줄 — RunStats(생산·격파·손실) + 플레이 시간 mm:ss
            string statsOneLine = runStatsRef != null
                ? runStatsRef.BuildFullResultSummaryLine(resultPlaySeconds, resultPlayerCredits)
                : $"플레이 {BattleAcesRunStats.FormatPlayTimeMmSs(resultPlaySeconds)} · 종료 시 자원 {resultPlayerCredits}";
            GUI.Label(new Rect(card.x + padX, afterBody, innerW, 36f), statsOneLine);

            float extraY = afterBody + 34f;

            if (mission != null && !string.IsNullOrEmpty(mission.OptionalBonusObjectiveId))
            {
                GUI.skin.label.fontSize = 13;
                string desc = BattleAcesMissionBonusEvaluator.DescribeBonusForUi(mission.OptionalBonusObjectiveId);
                string bonusText = won
                    ? (bonusMetThisRound
                        ? $"보조 목표: 달성 — {desc}"
                        : $"보조 목표: 미달성 — {desc}")
                    : $"보조 목표: 패배로 미적용 — {desc}";
                GUI.color = bonusMetThisRound && won ? ImGuiGameUi.AccentGold : ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(card.x + padX, extraY, innerW, 40f), bonusText);
                GUI.color = ImGuiGameUi.TextMuted;
                extraY += 42f;
            }

            GUI.skin.label.fontSize = 12;
            GUI.Label(
                new Rect(card.x + padX, extraY, innerW, 34f),
                "결과 후 콘솔(경고·에러) 확인. R 재시작은 상단 금색 안내와 같습니다.");
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 14;

            float btnY = card.yMax - 72f;
            float btnW = (card.width - 64f) * 0.5f;
            Rect retry = new Rect(card.x + 24f, btnY, btnW - 8f, 48f);
            Rect menu = new Rect(card.x + 32f + btnW, btnY, btnW - 8f, 48f);

            string retryLabel = isFallbackOneMatchDemo
                ? "같은 데모 재시작"
                : skirmishPractice
                    ? "같은 스커미시 재시작"
                    : "같은 미션 재시작";
            if (ImGuiGameUi.GameMenuButton(retry, retryLabel))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().path);
            }

            string menuButtonLabel = skirmishPractice ? "메인 메뉴로" : "캠페인 메뉴로";
            if (ImGuiGameUi.GameMenuButton(menu, menuButtonLabel))
            {
                Time.timeScale = 1f;
                string menuScene = core != null ? core.CampaignMenuSceneName : "CampaignMenu";
                CampaignSceneLoadUtility.TryLoadSceneByName(menuScene, "결과 화면에서 메뉴 복귀");
            }

            ImGuiGameUi.EndScaledGui();
        }

        /// <summary>패배 시 재시도 힌트 — DialogueTable 키 (CampaignDialogue_KR)</summary>
        private static string GetDefeatRetryHintDialogueId(BattleAcesMatchController.MatchEndReason reason)
        {
            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "hint_retry_player_core",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "hint_retry_relic_objective",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "hint_retry_mission_fail",
                _ => "hint_retry_generic"
            };
        }

        /// <summary>미션 접미사 키(예: hint_retry_mission_fail_m03)가 있으면 우선, 없으면 기본 키</summary>
        private static string ResolveDefeatRetryHintText(
            PersistentGameCore core,
            MissionDefinition missionDef,
            BattleAcesMatchController.MatchEndReason reason)
        {
            string baseId = GetDefeatRetryHintDialogueId(reason);
            if (string.IsNullOrEmpty(baseId))
            {
                return null;
            }

            if (core == null)
            {
                return GetBuiltInDefeatRetryHint(reason);
            }

            string suffix = MissionDialogueHintIds.TryGetShortMissionSuffix(missionDef);
            if (!string.IsNullOrEmpty(suffix))
            {
                string combined = baseId + "_" + suffix;
                string fromMission = core.TryGetDialogue(combined);
                if (!string.IsNullOrEmpty(fromMission))
                {
                    return fromMission;
                }
            }

            string fromTable = core.TryGetDialogue(baseId);
            if (!string.IsNullOrEmpty(fromTable))
            {
                return fromTable;
            }

            return GetBuiltInDefeatRetryHint(reason);
        }

        /// <summary>대사 테이블에 힌트 키가 없을 때 한 줄 폴백</summary>
        private static string GetBuiltInDefeatRetryHint(BattleAcesMatchController.MatchEndReason reason)
        {
            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed =>
                    "재도전: 생산(1~8)·집결(Alt+우클릭)·자원·강화(T/Y/U)로 본진 방어를 보강해 보세요.",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost =>
                    "재도전: 목표 유물·거점을 먼저 확보하고 분산을 줄이세요.",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed =>
                    "재도전: 미션 목표(상단·F1)를 확인한 뒤 병력 운용을 바꿔 보세요.",
                _ =>
                    "재도전: 자원·생산·집결을 정비한 뒤 R 또는 아래 버튼으로 다시 시도하세요."
            };
        }

        private static string FormatMatchEndReasonLine(BattleAcesMatchController.MatchEndReason reason, bool won)
        {
            if (won)
            {
                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.VictoryEnemyCoreDestroyed => "종료 사유: 적 코어 파괴",
                    BattleAcesMatchController.MatchEndReason.VictoryMissionObjective => "종료 사유: 미션 목표 달성",
                    _ => "종료 사유: 작전 승리"
                };
            }

            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "종료 사유: 아군 코어 파괴",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "종료 사유: 핵심 목표 상실",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "종료 사유: 미션 실패",
                _ => "종료 사유: 작전 실패"
            };
        }

        private GUIStyle GetOrCreateResultNextStepHintStyle()
        {
            if (resultNextStepHintStyle != null && resultNextStepHintStyleSkin == GUI.skin)
            {
                return resultNextStepHintStyle;
            }

            resultNextStepHintStyleSkin = GUI.skin;
            resultNextStepHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            resultNextStepHintStyle.normal.textColor = ImGuiGameUi.TextMuted;
            return resultNextStepHintStyle;
        }

        private GUIStyle GetOrCreateResultBodyLabelStyle()
        {
            if (resultBodyLabelStyle != null && resultBodyLabelStyleSkin == GUI.skin)
            {
                return resultBodyLabelStyle;
            }

            resultBodyLabelStyleSkin = GUI.skin;
            // 한글 기본 폰트가 얇을 때 줄간격만 살짝 넓혀 본문만 읽기 쉽게 함
            resultBodyLabelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                fontSize = 17,
                alignment = TextAnchor.UpperLeft
            };
            resultBodyLabelStyle.normal.textColor = ImGuiGameUi.TextTitle;
            return resultBodyLabelStyle;
        }

        private static void DrawBriefingTopObjectiveBar(MissionDefinition m)
        {
            if (m == null)
            {
                return;
            }

            const float barH = 54f;
            ImGuiGameUi.DrawTopAccentBar(barH, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderAccent);

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 19;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(
                new Rect(22f, 14f, Screen.width - 44f, 36f),
                $"작전 목표 · {MissionObjectiveDisplayText.GetPrimaryLine(m)}");
            GUI.skin.label.fontSize = prevSize;
            GUI.color = Color.white;
        }
    }
}
