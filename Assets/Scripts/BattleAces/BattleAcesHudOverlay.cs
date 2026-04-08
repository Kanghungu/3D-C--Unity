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

        private static float commandRejectHintHideUnscaled = -999f;
        private static float rallyPointHintHideUnscaled = -999f;
        private static float deckRejectHintHideUnscaled = -999f;
        private static string deckRejectHintMessage = string.Empty;

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

        public static void PulseDeckRejectHint(string messageKo)
        {
            deckRejectHintMessage = messageKo ?? string.Empty;
            deckRejectHintHideUnscaled = Time.unscaledTime + DeckRejectHintSeconds;
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
            ImGuiGameUi.BeginScaledGui();
            DrawPracticeRoundIntentToast();
            DrawLeftCommandPanel();

            if (match != null && match.IsFinished && missionContext == null)
            {
                DrawSkirmishResultOverlay();
            }

            DrawRallyPointSetTransientHint();
            DrawCommandRejectTransientHint();
            DrawDeckRejectTransientHint();
            ImGuiGameUi.EndScaledGui();
        }

        private void DrawLeftCommandPanel()
        {
            float topReserve = GetImGuiTopReserve();
            float x = PanelPad;
            float y = topReserve + 10f;
            float width = Mathf.Min(438f, Screen.width * 0.29f);
            width = Mathf.Max(width, 356f);

            Rect shell = new Rect(x, y, width, 276f);
            ImGuiGameUi.DrawGlassPanel(shell, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
            ImGuiGameUi.DrawFilledRect(
                new Rect(shell.x, shell.y, shell.width, 26f),
                new Color(ImGuiGameUi.AccentCyan.r, ImGuiGameUi.AccentCyan.g, ImGuiGameUi.AccentCyan.b, 0.08f));
            ImGuiGameUi.DrawHorizontalRule(
                new Rect(shell.x + 16f, shell.y + 36f, shell.width - 32f, 1f),
                new Color(0.17f, 0.21f, 0.26f, 0.9f));

            DrawOperationSummary(shell);
            DrawEconomyStrip(shell);
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
            GUI.Label(new Rect(x, y, width, 14f), "TACTICAL STATUS");

            GUI.skin.label.fontSize = 18;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y + 14f, width, 22f), GetTopTitle());

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y + 40f, width, 18f), GetTopSubtitle());

            string objective = missionContext != null
                ? MissionObjectiveDisplayText.GetPrimaryLine(missionContext)
                : DemoPresentationCopy.RoundGoalOneLineKo;
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y + 62f, width, 32f), objective);
        }

        private void DrawEconomyStrip(Rect shell)
        {
            if (economy == null)
            {
                return;
            }

            Rect strip = new Rect(shell.x + 18f, shell.y + 98f, shell.width - 36f, 36f);
            ImGuiGameUi.DrawPanelFrame(
                strip,
                new Color(0.075f, 0.082f, 0.095f, 0.94f),
                new Color(0.2f, 0.23f, 0.3f, 0.7f),
                1f);

            GUI.skin.label.fontSize = 27;
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
        }

        private void DrawDeckGrid(Rect shell)
        {
            if (playerCore == null)
            {
                return;
            }

            float x = shell.x + 18f;
            float y = shell.y + 148f;
            float width = shell.width - 36f;
            const float gap = 4f;
            float cellW = (width - gap * 3f) / 4f;
            const float cellH = 22f;

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y - 14f, width, 12f), IsKorean ? "생산 슬롯 1-8" : "Production slots 1-8");

            for (int i = 0; i < 8; i++)
            {
                int row = i / 4;
                int col = i % 4;
                Rect cell = new Rect(x + col * (cellW + gap), y + row * (cellH + gap), cellW, cellH);
                ImGuiGameUi.DrawPanelFrame(cell, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, 1f);
                GUI.color = ImGuiGameUi.AccentGold;
                GUI.Label(new Rect(cell.x + 4f, cell.y + 1f, 14f, cellH), BattleAcesCore.GetDeckHotkeyLabel(i));
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(cell.x + 18f, cell.y + 1f, cell.width - 22f, cellH), GetShortName(playerCore.GetDeckSlot(i)));
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
            float y = shell.y + 212f;

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(x, y, width, 18f), BuildProductionLine());

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(x, y + 18f, width, 16f), BuildUpgradeLine());
        }

        private void DrawInputHints(Rect shell)
        {
            float x = shell.x + 18f;
            float width = shell.width - 36f;
            float y = shell.y + shell.height - 38f;

            GUI.skin.label.fontSize = 10;
            GUI.color = new Color(0.56f, 0.62f, 0.7f, 1f);
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
            GUI.Label(new Rect(shell.x + 18f, shell.y + shell.height - 20f, shell.width - 36f, 14f), rtc.GetStatusLineKo());
        }

        private void DrawSkirmishResultOverlay()
        {
            string headline = match != null && match.State == BattleAcesMatchController.MatchState.Victory
                ? (IsKorean ? "승리 · 적 코어 파괴" : "Victory · Enemy core destroyed")
                : (IsKorean ? "패배 · 아군 코어 붕괴" : "Defeat · Command core lost");

            float playSec = missionFlow != null
                ? missionFlow.LastMatchPlaySecondsUnscaled
                : (BattleAcesRunStats.Instance != null
                    ? BattleAcesRunStats.Instance.GetFrozenPlaySecondsUnscaled()
                    : 0f);
            int credits = economy != null ? Mathf.RoundToInt(economy.PlayerCredits) : 0;
            string statLine = BattleAcesRunStats.Instance != null
                ? BattleAcesRunStats.Instance.BuildFullResultSummaryLine(playSec, credits)
                : $"{BattleAcesRunStats.FormatPlayTimeMmSs(playSec)} · {credits}";

            float boxW = Mathf.Min(520f, Screen.width - 32f);
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

            return IsKorean ? "스커미시 전술 네트워크" : "Skirmish Tactical Network";
        }

        private string GetTopSubtitle()
        {
            if (missionContext != null)
            {
                return MissionObjectiveDisplayText.GetGameplayHint(missionContext);
            }

            return IsKorean
                ? "전장 운영, 생산, 재배치를 한 패널에서 확인"
                : "Track command flow, production, and redeployment from one panel";
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
            return IsKorean
                ? "1-8 생산 · WASD 이동 · 우클릭 명령 · Ctrl+A 전체 선택 · F1 도움말"
                : "1-8 build · WASD move · Right Click command · Ctrl+A select all · F1 help";
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

        private static void DrawTransientBar(float y, float width, Color accent, string msg)
        {
            float barW = Mathf.Min(width, Screen.width - 32f);
            Rect bar = new Rect((Screen.width - barW) * 0.5f, y, barW, 28f);
            ImGuiGameUi.DrawGlassPanel(bar, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, accent);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(bar.x + 12f, bar.y + 5f, bar.width - 24f, 18f), msg);
        }

        private string GetShortName(UnitArchetype archetype)
        {
            if (IsKorean)
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
                    UnitArchetype.Outrider => "기동",
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
                UnitArchetype.Outrider => "Outrider",
                UnitArchetype.MobileFortress => "Fortress",
                UnitArchetype.AirborneCitadel => "Citadel",
                _ => archetype.ToString()
            };
        }
    }
}
