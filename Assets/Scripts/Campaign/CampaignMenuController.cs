using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.Campaign
{
    /// <summary>
    /// 캠페인 미션 목록 — OnGUI. Catalog 가 비어 있으면 런타임 데모 버튼 표시.
    /// </summary>
    public sealed class CampaignMenuController : MonoBehaviour
    {
        [SerializeField] private CampaignMissionCatalog catalog;

        [SerializeField] private bool useRuntimeDemoIfCatalogEmpty = true;

        private void OnGUI()
        {
            DrawMenuBackground();

            float pad = 36f;
            float panelW = Mathf.Min(920f, Screen.width - pad * 2f);

            GUI.skin.label.fontSize = 32;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 32f, panelW, 48f), "교단 작전 본부");
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(pad, 78f, panelW, 28f), "ORBITAL COMMAND  ·  작전 구역을 선택하십시오.");
            string progressDots = BuildMissionProgressDotsLine();
            if (!string.IsNullOrEmpty(progressDots))
            {
                GUI.Label(new Rect(pad, 100f, panelW, 24f), progressDots);
            }

            GUI.color = Color.white;

            if (catalog == null || catalog.Count == 0)
            {
                DrawEmptyCatalogPanel(pad, panelW);
                DrawClearButton(pad);
                return;
            }

            float listTop = string.IsNullOrEmpty(progressDots) ? 118f : 132f;
            Rect listPanel = new Rect(pad, listTop, panelW, Screen.height - listTop - 72f);
            ImGuiGameUi.DrawPanelFrame(listPanel, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);

            int unlocked = CampaignProgressStorage.GetHighestUnlockedMissionIndex();
            float rowY = listPanel.y + 18f;
            float innerW = listPanel.width - 40f;

            for (int i = 0; i < catalog.Count; i++)
            {
                MissionDefinition m = catalog.GetMissionAt(i);
                if (m == null)
                {
                    continue;
                }

                bool canPlay = i <= unlocked;
                string label =
                    $"{i + 1}.  {m.DisplayName}   ·   {MissionObjectiveDisplayText.GetShortLabelForMenu(m.ObjectiveKind)}";
                if (!canPlay)
                {
                    label += "   [잠김]";
                }

                Rect row = new Rect(listPanel.x + 20f, rowY, innerW, 46f);
                if (ImGuiGameUi.GameMenuButton(row, label, canPlay))
                {
                    StartMission(i, m);
                }

                rowY += 54f;
            }

            DrawClearButton(pad);
        }

        /// <summary>전체 배경 — 단색 + 좌측 톤 차이로 SF 메뉴 느낌.</summary>
        private static void DrawMenuBackground()
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.03f, 0.04f, 0.07f, 1f));
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width * 0.42f, Screen.height), new Color(0.05f, 0.07f, 0.12f, 0.55f));
        }

        private void DrawEmptyCatalogPanel(float pad, float panelW)
        {
            Rect box = new Rect(pad, 118f, panelW, 200f);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);
            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(box.x + 20f, box.y + 20f, box.width - 40f, 120f),
                "CampaignMissionCatalog 가 비어 있습니다.\nCreate → Game/Campaign/Mission Catalog 로 만든 뒤 인스펙터에 연결하거나, 아래 데모를 사용하세요.");
            GUI.color = Color.white;

            if (useRuntimeDemoIfCatalogEmpty &&
                ImGuiGameUi.GameMenuButton(new Rect(box.x + 20f, box.y + 130f, box.width - 40f, 48f),
                    "데모: 스커미시 (적 코어 파괴)"))
            {
                StartDemoSkirmish();
            }
        }

        /// <summary>캠페인 카탈로그 상위 5개 미션 클리어 여부(●/○).</summary>
        private string BuildMissionProgressDotsLine()
        {
            if (catalog == null || catalog.Count == 0)
            {
                return string.Empty;
            }

            int missionCount = Mathf.Min(5, catalog.Count);
            string line = "미션 클리어  ";
            for (int i = 0; i < missionCount; i++)
            {
                MissionDefinition m = catalog.GetMissionAt(i);
                bool cleared = m != null && CampaignProgressStorage.IsMissionCompleted(m.MissionId);
                line += cleared ? "● " : "○ ";
            }

            return line.TrimEnd();
        }

        private void DrawClearButton(float pad)
        {
            if (ImGuiGameUi.GameMenuButton(new Rect(pad, Screen.height - 62f, 260f, 48f), "진행 초기화 (해금·완료 표시)"))
            {
                CampaignProgressStorage.ClearAllProgress(catalog);
            }
        }

        private static void StartDemoSkirmish()
        {
            MissionDefinition demo = ScriptableObject.CreateInstance<MissionDefinition>();
            demo.AssignRuntimeCampaign(
                "데모 — 성스러운 스커미시",
                "NewSampleScene",
                MissionObjectiveKind.DestroyEnemyCore,
                null,
                null,
                null,
                null,
                120f,
                12f,
                0);

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(demo);
            core.PendingMissionOrderIndex = 0;
            CampaignSceneLoadUtility.TryLoadSceneByName("NewSampleScene", "데모 스커미시");
        }

        private void StartMission(int orderIndex, MissionDefinition mission)
        {
            if (mission == null)
            {
                Debug.LogWarning("[Campaign] 미션 정의가 null 입니다.");
                return;
            }

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(mission);
            core.PendingMissionOrderIndex = orderIndex;
            string scene = string.IsNullOrEmpty(mission.GameplaySceneName) ? "NewSampleScene" : mission.GameplaySceneName;
            CampaignSceneLoadUtility.TryLoadSceneByName(scene, $"미션: {mission.DisplayName}");
        }

        /// <summary>씬에 이미 있으면 새로 만들지 않음(비활성 오브젝트 포함 검색).</summary>
        private static PersistentGameCore EnsurePersistentCore()
        {
            if (PersistentGameCore.Instance != null)
            {
                return PersistentGameCore.Instance;
            }

            // Instance 가 아직 없을 때(비활성 포함) 씬에 배치된 코어가 있으면 그걸 쓴다 — 빈 코어 중복 생성 방지
            PersistentGameCore existingInScene = Object.FindObjectOfType<PersistentGameCore>(true);
            if (existingInScene != null)
            {
                return existingInScene;
            }

            GameObject go = new GameObject("PersistentGameCore");
            return go.AddComponent<PersistentGameCore>();
        }
    }
}
