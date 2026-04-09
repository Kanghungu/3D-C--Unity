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
            // 부제가 한 줄 늘어나 체크 항목 시작 위치만 살짝 아래로
            float panelH = 182f;
            Rect r = new Rect(Screen.width - panelW - 18f, 124f, panelW, panelH);
            ImGuiGameUi.DrawPanelFrame(r, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, 2f);

            int prod = runStats != null ? runStats.GetTotalPlayerUnitsProduced() : 0;
            bool okProd = prod > 0;
            bool okMove = notifiedMove;
            bool okMm = notifiedMinimap;
            bool okRally = notifiedRally;

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(new Rect(r.x + 10f, r.y + 8f, r.width - 20f, 22f), DemoPresentationCopy.FirstPlayChecklistTitle);
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(r.x + 10f, r.y + 28f, r.width - 20f, 48f), DemoPresentationCopy.FirstPlayChecklistSubtitle);
            GUI.color = Color.white;

            float ly = r.y + 76f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okProd, DemoPresentationCopy.FirstPlayCheckProd);
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okMove, DemoPresentationCopy.FirstPlayCheckMove);
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okMm, DemoPresentationCopy.FirstPlayCheckMinimap);
            ly += 22f;
            DrawCheckLine(new Rect(r.x + 12f, ly, r.width - 24f, 20f), okRally, DemoPresentationCopy.FirstPlayCheckRally);

            Rect hideBtn = new Rect(r.x + 10f, r.yMax - 34f, r.width - 20f, 26f);
            GUI.skin.label.fontSize = 12;
            if (GUI.Button(hideBtn, DemoPresentationCopy.FirstPlayChecklistHideButton))
            {
                PlayerPrefs.SetInt(PrefsChecklistHidden, 1);
                PlayerPrefs.Save();
            }

            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawCheckLine(Rect lineRect, bool done, string label)
        {
            string mark = done ? "[✓]" : "[  ]";
            GUI.color = done ? ImGuiGameUi.AccentCyan : ImGuiGameUi.TextMuted;
            GUI.Label(lineRect, $"{mark} {label}");
            GUI.color = Color.white;
        }
    }
}
