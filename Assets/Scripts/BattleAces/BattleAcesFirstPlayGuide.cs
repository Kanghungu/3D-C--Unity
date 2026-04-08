using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// 첫 캠페인 미션(mission_01_skirmish) 한정 — 목표 체크리스트 4항(F1·상단 목표와 중복 최소화).
    /// </summary>
    public sealed class BattleAcesFirstPlayGuide : MonoBehaviour
    {
        public const string FirstCampaignMissionId = "mission_01_skirmish";

        /// <summary>체크리스트 영구 숨김(같은 PC에서 다시 안 봄)</summary>
        private const string PrefsChecklistHidden = "ba_first_mission_checklist_hidden";

        private static bool notifiedMove;
        private static bool notifiedMinimap;
        private static bool notifiedRally;

        private MissionDefinition mission;
        private BattleMissionFlow missionFlow;
        private BattleAcesMatchController match;
        private BattleAcesRunStats runStats;

        /// <summary>씬 전환 시 정적 플래그 초기화</summary>
        private void Awake()
        {
            notifiedMove = false;
            notifiedMinimap = false;
            notifiedRally = false;
        }

        public void Initialize(
            MissionDefinition activeMission,
            BattleMissionFlow flow,
            BattleAcesMatchController matchController,
            BattleAcesRunStats stats)
        {
            mission = activeMission;
            missionFlow = flow;
            match = matchController;
            runStats = stats;
        }

        /// <summary>지면 이동/공격 이동 명령 성공 시 Selection 쪽에서 호출</summary>
        public static void NotifyGroundCommandIssued()
        {
            notifiedMove = true;
        }

        /// <summary>미니맵 클릭(시야 이동) 시 미니맵에서 호출</summary>
        public static void NotifyMinimapClick()
        {
            notifiedMinimap = true;
        }

        /// <summary>랠리 설정 시 HUD 퍼스와 함께 호출</summary>
        public static void NotifyRallySet()
        {
            notifiedRally = true;
        }

        private static bool IsChecklistDismissedPermanently()
        {
            return PlayerPrefs.GetInt(PrefsChecklistHidden, 0) == 1;
        }

        private void OnGUI()
        {
            if (IsChecklistDismissedPermanently())
            {
                return;
            }

            if (mission == null || mission.MissionId != FirstCampaignMissionId)
            {
                return;
            }

            if (missionFlow != null && !missionFlow.IsGameplayStarted)
            {
                return;
            }

            if (match != null && match.IsFinished)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();

            const float panelW = 268f;
            float panelH = 168f;
            Rect r = new Rect(Screen.width - panelW - 18f, 124f, panelW, panelH);
            ImGuiGameUi.DrawPanelFrame(r, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, 2f);

            int prod = runStats != null ? runStats.GetTotalPlayerUnitsProduced() : 0;
            bool okProd = prod > 0;
            bool okMove = notifiedMove;
            bool okMm = notifiedMinimap;
            bool okRally = notifiedRally;

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(r.x + 10f, r.y + 8f, r.width - 20f, 22f), "첫 작전 체크리스트");
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(r.x + 10f, r.y + 28f, r.width - 20f, 36f), "F1·상단 목표와 겹치지 않게 최소만 표시합니다.");
            GUI.color = Color.white;

            float ly = r.y + 62f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okProd, "유닛 생산 (키 1~8)");
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okMove, "지면 이동 (우클릭)");
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okMm, "전술 지도 클릭 (시야)");
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okRally, "집결 (Alt+우클릭)");

            Rect hideBtn = new Rect(r.x + 10f, r.yMax - 34f, r.width - 20f, 26f);
            GUI.skin.label.fontSize = 12;
            if (GUI.Button(hideBtn, "다시 안 보기"))
            {
                PlayerPrefs.SetInt(PrefsChecklistHidden, 1);
                PlayerPrefs.Save();
            }

            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawCheckLine(Rect lineRect, bool done, string label)
        {
            string mark = done ? "[✓]" : "[  ]";
            GUI.color = done ? new Color(0.45f, 0.95f, 0.55f, 1f) : ImGuiGameUi.TextMuted;
            GUI.Label(lineRect, $"{mark} {label}");
            GUI.color = Color.white;
        }
    }
}
