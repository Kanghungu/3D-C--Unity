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

        private static bool IsKoreanLang => GameUserSettings.Language == GameLanguage.Korean;

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

        /// <summary>승패 스팅 직후 unscaled 시각까지는 결과 카드를 그리지 않음(청각→시각 순서 통일)</summary>
        private float resultCardRevealNotBeforeUnscaled = -1f;

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

        /// <summary>스커미시 IMGUI 결과 등, 미션 흐름과 동일한 «스팅 후 카드» 타이밍용</summary>
        public float ResultCardRevealNotBeforeUnscaled => resultCardRevealNotBeforeUnscaled;

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
            resultCardRevealNotBeforeUnscaled = -1f;
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

            // 한 판 톤: 카메라 임펄스 → 결과 스팅 레이어 → (unscaled) 짧은 간격 → 결과 카드
            bool victory = st == BattleAcesMatchController.MatchState.Victory;
            BattleAcesCombatJuice.NotifyMatchResult(victory);
            ProceduralAudioUtility.PlayResultSting(victory);
            resultCardRevealNotBeforeUnscaled = Time.unscaledTime + BattleAcesFeedbackTiming.ResultCardDelayAfterStingUnscaled;
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

            if (resultCardRevealNotBeforeUnscaled > 0f && Time.unscaledTime < resultCardRevealNotBeforeUnscaled)
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
                text = DemoPresentationCopy.BuildDefaultBriefingBodyKo(
                    MissionObjectiveDisplayText.ResolveMissionDisplayName(mission),
                    MissionObjectiveDisplayText.GetPrimaryLine(mission));
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
                DemoPresentationCopy.BriefingContinueFooterKo);
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
                body = skirmishPractice
                    ? DemoPresentationCopy.ResultBodyFallbackPractice(won, isFallbackOneMatchDemo)
                    : DemoPresentationCopy.ResultBodyFallbackCampaign(won);
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

            // 순서: 스팅(오디오) → 카드 → 본문 → 통계 → 다음 단계·입력 안내 → 버튼 (스팅은 OnMatchEnded 에서 먼저 재생됨)
            string title = DemoPresentationCopy.GetResultScreenTitle(won);
            GUI.skin.label.fontSize = 26;
            GUI.color = won ? ImGuiGameUi.VictoryTint : ImGuiGameUi.DefeatTint;
            GUI.Label(new Rect(card.x + padX, y, innerW, 40f), title);
            y += 44f;

            string endReasonLine = match != null
                ? FormatMatchEndReasonLine(match.LastEndReason, won)
                : string.Empty;
            if (!string.IsNullOrEmpty(endReasonLine))
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(card.x + padX, y, innerW, 24f), endReasonLine);
                GUI.color = Color.white;
                y += 28f;
            }

            if (!won && core != null && match != null)
            {
                string retryHint = ResolveDefeatRetryHintText(core, mission, match.LastEndReason);
                if (!string.IsNullOrEmpty(retryHint))
                {
                    GUI.skin.label.fontSize = 14;
                    GUI.color = ImGuiGameUi.AccentCyan;
                    GUI.Label(new Rect(card.x + padX, y, innerW, 40f), retryHint);
                    y += 44f;
                }
            }

            // 하단: 통계·보조목표·다음 단계·R/Esc·개발 메모·버튼
            float footerBlock = veryLowRes ? 292f : 304f;
            if (mission != null && !string.IsNullOrEmpty(mission.OptionalBonusObjectiveId))
            {
                footerBlock += 44f;
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
                : (IsKoreanLang
                    ? $"플레이 {BattleAcesRunStats.FormatPlayTimeMmSs(resultPlaySeconds)} · 종료 시 자원 {resultPlayerCredits}"
                    : $"Play {BattleAcesRunStats.FormatPlayTimeMmSs(resultPlaySeconds)} · credits at end {resultPlayerCredits}");
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

            string nextCampaignLine;
            if (skirmishPractice)
            {
                nextCampaignLine = DemoPresentationCopy.ResultNextStepPractice(isFallbackOneMatchDemo);
            }
            else
            {
                nextCampaignLine = core != null
                    ? core.TryGetDialogue(ResultCampaignNextHintDialogueId)
                    : null;
                if (string.IsNullOrEmpty(nextCampaignLine))
                {
                    nextCampaignLine = DemoPresentationCopy.ResultNextStepCampaignDefaultKo;
                }
            }

            GUIStyle nextStepStyle = GetOrCreateResultNextStepHintStyle();
            float nextStepH = nextStepStyle.CalcHeight(new GUIContent(nextCampaignLine), innerW);
            nextStepH = Mathf.Clamp(nextStepH, 22f, 72f);
            GUI.skin.label.fontSize = 13;
            GUI.Label(new Rect(card.x + padX, extraY, innerW, nextStepH), nextCampaignLine, nextStepStyle);
            extraY += nextStepH + 10f;

            GUI.skin.label.fontSize = 17;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(
                new Rect(card.x + padX, extraY, innerW, 28f),
                DemoPresentationCopy.ResultScreenRKeyLine(isFallbackOneMatchDemo, skirmishPractice));
            extraY += 28f;

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(card.x + padX, extraY, innerW, 26f),
                DemoPresentationCopy.ResultScreenEscLine(skirmishPractice));
            GUI.color = Color.white;
            extraY += 30f;

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + padX, extraY, innerW, 34f), DemoPresentationCopy.ResultScreenFooterDevNoteKo);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 14;

            float btnY = card.yMax - 72f;
            float btnW = (card.width - 64f) * 0.5f;
            Rect retry = new Rect(card.x + 24f, btnY, btnW - 8f, 48f);
            Rect menu = new Rect(card.x + 32f + btnW, btnY, btnW - 8f, 48f);

            string retryLabel = isFallbackOneMatchDemo
                ? (IsKoreanLang ? "같은 데모 재시작" : "Restart demo")
                : skirmishPractice
                    ? (IsKoreanLang ? "같은 스커미시 재시작" : "Restart skirmish")
                    : (IsKoreanLang ? "같은 미션 재시작" : "Restart mission");
            if (ImGuiGameUi.GameMenuButton(retry, retryLabel))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().path);
            }

            string menuButtonLabel = skirmishPractice
                ? (IsKoreanLang ? "메인 메뉴로" : "Main menu")
                : (IsKoreanLang ? "캠페인 메뉴로" : "Campaign menu");
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
            if (!IsKoreanLang)
            {
                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed =>
                        "Retry: keep production (1–8) and T/Y/U upgrades near your core; rally (Alt+right-click) before big fights.",
                    BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost =>
                        "Retry: hold the relic or key point first; shorten side engagements.",
                    BattleAcesMatchController.MatchEndReason.DefeatMissionFailed =>
                        "Retry: check the top bar / F1 objective, then adjust route and timing.",
                    _ =>
                        "Retry: reset economy, production, and rally, then press R or the button below."
                };
            }

            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed =>
                    "재도전: 지휘 코어 앞 생산(1~8)·T/Y/U 강화, 집결(Alt+우클릭)로 방어 리듬을 다시 맞추십시오.",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost =>
                    "재도전: 성유물·거점을 먼저 지키고, 측면 교전은 짧게 끊으십시오.",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed =>
                    "재도전: 상단 목표 바·F1의 주 목표를 확인한 뒤 경로·시간에 맞춰 접근을 바꾸십시오.",
                _ =>
                    "재도전: 자원·생산·집결을 한 번 정리한 뒤 R 또는 아래 버튼으로 이어가세요."
            };
        }

        private static string FormatMatchEndReasonLine(BattleAcesMatchController.MatchEndReason reason, bool won)
        {
            if (!IsKoreanLang)
            {
                if (won)
                {
                    return reason switch
                    {
                        BattleAcesMatchController.MatchEndReason.VictoryEnemyCoreDestroyed => "Outcome: enemy core destroyed",
                        BattleAcesMatchController.MatchEndReason.VictoryMissionObjective => "Outcome: mission objective complete",
                        _ => "Outcome: victory"
                    };
                }

                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "Outcome: command core destroyed",
                    BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "Outcome: key objective lost",
                    BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "Outcome: mission failed",
                    _ => "Outcome: defeat"
                };
            }

            if (won)
            {
                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.VictoryEnemyCoreDestroyed => "종료 사유: 적 코어 격파",
                    BattleAcesMatchController.MatchEndReason.VictoryMissionObjective => "종료 사유: 작전 목표 달성",
                    _ => "종료 사유: 작전 승리"
                };
            }

            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "종료 사유: 지휘 코어 붕괴",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "종료 사유: 핵심 목표 상실",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "종료 사유: 작전 실패",
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
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(
                new Rect(22f, 14f, Screen.width - 44f, 36f),
                $"{DemoPresentationCopy.BriefingTopBarPrefixKo} · {MissionObjectiveDisplayText.GetPrimaryLine(m)}");
            GUI.skin.label.fontSize = prevSize;
            GUI.color = Color.white;
        }
    }
}
