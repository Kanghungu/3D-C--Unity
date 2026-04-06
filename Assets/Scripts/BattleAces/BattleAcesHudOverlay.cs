using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 자원·덱·승패 안내 — ImGuiGameUi 톤과 맞춘 패널형 HUD.
    /// </summary>
    public class BattleAcesHudOverlay : MonoBehaviour
    {
        private const float PanelPad = 14f;

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
            float x0 = PanelPad;
            float topReserve = GetImGuiTopReserve();
            float y = topReserve;
            float maxW = Mathf.Min(1020f, Screen.width - PanelPad * 2f);

            bool showMissionBlock = missionContext != null &&
                                    (campaignFlow == null || campaignFlow.IsGameplayStarted);
            // UGUI가 상단 목표를 그리면 IMGUI 목표 블록은 생략
            bool drawImGuiMission = showMissionBlock &&
                                    (uguiObjective == null || !uguiObjective.HasObjectiveUi);

            // 배경 패널 — 미션+자원+덱을 한 덩어리로 묶음 (UGUI 목표 아래부터)
            float panelH = EstimatePanelHeight(showMissionBlock, drawImGuiMission);
            float panelTop = Mathf.Max(PanelPad - 4f, topReserve - 10f);
            ImGuiGameUi.DrawPanelFrame(
                new Rect(x0 - 6f, panelTop, maxW + 12f, panelH),
                ImGuiGameUi.PanelBgDeep,
                ImGuiGameUi.BorderCool,
                2f);

            if (drawImGuiMission)
            {
                DrawMissionHeaderBlock(ref y, maxW);
            }
            else if (missionContext == null)
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(x0, y, maxW, 26f), "Battle Aces — 단독 실행 · 적 코어 파괴 시 승리 · R 재시작(종료 후)");
                y += 28f;
                GUI.color = ImGuiGameUi.TextTitle;
            }

            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;

            if (economy != null)
            {
                GUI.Label(new Rect(x0, y, maxW, 26f),
                    $"자원  {economy.PlayerCredits:0}  (+{economy.PlayerTotalIncomePerSecond:0.##}/s)   ·   적 추정  {economy.EnemyCredits:0}");
                y += 26f;
            }

            if (playerCore != null)
            {
                string deckHint = missionContext != null
                    ? "미션 덱 · 1~8 생산 · 우클릭 이동/공격"
                    : "덱 · 1~8 생산 · 우클릭 이동/공격";
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(x0, y, maxW, 26f), deckHint);
                y += 26f;

                GUI.color = ImGuiGameUi.TextTitle;
                string deckLine = string.Empty;
                for (int i = 0; i < 8; i++)
                {
                    deckLine += $"{i + 1}) {playerCore.GetDeckSlot(i)}   ";
                }

                GUI.Label(new Rect(x0, y, maxW, 28f), deckLine);
                y += 28f;
                GUI.color = ImGuiGameUi.TextMuted;
                string vState = BattleAcesCombatSettings.AutoAcquireMode == AutoAcquireMode.PreferNearest
                    ? "자동 표적: 가까운 적 우선 (V 전환)"
                    : "자동 표적: 표적 고정 (V 전환)";
                GUI.Label(new Rect(x0, y, maxW, 52f),
                    "코어 업그레이드  T · Y · U   |   P / 1·2·3 배속   |   F1 도움말\n" + vState);
                y += 48f;
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(x0, y, 400f, 24f), $"생산 대기  {playerCore.QueueCount}");
                y += 26f;
            }

            GUI.color = Color.white;

            if (match != null && match.IsFinished && missionContext == null)
            {
                DrawSkirmishResultOverlay();
            }
        }

        /// <summary>캠페인 없이 씬만 열었을 때 승패 표시.</summary>
        private void DrawSkirmishResultOverlay()
        {
            if (match == null)
            {
                return;
            }

            string msg = match.State == BattleAcesMatchController.MatchState.Victory
                ? "승리 — 적 코어 파괴"
                : "패배 — 아군 코어 파괴";

            float bw = 420f;
            float bh = 120f;
            Rect box = new Rect(Screen.width * 0.5f - bw * 0.5f, Screen.height * 0.38f, bw, bh);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);

            GUI.skin.label.fontSize = 22;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 16f, box.y + 20f, box.width - 32f, 36f), msg);
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(box.x + 16f, box.y + 64f, box.width - 32f, 28f), "R 키 — 재시작");
            GUI.color = Color.white;
        }

        /// <summary>UGUI 목표 스트립이 있으면 그 아래에서 IMGUI를 그린다.</summary>
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
            if (missionContext == null && !showMissionBlock)
            {
                return 276f;
            }

            if (!drawImGuiMission && showMissionBlock)
            {
                return missionContext != null &&
                       missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense
                    ? 268f
                    : 248f;
            }

            if (showMissionBlock && missionContext != null &&
                missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense)
            {
                return 338f;
            }

            return showMissionBlock ? 298f : 248f;
        }

        /// <summary>상단 목표 블록 — 금색 구분선.</summary>
        private void DrawMissionHeaderBlock(ref float y, float maxW)
        {
            string primary = MissionObjectiveDisplayText.GetPrimaryLine(missionContext.ObjectiveKind);
            string hint = MissionObjectiveDisplayText.GetGameplayHint(missionContext.ObjectiveKind);
            string extra = GetDefenseCountdownLine();

            float blockH = string.IsNullOrEmpty(extra) || extra.StartsWith("작전 시작") ? 62f : 86f;
            float x0 = PanelPad;

            ImGuiGameUi.DrawFilledRect(new Rect(x0, y, maxW, 2f), ImGuiGameUi.BorderAccent);

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x0, y + 6f, maxW, 28f), $"《 {missionContext.DisplayName} 》   ·   {primary}");
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.skin.label.fontSize = 13;
            GUI.Label(new Rect(x0, y + 34f, maxW, 48f), string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}");
            GUI.skin.label.fontSize = 14;
            GUI.color = Color.white;
            y += blockH + 10f;
        }

        /// <summary>성역 방어 전용 — 남은 시간.</summary>
        private string GetDefenseCountdownLine()
        {
            if (missionContext == null || missionContext.ObjectiveKind != MissionObjectiveKind.SanctuaryDefense)
            {
                return string.Empty;
            }

            if (campaignFlow == null || !campaignFlow.IsGameplayStarted)
            {
                return "작전 시작 후 제한 시간이 표시됩니다.";
            }

            float elapsed = Time.time - campaignFlow.GameplayStartTime;
            float remain = Mathf.Max(0f, missionContext.DefenseDurationSeconds - elapsed);
            return $"남은 시간  {remain:0}초  /  목표  {missionContext.DefenseDurationSeconds:0}초  생존";
        }
    }
}
