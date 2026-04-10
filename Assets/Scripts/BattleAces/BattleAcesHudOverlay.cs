using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.Settings;
using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    public sealed class BattleAcesHudOverlay : MonoBehaviour
    {
        private const float PanelPad = 14f;
        private const float CommandRejectHintSeconds = 1.05f;
        private const float RallyPointHintSeconds = 2.55f;
        private const float DeckRejectHintSeconds = 2.15f;
        private const float DeckOrderOkHintSeconds = 1.05f;
        private const float PlayerCoreHitHudHintSeconds = 1.38f;

        private static float commandRejectHintHideUnscaled = -999f;
        private static float rallyPointHintHideUnscaled = -999f;
        private static float deckRejectHintHideUnscaled = -999f;
        private static string deckRejectHintMessage = string.Empty;
        private static float deckOrderOkHintHideUnscaled = -999f;
        private static string deckOrderOkHintMessage = string.Empty;
        private static float playerCoreHitHudHideUnscaled = -999f;
        private static string playerCoreHitHudMessage = string.Empty;

        [SerializeField] private BattleAcesEconomy economy;
        [SerializeField] private BattleAcesCore playerCore;
        [SerializeField] private BattleAcesMatchController match;
        [SerializeField] private MissionDefinition missionContext;
        [SerializeField] private BattleMissionFlow missionFlow;
        [SerializeField] private BattleAcesObjectiveUgui uguiObjective;

        private bool IsKorean => GameUserSettings.Language == GameLanguage.Korean;

        public static void PulseCommandRejectTextHint()
        {
            commandRejectHintHideUnscaled = Time.unscaledTime + CommandRejectHintSeconds;
        }

        public static void PulseRallyPointSetHint()
        {
            rallyPointHintHideUnscaled = Time.unscaledTime + RallyPointHintSeconds;
            BattleAcesFirstPlayGuide.NotifyRallySet();
        }

        /// <summary>덱 생산·T/Y/U 강화 거절 등 경제/큐 거절 한 줄(한·영 문구는 호출부에서 결정)</summary>
        public static void PulseEconomyRejectHint(string hintLine)
        {
            deckRejectHintMessage = hintLine ?? string.Empty;
            deckRejectHintHideUnscaled = Time.unscaledTime + DeckRejectHintSeconds;
        }

        /// <summary>덱 주문 성공 — 거절 막대(금색)와 다른 청록 톤·짧은 문구</summary>
        public static void PulseDeckOrderSuccessHint(UnitArchetype archetype)
        {
            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            string unit = FormatArchetypeShortStatic(archetype, ko);
            deckOrderOkHintMessage = ko
                ? $"{unit} · 생산 대기열에 추가됨"
                : $"{unit} · queued for production";
            deckOrderOkHintHideUnscaled = Time.unscaledTime + DeckOrderOkHintSeconds;
        }

        /// <summary>지휘 코어 피격 — 유닛보다 무거운 HUD 한 줄(쿨다운은 CoreStructureHitSound 에서 묶음 처리)</summary>
        public static void PulsePlayerCoreHitHud()
        {
            bool ko = GameUserSettings.Language == GameLanguage.Korean;
            playerCoreHitHudMessage = ko
                ? "지휘 코어 피격 — 전열을 재정비하십시오"
                : "Command core under attack — regroup";
            playerCoreHitHudHideUnscaled = Time.unscaledTime + PlayerCoreHitHudHintSeconds;
        }

        public void Bind(
            BattleAcesEconomy eco,
            BattleAcesCore player,
            BattleAcesMatchController m,
            MissionDefinition mission,
            BattleMissionFlow flow = null,
            BattleAcesObjectiveUgui objectiveUgui = null)
        {
            economy = eco;
            playerCore = player;
            match = m;
            missionContext = mission;
            missionFlow = flow;
            uguiObjective = objectiveUgui;
        }

        private void OnGUI()
        {
            if (BattleAcesHudCaptureMode.SuppressCombatChromeForScreenshot)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();
            DrawPracticeRoundIntentToast();
            DrawLeftCommandPanel();

            if (match != null && match.IsFinished && missionContext == null)
            {
                bool deferForSting = missionFlow != null &&
                                     missionFlow.ResultCardRevealNotBeforeUnscaled > 0f &&
                                     Time.unscaledTime < missionFlow.ResultCardRevealNotBeforeUnscaled;
                if (!deferForSting)
                {
                    DrawSkirmishResultOverlay();
                }
            }

            DrawDeckOrderOkTransientHint();
            DrawRallyPointSetTransientHint();
            DrawCommandRejectTransientHint();
            DrawDeckRejectTransientHint();
            DrawPlayerCoreHitTransientHint();
            ImGuiGameUi.EndScaledGui();
        }

        private void DrawLeftCommandPanel()
        {
            float topReserve = GetImGuiTopReserve();
            float x = PanelPad;
            float y = topReserve + 10f;
            float width = Mathf.Min(438f, Screen.width * 0.29f);
            width = Mathf.Max(width, 356f);
            // 좁은 가로(720p 등)에서 화면 밖으로 밀리지 않게
            width = Mathf.Min(width, Screen.width - PanelPad * 2f - 8f);

            // 짧은 세로(720p 이하) — 하단·미니맵과 겹침 완화
            bool compactVertical = Screen.height <= 768;
            float shellH = compactVertical ? 304f : 332f;
            Rect shell = new Rect(x, y, width, shellH);
            ImGuiGameUi.DrawGlassPanel(shell, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
            ImGuiGameUi.DrawFilledRect(
                new Rect(shell.x, shell.y, shell.width, 26f),
                new Color(ImGuiGameUi.AccentCyan.r, ImGuiGameUi.AccentCyan.g, ImGuiGameUi.AccentCyan.b, 0.08f));
            // 헤더·본문 구분 — 티얼 한 방울만 섞어 한 줄이 읽힘
            ImGuiGameUi.DrawHorizontalRule(
                new Rect(shell.x + 16f, shell.y + 36f, shell.width - 32f, 1f),
                Color.Lerp(new Color(0.17f, 0.21f, 0.26f, 0.92f), ImGuiGameUi.AccentCyan, 0.12f));

            DrawOperationSummary(shell);
            DrawEconomyStrip(shell);
            DrawStatusChipRow(shell);
            DrawDeckGrid(shell);
            DrawProductionAndUpgradeLines(shell);
            DrawInputHints(shell);
            DrawTimeStatus(shell);
        }

        private void DrawOperationSummary(Rect shell)
        {
            float x = shell.x + 18f;
            float y = shell.y + 14f;
            float width = shell.width - 36f;

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x, y, width, 14f), DemoPresentationCopy.HudLeftPanelEyebrow);

            GUI.skin.label.fontSize = 18;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y + 14f, width, 22f), GetTopTitle());

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y + 40f, width, 18f), GetTopSubtitle());

            // 상단 UGUI 목표 줄이 있으면 같은 문구를 좌측에 다시 쓰지 않음 — 한 판 톤·가독성 통일
            bool objectiveOnTopBar =
                uguiObjective != null &&
                uguiObjective.HasObjectiveUi &&
                missionFlow != null &&
                missionFlow.IsGameplayStarted;

            string objective = missionContext != null
                ? MissionObjectiveDisplayText.GetPrimaryLine(missionContext)
                : DemoPresentationCopy.RoundGoalOneLineKo;
            GUI.skin.label.fontSize = 11;
            if (objectiveOnTopBar)
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(x, y + 62f, width, 32f), DemoPresentationCopy.LeftHudObjectiveFromTopBarKo);
            }
            else
            {
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(x, y + 62f, width, 32f), objective);
            }
        }

        private void DrawEconomyStrip(Rect shell)
        {
            if (economy == null)
            {
                return;
            }

            Rect strip = new Rect(shell.x + 18f, shell.y + 98f, shell.width - 36f, 44f);
            ImGuiGameUi.DrawPanelFrame(
                strip,
                ImGuiGameUi.EconomyStripPanelBg,
                ImGuiGameUi.EconomyStripBorder,
                1f);

            // 저해상에서 자원 숫자 한 단계 축소 — 패널 밀도만 완화
            int creditFont = Screen.height < 720 ? 24 : 27;
            GUI.skin.label.fontSize = creditFont;
            GUI.color = ImGuiGameUi.ResourceHighlight;
            GUI.Label(new Rect(strip.x + 10f, strip.y + 1f, 84f, 32f), $"{economy.PlayerCredits:0}");

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(strip.x + 92f, strip.y + 6f, 140f, 16f),
                IsKorean ? $"+{economy.PlayerTotalIncomePerSecond:0.#}/s 수입" : $"+{economy.PlayerTotalIncomePerSecond:0.#}/s income");
            GUI.Label(
                new Rect(strip.x + 92f, strip.y + 18f, strip.width - 102f, 16f),
                IsKorean ? $"적 전력 추정 {economy.EnemyCredits:0}" : $"Enemy reserve {economy.EnemyCredits:0}");

            float totalReserve = Mathf.Max(1f, economy.PlayerCredits + economy.EnemyCredits);
            float playerShare = economy.PlayerCredits / totalReserve;
            ImGuiGameUi.DrawProgressBar(
                new Rect(strip.x + 10f, strip.y + 30f, strip.width - 20f, 8f),
                playerShare,
                new Color(0.06f, 0.07f, 0.09f, 0.92f),
                ImGuiGameUi.AccentCyan,
                ImGuiGameUi.EconomyStripBorder);

            // 자원 스트립과 상태 칩 사이 시각적 구분(밀도만 올리고 정보는 동일)
            ImGuiGameUi.DrawHorizontalRule(
                new Rect(shell.x + 16f, shell.y + 146f, shell.width - 32f, 1f),
                new Color(0.17f, 0.21f, 0.26f, 0.5f));
        }

        private void DrawStatusChipRow(Rect shell)
        {
            if (playerCore == null)
            {
                return;
            }

            float x = shell.x + 18f;
            float y = shell.y + 150f;
            float width = shell.width - 36f;
            const float gap = 6f;
            float chipWidth = (width - gap * 3f) / 4f;

            DrawStatusChip(
                new Rect(x, y, chipWidth, 28f),
                IsKorean ? "큐" : "Queue",
                $"{playerCore.QueueCount}/14",
                ImGuiGameUi.AccentGold);
            DrawStatusChip(
                new Rect(x + (chipWidth + gap), y, chipWidth, 28f),
                "T",
                $"{playerCore.ProductionUpgradeTier + 1}/4",
                ImGuiGameUi.AccentCyan);
            DrawStatusChip(
                new Rect(x + (chipWidth + gap) * 2f, y, chipWidth, 28f),
                "Y",
                $"{playerCore.HullUpgradeTier + 1}/4",
                Color.Lerp(ImGuiGameUi.AccentCyan, Color.white, 0.16f));
            DrawStatusChip(
                new Rect(x + (chipWidth + gap) * 3f, y, chipWidth, 28f),
                "U",
                $"{playerCore.IncomeUpgradeTier + 1}/4",
                Color.Lerp(ImGuiGameUi.AccentGold, ImGuiGameUi.AccentCyan, 0.24f));
        }

        private static void DrawStatusChip(Rect rect, string eyebrow, string value, Color accent)
        {
            ImGuiGameUi.DrawHudCardWithLeftStripe(rect, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, accent, 2f);

            GUI.skin.label.fontSize = 9;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(rect.x + 8f, rect.y + 3f, rect.width - 16f, 10f), eyebrow);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(rect.x + 8f, rect.y + 12f, rect.width - 16f, 14f), value);
        }

        private void DrawDeckGrid(Rect shell)
        {
            if (playerCore == null)
            {
                return;
            }

            float x = shell.x + 18f;
            float y = shell.y + 188f;
            float width = shell.width - 36f;
            const float gap = 4f;
            float cellW = (width - gap * 3f) / 4f;
            const float cellH = 22f;

            bool hasPreview = playerCore.TryGetNextProductionPreview(out UnitArchetype nextArchetype, out _);

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y - 14f, width, 12f), IsKorean ? "생산 슬롯 1-8" : "Production slots 1-8");

            for (int i = 0; i < 8; i++)
            {
                int row = i / 4;
                int col = i % 4;
                Rect cell = new Rect(x + col * (cellW + gap), y + row * (cellH + gap), cellW, cellH);
                UnitArchetype slot = playerCore.GetDeckSlot(i);
                bool queued = hasPreview && slot == nextArchetype;
                ImGuiGameUi.DrawHudCardWithLeftStripe(
                    cell,
                    ImGuiGameUi.PanelBgHudCard,
                    queued ? ImGuiGameUi.AccentCyan : ImGuiGameUi.BorderCool,
                    queued ? ImGuiGameUi.AccentGold : ImGuiGameUi.AccentCyan,
                    queued ? 3f : 2f);
                GUI.color = ImGuiGameUi.AccentGold;
                GUI.Label(new Rect(cell.x + 4f, cell.y + 1f, 14f, cellH), BattleAcesCore.GetDeckHotkeyLabel(i));
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(cell.x + 18f, cell.y + 1f, cell.width - 22f, cellH), GetShortName(slot));
            }
        }

        private void DrawProductionAndUpgradeLines(Rect shell)
        {
            if (playerCore == null)
            {
                return;
            }

            float x = shell.x + 18f;
            float width = shell.width - 36f;
            float y = shell.y + 252f;

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y, width, 18f), BuildProductionLine());

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y + 18f, width, 16f), BuildUpgradeLine());

            float queueRatio = playerCore != null ? Mathf.Clamp01(playerCore.QueueCount / 14f) : 0f;
            ImGuiGameUi.DrawProgressBar(
                new Rect(x, y + 40f, width, 10f),
                queueRatio,
                new Color(0.06f, 0.07f, 0.09f, 0.94f),
                ImGuiGameUi.AccentGold,
                ImGuiGameUi.BorderCool);
        }

        private void DrawInputHints(Rect shell)
        {
            float x = shell.x + 18f;
            float width = shell.width - 36f;
            float y = shell.y + shell.height - 42f;

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y, width, 16f), GetInputHintLine());
        }

        private void DrawTimeStatus(Rect shell)
        {
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc == null || match == null || match.IsFinished || (missionFlow != null && !missionFlow.IsGameplayStarted))
            {
                return;
            }

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(shell.x + 18f, shell.y + shell.height - 22f, shell.width - 36f, 14f), rtc.GetHudTimeStatusLine());
        }

        private void DrawSkirmishResultOverlay()
        {
            bool victory = match != null && match.State == BattleAcesMatchController.MatchState.Victory;
            DemoPresentationCopy.GetSkirmishResultHeadline(victory, out string headline);

            float playSec = missionFlow != null
                ? missionFlow.LastMatchPlaySecondsUnscaled
                : (BattleAcesRunStats.Instance != null
                    ? BattleAcesRunStats.Instance.GetFrozenPlaySecondsUnscaled()
                    : 0f);
            int credits = economy != null ? Mathf.RoundToInt(economy.PlayerCredits) : 0;
            string statLine = BattleAcesRunStats.Instance != null
                ? BattleAcesRunStats.Instance.BuildFullResultSummaryLine(playSec, credits)
                : $"{BattleAcesRunStats.FormatPlayTimeMmSs(playSec)} · {credits}";

            float boxW = Mathf.Min(520f, Screen.width - 48f);
            Rect box = new Rect((Screen.width - boxW) * 0.5f, Screen.height * 0.34f, boxW, 152f);
            ImGuiGameUi.DrawGlassPanel(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, ImGuiGameUi.AccentGold);

            GUI.skin.label.fontSize = 22;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 20f, box.y + 16f, box.width - 40f, 28f), headline);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(box.x + 20f, box.y + 50f, box.width - 40f, 20f), statLine);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(
                new Rect(box.x + 20f, box.y + 78f, box.width - 40f, 18f),
                IsKorean ? "R 로 즉시 재시작" : "Press R to restart");

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(box.x + 20f, box.y + 100f, box.width - 40f, 34f),
                DemoPresentationCopy.AfterMatchExitOneLineKo);
        }

        private void DrawPracticeRoundIntentToast()
        {
            if (match == null || match.IsFinished || missionFlow == null || !missionFlow.IsGameplayStarted || missionContext == null || !missionContext.IsPracticeStyleOneMatch)
            {
                return;
            }

            float elapsed = Time.unscaledTime - missionFlow.GameplayStartUnscaledTime;
            if (elapsed < 0f || elapsed > 30f)
            {
                return;
            }

            float w = Mathf.Min(600f, Screen.width - 32f);
            float top = GetImGuiTopReserve() + 2f;
            Rect bar = new Rect((Screen.width - w) * 0.5f, top, w, 28f);
            ImGuiGameUi.DrawGlassPanel(bar, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(bar.x + 12f, bar.y + 5f, bar.width - 24f, 18f), DemoPresentationCopy.PracticeToastLineKo);
        }

        private float GetImGuiTopReserve()
        {
            if (uguiObjective != null && uguiObjective.HasObjectiveUi)
            {
                return Mathf.Max(PanelPad + 4f, uguiObjective.TopReservePixels);
            }

            return PanelPad + 4f;
        }

        private string GetTopTitle()
        {
            if (missionContext != null)
            {
                return BattleAcesObjectiveUgui.FormatTopBarDemoTitle(missionContext);
            }

            return DemoPresentationCopy.HudSkirmishFallbackTitle;
        }

        private string GetTopSubtitle()
        {
            if (missionContext != null)
            {
                return MissionObjectiveDisplayText.GetGameplayHint(missionContext);
            }

            return DemoPresentationCopy.HudSkirmishFallbackSubtitle;
        }

        private string BuildProductionLine()
        {
            if (match != null && match.IsFinished)
            {
                return string.Empty;
            }

            if (playerCore != null && playerCore.TryGetNextProductionPreview(out UnitArchetype arch, out float secLeft))
            {
                string unitShort = GetShortName(arch);
                int waiting = playerCore.QueuedProductionCount;
                if (IsKorean)
                {
                    return secLeft > 0.05f
                        ? $"생산 {unitShort} {secLeft:0.0}s · 대기 {waiting} · 큐 {playerCore.QueueCount}"
                        : $"대기 {unitShort} · 대기 {waiting} · 큐 {playerCore.QueueCount}";
                }

                return secLeft > 0.05f
                    ? $"Building {unitShort} {secLeft:0.0}s · waiting {waiting} · queue {playerCore.QueueCount}"
                    : $"Queued {unitShort} · waiting {waiting} · queue {playerCore.QueueCount}";
            }

            return IsKorean ? "생산 대기 중 · 슬롯 1-8로 호출" : "Production idle · trigger with slots 1-8";
        }

        private string BuildUpgradeLine()
        {
            if (playerCore == null)
            {
                return string.Empty;
            }

            string t = playerCore.ProductionUpgradeTier >= 3
                ? "T MAX"
                : playerCore.TryGetNextProductionUpgradeCost(out int cT) ? $"T {cT}" : "T --";
            string y = playerCore.HullUpgradeTier >= 3
                ? "Y MAX"
                : playerCore.TryGetNextHullUpgradeCost(out int cY) ? $"Y {cY}" : "Y --";
            string u = playerCore.IncomeUpgradeTier >= 3
                ? "U MAX"
                : playerCore.TryGetNextIncomeUpgradeCost(out int cU) ? $"U {cU}" : "U --";

            return IsKorean
                ? $"강화 {t}  {y}  {u} · Alt+우클릭 집결"
                : $"Upgrades {t}  {y}  {u} · Alt+Right Click rally";
        }

        private string GetInputHintLine()
        {
            return DemoPresentationCopy.HudCombatInputHintOneLine;
        }

        private static void DrawRallyPointSetTransientHint()
        {
            if (Time.unscaledTime >= rallyPointHintHideUnscaled)
            {
                return;
            }

            bool korean = GameUserSettings.Language == GameLanguage.Korean;
            string msg = korean
                ? "집결 지점 설정 완료 · 이후 생산 유닛이 바로 이동합니다"
                : "Rally point set · new units will route there";
            DrawTransientBar(Screen.height - 92f, 560f, ImGuiGameUi.AccentCyan, msg);
        }

        private static void DrawCommandRejectTransientHint()
        {
            if (Time.unscaledTime >= commandRejectHintHideUnscaled)
            {
                return;
            }

            bool korean = GameUserSettings.Language == GameLanguage.Korean;
            string msg = korean
                ? "명령 불가 · 유효한 지면이나 대상을 확인하세요"
                : "Command rejected · check the target or ground";
            DrawTransientBar(Screen.height - 58f, 500f, ImGuiGameUi.DefeatTint, msg);
        }

        private static void DrawDeckRejectTransientHint()
        {
            if (Time.unscaledTime >= deckRejectHintHideUnscaled || string.IsNullOrEmpty(deckRejectHintMessage))
            {
                return;
            }

            DrawTransientBar(Screen.height - 126f, 580f, ImGuiGameUi.AccentGold, deckRejectHintMessage);
        }

        private static void DrawDeckOrderOkTransientHint()
        {
            if (Time.unscaledTime >= deckOrderOkHintHideUnscaled || string.IsNullOrEmpty(deckOrderOkHintMessage))
            {
                return;
            }

            DrawTransientBar(Screen.height - 158f, 540f, ImGuiGameUi.AccentCyan, deckOrderOkHintMessage);
        }

        private static void DrawPlayerCoreHitTransientHint()
        {
            if (Time.unscaledTime >= playerCoreHitHudHideUnscaled || string.IsNullOrEmpty(playerCoreHitHudMessage))
            {
                return;
            }

            // 코어 피격은 명령 거절보다 위(나중에 그려서 최상단)
            Color coreHitAccent = BattleAcesArtDirection.EnemyEmber;
            DrawTransientBar(Screen.height - 48f, 620f, coreHitAccent, playerCoreHitHudMessage);
        }

        private static void DrawTransientBar(float y, float width, Color accent, string msg)
        {
            // 좁은 창·노치 대비 좌우 여유(울트라와이드는 중앙 정렬 유지)
            float barW = Mathf.Min(width, Screen.width - 48f);
            Rect bar = new Rect((Screen.width - barW) * 0.5f, y, barW, 28f);
            ImGuiGameUi.DrawGlassPanel(bar, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, accent);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(bar.x + 12f, bar.y + 5f, bar.width - 24f, 18f), msg);
        }

        private string GetShortName(UnitArchetype archetype)
        {
            return FormatArchetypeShortStatic(archetype, IsKorean);
        }

        private static string FormatArchetypeShortStatic(UnitArchetype archetype, bool korean)
        {
            if (korean)
            {
                return archetype switch
                {
                    UnitArchetype.Spearman => "창",
                    UnitArchetype.ShieldInfantry => "방패",
                    UnitArchetype.Rifleman => "소총",
                    UnitArchetype.Artillery => "포",
                    UnitArchetype.Fighter => "전투기",
                    UnitArchetype.SpecialWarrior => "특전",
                    UnitArchetype.RoyalGuard => "근위",
                    UnitArchetype.Outrider => "호버",
                    UnitArchetype.MobileFortress => "요새",
                    UnitArchetype.AirborneCitadel => "공성",
                    _ => archetype.ToString()
                };
            }

            return archetype switch
            {
                UnitArchetype.Spearman => "Spear",
                UnitArchetype.ShieldInfantry => "Shield",
                UnitArchetype.Rifleman => "Rifle",
                UnitArchetype.Artillery => "Artillery",
                UnitArchetype.Fighter => "Fighter",
                UnitArchetype.SpecialWarrior => "Special",
                UnitArchetype.RoyalGuard => "Royal Guard",
                UnitArchetype.Outrider => "Hover",
                UnitArchetype.MobileFortress => "Fortress",
                UnitArchetype.AirborneCitadel => "Citadel",
                _ => archetype.ToString()
            };
        }
    }
}
