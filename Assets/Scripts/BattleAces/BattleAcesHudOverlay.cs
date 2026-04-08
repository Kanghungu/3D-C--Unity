using Game.Campaign.Data;
using Game.Campaign.Scene;
using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    public class BattleAcesHudOverlay : MonoBehaviour
    {
        private const float PanelPad = 14f;

        /// <summary>우클릭 명령 불가 시 짧게 띄우는 안내(과도한 스팸 방지는 Selection 쪽 쿨다운)</summary>
        private const float CommandRejectHintSeconds = 1.05f;

        private static float commandRejectHintHideUnscaled = -999f;

        /// <summary>Alt+우클릭으로 랠리를 잡았을 때 하단 짧은 확인(튜토리얼 없이 도달 가능성 강화)</summary>
        private const float RallyPointHintSeconds = 2.55f;

        private static float rallyPointHintHideUnscaled = -999f;

        /// <summary>덱 생산·T/Y/U 강화 실패 시 하단 한 줄(자원·큐·상한 등 구분)</summary>
        private const float DeckRejectHintSeconds = 2.15f;

        private static float deckRejectHintHideUnscaled = -999f;

        private static string deckRejectHintMessage = string.Empty;

        public static void PulseCommandRejectTextHint()
        {
            commandRejectHintHideUnscaled = Time.unscaledTime + CommandRejectHintSeconds;
        }

        public static void PulseRallyPointSetHint()
        {
            rallyPointHintHideUnscaled = Time.unscaledTime + RallyPointHintSeconds;
            BattleAcesFirstPlayGuide.NotifyRallySet();
        }

        /// <summary>생산·강화 거절 시 HUD 메시지(한국어 한 줄)</summary>
        public static void PulseDeckRejectHint(string messageKo)
        {
            deckRejectHintMessage = messageKo ?? string.Empty;
            deckRejectHintHideUnscaled = Time.unscaledTime + DeckRejectHintSeconds;
        }

        [SerializeField] private BattleAcesEconomy economy;
        [SerializeField] private BattleAcesCore playerCore;
        [SerializeField] private BattleAcesMatchController match;
        [SerializeField] private MissionDefinition missionContext;
        [SerializeField] private BattleMissionFlow missionFlow;
        [SerializeField] private BattleAcesObjectiveUgui uguiObjective;

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
            float x0 = PanelPad;
            float topReserve = GetImGuiTopReserve();
            float innerLeft = x0 + 10f;
            float y = topReserve + 8f;
            float maxW = Mathf.Min(400f, Screen.width - PanelPad * 2f);

            bool showMissionBlock = missionContext != null &&
                                    (missionFlow == null || missionFlow.IsGameplayStarted);
            bool drawImGuiMission = showMissionBlock &&
                                    (uguiObjective == null || !uguiObjective.HasObjectiveUi);

            float panelH = EstimatePanelHeight(showMissionBlock, drawImGuiMission);
            float panelTop = Mathf.Max(PanelPad - 2f, topReserve - 4f);
            Rect fullPanel = new Rect(x0 - 8f, panelTop, maxW + 16f, panelH);
            ImGuiGameUi.DrawHudCardWithLeftStripe(
                fullPanel,
                ImGuiGameUi.PanelBgHud,
                ImGuiGameUi.BorderCool,
                ImGuiGameUi.HudStripeTactical,
                3f);

            float iw = maxW - 4f;

            if (drawImGuiMission)
            {
                DrawMissionHeaderBlockCompact(ref y, innerLeft, iw);
            }
            else if (missionContext == null)
            {
                GUI.skin.label.fontSize = 12;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(innerLeft, y, iw, 40f),
                    DemoPresentationCopy.RoundGoalOneLineKo + "\n" + DemoPresentationCopy.AfterMatchExitOneLineKo);
                y += 38f;
            }

            ImGuiGameUi.DrawHorizontalRule(new Rect(innerLeft, y, iw, 1f), new Color(0.18f, 0.2f, 0.26f, 0.55f));
            y += 8f;

            if (economy != null)
            {
                GUI.skin.label.fontSize = 22;
                GUI.color = ImGuiGameUi.ResourceHighlight;
                GUI.Label(new Rect(innerLeft, y, 96f, 28f), $"{economy.PlayerCredits:0}");
                GUI.skin.label.fontSize = 11;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(innerLeft + 100f, y + 6f, iw - 100f, 20f),
                    $"+{economy.PlayerTotalIncomePerSecond:0.#}/s   적 동원 {economy.EnemyCredits:0}");
                y += 30f;
            }

            if (playerCore != null)
            {
                GUI.skin.label.fontSize = 10;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(innerLeft, y, iw, 14f), "덱 1–8");
                y += 14f;
                DrawDeckSlotGrid(innerLeft, y, iw, playerCore);
                y += 50f;

                DrawProductionLineCompact(ref y, innerLeft, iw);
                DrawRallyAndUpgradesMergedLine(ref y, innerLeft, iw);

                GUI.skin.label.fontSize = 10;
                GUI.color = new Color(0.45f, 0.5f, 0.58f, 1f);
                GUI.Label(
                    new Rect(innerLeft, y, iw, 28f),
                    "덱1~8  WASD·가장자리  ,랠리  Ctrl+A  Esc  TYU  V  F1  P  [ ]");
                y += 28f;

                RtsTimeControl rtc = RtsTimeControl.Instance;
                if (rtc != null && match != null && !match.IsFinished &&
                    (missionFlow == null || missionFlow.IsGameplayStarted))
                {
                    string line = rtc.GetStatusLineKo();
                    if (!string.IsNullOrEmpty(line))
                    {
                        GUI.skin.label.fontSize = 10;
                        GUI.color = ImGuiGameUi.TextMuted;
                        GUI.Label(new Rect(innerLeft, y, iw, 16f), line);
                    }
                }
            }

            GUI.color = Color.white;

            if (match != null && match.IsFinished && missionContext == null)
            {
                DrawSkirmishResultOverlay();
            }

            DrawRallyPointSetTransientHint();
            DrawCommandRejectTransientHint();
            DrawDeckRejectTransientHint();

            ImGuiGameUi.EndScaledGui();
        }

        private void DrawProductionLineCompact(ref float y, float innerLeft, float iw)
        {
            if (playerCore == null || match == null || match.IsFinished)
            {
                return;
            }

            string body;
            if (playerCore.TryGetNextProductionPreview(out UnitArchetype arch, out float secLeft))
            {
                string unitShort = GetShortName(arch);
                int waiting = playerCore.QueuedProductionCount;
                if (secLeft > 0.05f)
                {
                    body = $"생산 {unitShort} {secLeft:0.0}s · 대기{waiting} · 합{playerCore.QueueCount}";
                }
                else
                {
                    body = $"대기 {unitShort} · 큐{waiting} · 합{playerCore.QueueCount}";
                }
            }
            else
            {
                body = "생산 대기 — 1~8";
            }

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(innerLeft, y, iw, 18f), body);
            y += 20f;
        }

        private void DrawRallyAndUpgradesMergedLine(ref float y, float innerLeft, float iw)
        {
            if (playerCore == null || match == null || match.IsFinished)
            {
                return;
            }

            string t = playerCore.ProductionUpgradeTier >= 3
                ? "T✓"
                : playerCore.TryGetNextProductionUpgradeCost(out int cT)
                    ? $"T{cT}"
                    : "T—";
            string hull = playerCore.HullUpgradeTier >= 3
                ? "Y✓"
                : playerCore.TryGetNextHullUpgradeCost(out int cY)
                    ? $"Y{cY}"
                    : "Y—";
            string inc = playerCore.IncomeUpgradeTier >= 3
                ? "U✓"
                : playerCore.TryGetNextIncomeUpgradeCost(out int cU)
                    ? $"U{cU}"
                    : "U—";

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(innerLeft, y, iw, 28f),
                $"강화 {t} {hull} {inc}  ·  랠리 Alt+우클릭 지면");
            y += 28f;
        }

        private static void DrawRallyPointSetTransientHint()
        {
            if (Time.unscaledTime >= rallyPointHintHideUnscaled)
            {
                return;
            }

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 13;
            const string msg = "집결 지점 설정 · 이후 생산 유닛이 링으로 집결 (F1)";
            float w = Mathf.Min(520f, Screen.width - 28f);
            Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 88f, w, 26f);
            ImGuiGameUi.DrawPanelFrame(bar, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.HudStripeTactical, 1f);
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(bar.x + 10f, bar.y + 6f, bar.width - 20f, 22f), msg);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = prevSize;
        }

        private static void DrawDeckSlotGrid(float ix, float iy, float iw, BattleAcesCore core)
        {
            const float gap = 4f;
            float cellW = (iw - gap * 3f) / 4f;
            const float cellH = 20f;

            for (int i = 0; i < 8; i++)
            {
                int row = i / 4;
                int col = i % 4;
                Rect cell = new Rect(ix + col * (cellW + gap), iy + row * (cellH + gap), cellW, cellH);
                ImGuiGameUi.DrawPanelFrame(cell, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, 0.5f);
                string key = BattleAcesCore.GetDeckHotkeyLabel(i);
                string name = GetShortName(core.GetDeckSlot(i));
                GUI.skin.label.fontSize = 10;
                GUI.color = ImGuiGameUi.AccentGold;
                GUI.Label(new Rect(cell.x + 3f, cell.y + 1f, 14f, cellH), key);
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(cell.x + 17f, cell.y + 1f, cell.width - 20f, cellH), name);
            }
        }

        private static void DrawCommandRejectTransientHint()
        {
            if (Time.unscaledTime >= commandRejectHintHideUnscaled)
            {
                return;
            }

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.DefeatTint;
            const string msg = "명령 불가 — 지면·표적을 확인";
            float w = Mathf.Min(480f, Screen.width - 32f);
            Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 56f, w, 24f);
            ImGuiGameUi.DrawPanelFrame(bar, ImGuiGameUi.PanelBgDeep, new Color(0.55f, 0.32f, 0.3f, 0.75f), 1f);
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(bar.x + 10f, bar.y + 4f, bar.width - 20f, 22f), msg);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = prevSize;
        }

        private static void DrawDeckRejectTransientHint()
        {
            if (Time.unscaledTime >= deckRejectHintHideUnscaled || string.IsNullOrEmpty(deckRejectHintMessage))
            {
                return;
            }

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 13;
            float w = Mathf.Min(560f, Screen.width - 28f);
            Rect bar = new Rect((Screen.width - w) * 0.5f, Screen.height - 120f, w, 26f);
            ImGuiGameUi.DrawPanelFrame(bar, ImGuiGameUi.PanelBgDeep, new Color(0.38f, 0.42f, 0.52f, 0.82f), 1f);
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(bar.x + 10f, bar.y + 5f, bar.width - 20f, 22f), deckRejectHintMessage);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = prevSize;
        }

        private static string GetShortName(Game.Units.UnitArchetype archetype)
        {
            return archetype switch
            {
                Game.Units.UnitArchetype.Spearman => "창",
                Game.Units.UnitArchetype.ShieldInfantry => "방패",
                Game.Units.UnitArchetype.Rifleman => "소총",
                Game.Units.UnitArchetype.Artillery => "포",
                Game.Units.UnitArchetype.Fighter => "전투기",
                Game.Units.UnitArchetype.SpecialWarrior => "특전",
                Game.Units.UnitArchetype.RoyalGuard => "근위",
                Game.Units.UnitArchetype.Outrider => "기동",
                Game.Units.UnitArchetype.MobileFortress => "요새",
                Game.Units.UnitArchetype.AirborneCitadel => "공성",
                _ => archetype.ToString()
            };
        }

        private void DrawSkirmishResultOverlay()
        {
            if (match == null)
            {
                return;
            }

            string msg = match.State == BattleAcesMatchController.MatchState.Victory
                ? "승리 — 적 코어 격파"
                : "패배 — 아군 코어 붕괴";

            float boxW = Mathf.Min(520f, Screen.width - 32f);
            Rect box = new Rect((Screen.width - boxW) * 0.5f, Screen.height * 0.36f, boxW, 152f);
            ImGuiGameUi.DrawHudCardWithLeftStripe(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, ImGuiGameUi.HudStripeTactical, 3f);

            GUI.skin.label.fontSize = 22;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 20f, box.y + 16f, box.width - 40f, 32f), msg);
            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            float playSec = missionFlow != null
                ? missionFlow.LastMatchPlaySecondsUnscaled
                : (BattleAcesRunStats.Instance != null
                    ? BattleAcesRunStats.Instance.GetFrozenPlaySecondsUnscaled()
                    : 0f);
            int credits = economy != null ? Mathf.RoundToInt(economy.PlayerCredits) : 0;
            string statLine = BattleAcesRunStats.Instance != null
                ? BattleAcesRunStats.Instance.BuildFullResultSummaryLine(playSec, credits)
                : $"플레이 {BattleAcesRunStats.FormatPlayTimeMmSs(playSec)} · 종료 시 자원 {credits}";
            GUI.Label(new Rect(box.x + 20f, box.y + 48f, box.width - 40f, 40f), statLine);
            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 20f, box.y + 84f, box.width - 40f, 22f), "R 키 — 같은 판 즉시 재시작");
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(box.x + 20f, box.y + 104f, box.width - 40f, 44f), DemoPresentationCopy.AfterMatchExitOneLineKo);
            GUI.color = Color.white;
        }

        /// <summary>스커미시·에디터 폴백 데모 — 작전 시작 후 30초만 상단 한 줄(캠페인 첫 미션 체크리스트와 역할 분리)</summary>
        private void DrawPracticeRoundIntentToast()
        {
            if (match == null || match.IsFinished)
            {
                return;
            }

            if (missionFlow == null || !missionFlow.IsGameplayStarted)
            {
                return;
            }

            if (missionContext == null || !missionContext.IsPracticeStyleOneMatch)
            {
                return;
            }

            float elapsed = Time.unscaledTime - missionFlow.GameplayStartUnscaledTime;
            if (elapsed < 0f || elapsed > 30f)
            {
                return;
            }

            float w = Mathf.Min(620f, Screen.width - 20f);
            float top = GetImGuiTopReserve() + 2f;
            Rect bar = new Rect((Screen.width - w) * 0.5f, top, w, 28f);
            ImGuiGameUi.DrawPanelFrame(bar, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, 1f);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(
                new Rect(bar.x + 10f, bar.y + 6f, bar.width - 20f, 18f),
                DemoPresentationCopy.PracticeToastLineKo);
            GUI.color = Color.white;
        }

        private float GetImGuiTopReserve()
        {
            if (uguiObjective != null && uguiObjective.HasObjectiveUi)
            {
                return Mathf.Max(PanelPad + 4f, uguiObjective.TopReservePixels);
            }

            return PanelPad + 4f;
        }

        private float EstimatePanelHeight(bool showMissionBlock, bool drawImGuiMission)
        {
            float h = 20f;

            if (missionContext == null && !showMissionBlock)
            {
                h += 48f;
            }
            else if (drawImGuiMission)
            {
                bool longDefense = missionContext != null &&
                                   missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense;
                bool seize = missionContext != null &&
                             missionContext.ObjectiveKind == MissionObjectiveKind.SeizeRelicOrNode;
                if (longDefense)
                {
                    h += 92f;
                }
                else if (seize)
                {
                    h += 94f;
                }
                else
                {
                    h += 78f;
                }
            }
            else if (!drawImGuiMission && showMissionBlock)
            {
                h += 8f;
            }

            h += 12f;

            if (economy != null)
            {
                h += 34f;
            }

            if (playerCore != null)
            {
                h += 14f + 50f + 28f;
                if (match != null && !match.IsFinished)
                {
                    h += 20f + 28f;
                }

                h += 14f;
            }

            return Mathf.Max(h, 158f);
        }

        private void DrawMissionHeaderBlockCompact(ref float y, float ix, float iw)
        {
            string primary = MissionObjectiveDisplayText.GetPrimaryLine(missionContext);
            string hint = MissionObjectiveDisplayText.GetGameplayHint(missionContext);
            string extra = GetDefenseCountdownLine();
            if (string.IsNullOrEmpty(extra))
            {
                extra = GetSeizeProgressLineForImGui();
            }

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(ix, y, iw, 20f), BattleAcesObjectiveUgui.FormatTopBarDemoTitle(missionContext));
            y += 20f;

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(ix, y, iw, 22f), primary);
            y += 24f;

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            string description = string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}";
            if (!string.IsNullOrEmpty(description))
            {
                float hintH = missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense ? 34f
                    : missionContext.ObjectiveKind == MissionObjectiveKind.SeizeRelicOrNode ? 36f
                    : 26f;
                GUI.Label(new Rect(ix, y, iw, hintH), description);
                y += hintH + 2f;
            }
        }

        /// <summary>UGUI 상단 바와 비슷하게 — 점령 미션만 IMGUI 폴백에서 진행 한 줄 표시</summary>
        // 챕터5 온보딩: UGUI가 꺼진 경우에도 점령 진행률이 왼쪽 패널에 보이게 함
        private static string GetSeizeProgressLineForImGui()
        {
            MissionCaptureZone zone = MissionCaptureZone.Instance;
            if (zone == null)
            {
                return string.Empty;
            }

            int pct = Mathf.RoundToInt(zone.HoldProgress01 * 100f);
            if (zone.IsCompleted)
            {
                return "점령 완료 처리 중";
            }

            if (!zone.IsPlayerInside)
            {
                return $"점령 대기 · 진행 {pct}% / {zone.HoldSecondsRequired:0}초 유지 필요";
            }

            return $"점령 중 · 아군 {zone.OccupyingPlayerUnitCount}기 · {pct}%";
        }

        private string GetDefenseCountdownLine()
        {
            if (missionContext == null || missionContext.ObjectiveKind != MissionObjectiveKind.SanctuaryDefense)
            {
                return string.Empty;
            }

            if (missionFlow == null || !missionFlow.IsGameplayStarted)
            {
                return "작전이 시작되면 방어 타이머가 진행됩니다.";
            }

            float elapsed = Time.time - missionFlow.GameplayStartTime;
            float remain = Mathf.Max(0f, missionContext.DefenseDurationSeconds - elapsed);
            return $"남은 시간 {remain:0}초 / 목표 {missionContext.DefenseDurationSeconds:0}초 생존";
        }
    }
}
