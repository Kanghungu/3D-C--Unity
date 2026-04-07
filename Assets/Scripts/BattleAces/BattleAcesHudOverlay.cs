using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    public class BattleAcesHudOverlay : MonoBehaviour
    {
        private const float PanelPad = 14f;

        /// <summary>우클릭 명령 불가 시 짧게 띄우는 안내(과도한 스팸 방지는 Selection 쪽 쿨다운)</summary>
        private const float CommandRejectHintSeconds = 1.05f;

        private static float commandRejectHintHideUnscaled = -999f;

        public static void PulseCommandRejectTextHint()
        {
            commandRejectHintHideUnscaled = Time.unscaledTime + CommandRejectHintSeconds;
        }

        [SerializeField] private BattleAcesEconomy economy;
        [SerializeField] private BattleAcesCore playerCore;
        [SerializeField] private BattleAcesMatchController match;
        [SerializeField] private MissionDefinition missionContext;
        [SerializeField] private CampaignBattleFlow campaignFlow;
        [SerializeField] private BattleAcesObjectiveUgui uguiObjective;

        public void Bind(
            BattleAcesEconomy eco,
            BattleAcesCore player,
            BattleAcesMatchController m,
            MissionDefinition mission,
            CampaignBattleFlow flow = null,
            BattleAcesObjectiveUgui objectiveUgui = null)
        {
            economy = eco;
            playerCore = player;
            match = m;
            missionContext = mission;
            campaignFlow = flow;
            uguiObjective = objectiveUgui;
        }

        private void OnGUI()
        {
            ImGuiGameUi.BeginScaledGui();
            float x0 = PanelPad;
            float topReserve = GetImGuiTopReserve();
            float y = topReserve;
            float maxW = Mathf.Min(760f, Screen.width - PanelPad * 2f);

            bool showMissionBlock = missionContext != null &&
                                    (campaignFlow == null || campaignFlow.IsGameplayStarted);
            bool drawImGuiMission = showMissionBlock &&
                                    (uguiObjective == null || !uguiObjective.HasObjectiveUi);

            float panelH = EstimatePanelHeight(showMissionBlock, drawImGuiMission);
            float panelTop = Mathf.Max(PanelPad - 4f, topReserve - 8f);
            ImGuiGameUi.DrawPanelFrame(
                new Rect(x0 - 6f, panelTop, maxW + 12f, panelH),
                ImGuiGameUi.PanelBgDeep,
                ImGuiGameUi.BorderCool,
                2f);

            GUI.skin.label.fontSize = 15;

            if (drawImGuiMission)
            {
                DrawMissionHeaderBlock(ref y, maxW);
            }
            else if (missionContext == null)
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 22f),
                    "스커미시 | 적 코어 파괴 시 승리 | 결과 후 R 재시작");
                y += 22f;
            }

            // 첫 플레이용 — 덱·이동을 한눈에 (F1·아래 줄과 중복돼도 유지)
            if (playerCore != null && match != null && !match.IsFinished)
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.AccentGold;
                GUI.Label(
                    new Rect(x0, y, maxW, 24f),
                    "▶ 숫자 1~8 덱 생산  ·  우클릭 이동·공격  ·  자세히 F1");
                y += 24f;
            }

            if (economy != null)
            {
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(
                    new Rect(x0, y, maxW, 22f),
                    $"자원 {economy.PlayerCredits:0} (+{economy.PlayerTotalIncomePerSecond:0.##}/초)  |  적 자원 {economy.EnemyCredits:0}");
                y += 22f;
            }

            if (playerCore != null)
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 20f),
                    "1~8 생산 | 우클릭 이동·공격 | T Y U 코어 강화 | V 표적");
                y += 20f;

                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(x0, y, maxW, 20f), BuildDeckLine());
                y += 20f;

                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 20f),
                    $"생산 큐 {playerCore.QueueCount}  |  F1 도움말  |  P · 1 · 2 · 3 배속");
                y += 20f;

                RtsTimeControl rtc = RtsTimeControl.Instance;
                if (rtc != null && match != null && !match.IsFinished &&
                    (campaignFlow == null || campaignFlow.IsGameplayStarted))
                {
                    GUI.color = ImGuiGameUi.TextMuted;
                    string line = rtc.GetStatusLineKo();
                    if (!string.IsNullOrEmpty(line))
                    {
                        GUI.Label(new Rect(x0, y, maxW, 20f), line);
                    }
                }
            }

            GUI.color = Color.white;

            if (match != null && match.IsFinished && missionContext == null)
            {
                DrawSkirmishResultOverlay();
            }

            DrawCommandRejectTransientHint();

            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawCommandRejectTransientHint()
        {
            if (Time.unscaledTime >= commandRejectHintHideUnscaled)
            {
                return;
            }

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            const string msg = "명령 불가 — 바닥·지형 또는 표적을 확인하십시오.";
            float w = Mathf.Min(520f, Screen.width - 32f);
            GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height - 56f, w, 24f), msg);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = prevSize;
        }

        private string BuildDeckLine()
        {
            if (playerCore == null)
            {
                return string.Empty;
            }

            string line = "덱 ";
            for (int i = 0; i < 8; i++)
            {
                if (i > 0)
                {
                    line += "  ";
                }

                line += $"{i + 1}:{GetShortName(playerCore.GetDeckSlot(i))}";
            }

            return line;
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

            Rect box = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.38f, 420f, 112f);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);

            GUI.skin.label.fontSize = 22;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 16f, box.y + 18f, box.width - 32f, 34f), msg);
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(box.x + 16f, box.y + 58f, box.width - 32f, 24f), "R 키로 다시 시작");
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
            float quickLine = playerCore != null ? 24f : 0f;

            if (missionContext == null && !showMissionBlock)
            {
                return 140f + quickLine;
            }

            if (!drawImGuiMission && showMissionBlock)
            {
                float h = missionContext != null &&
                          missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense
                    ? 166f
                    : 140f;
                return h + quickLine;
            }

            if (showMissionBlock && missionContext != null &&
                missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense)
            {
                return 208f + quickLine;
            }

            return (showMissionBlock ? 186f : 140f) + quickLine;
        }

        private void DrawMissionHeaderBlock(ref float y, float maxW)
        {
            string primary = MissionObjectiveDisplayText.GetPrimaryLine(missionContext.ObjectiveKind);
            string hint = MissionObjectiveDisplayText.GetGameplayHint(missionContext.ObjectiveKind);
            string extra = GetDefenseCountdownLine();

            float blockH = string.IsNullOrEmpty(extra) ? 56f : 74f;
            float x0 = PanelPad;

            ImGuiGameUi.DrawFilledRect(new Rect(x0, y, maxW, 2f), ImGuiGameUi.BorderAccent);

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x0, y + 6f, maxW, 22f), $"{missionContext.DisplayName} | {primary}");

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            string description = string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}";
            GUI.Label(new Rect(x0, y + 28f, maxW, 48f), description);

            GUI.skin.label.fontSize = 15;
            GUI.color = Color.white;
            y += blockH + 6f;
        }

        private string GetDefenseCountdownLine()
        {
            if (missionContext == null || missionContext.ObjectiveKind != MissionObjectiveKind.SanctuaryDefense)
            {
                return string.Empty;
            }

            if (campaignFlow == null || !campaignFlow.IsGameplayStarted)
            {
                return "작전이 시작되면 방어 타이머가 진행됩니다.";
            }

            float elapsed = Time.time - campaignFlow.GameplayStartTime;
            float remain = Mathf.Max(0f, missionContext.DefenseDurationSeconds - elapsed);
            return $"남은 시간 {remain:0}초 / 목표 {missionContext.DefenseDurationSeconds:0}초 생존";
        }
    }
}
