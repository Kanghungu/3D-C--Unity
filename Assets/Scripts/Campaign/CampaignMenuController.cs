using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.Campaign
{
    public sealed class CampaignMenuController : MonoBehaviour
    {
        [SerializeField] private CampaignMissionCatalog catalog;
        [SerializeField] private bool useRuntimeDemoIfCatalogEmpty = true;

        private void OnGUI()
        {
            ImGuiGameUi.BeginScaledGui();
            DrawMenuBackground();

            float pad = 36f;
            float panelW = Mathf.Min(920f, Screen.width - pad * 2f);

            GUI.skin.label.fontSize = 34;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 32f, panelW, 48f), "Orbital Command");

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(pad, 78f, panelW, 28f), "Select a mission and launch the prototype battle.");

            string progressDots = BuildMissionProgressDotsLine();
            if (!string.IsNullOrEmpty(progressDots))
            {
                GUI.Label(new Rect(pad, 102f, panelW, 24f), progressDots);
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
                MissionDefinition mission = catalog.GetMissionAt(i);
                if (mission == null)
                {
                    continue;
                }

                bool canPlay = i <= unlocked;
                string label = $"{i + 1}. {mission.DisplayName} | {MissionObjectiveDisplayText.GetShortLabelForMenu(mission.ObjectiveKind)}";
                if (!canPlay)
                {
                    label += " | Locked";
                }

                Rect row = new Rect(listPanel.x + 20f, rowY, innerW, 46f);
                if (ImGuiGameUi.GameMenuButton(row, label, canPlay))
                {
                    StartMission(i, mission);
                }

                rowY += 54f;
            }

            DrawClearButton(pad);
            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawMenuBackground()
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.03f, 0.04f, 0.07f, 1f));
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width * 0.42f, Screen.height), new Color(0.05f, 0.07f, 0.12f, 0.55f));
        }

        private void DrawEmptyCatalogPanel(float pad, float panelW)
        {
            Rect box = new Rect(pad, 118f, panelW, 220f);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);
            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(box.x + 20f, box.y + 20f, box.width - 40f, 88f),
                "No campaign catalog is assigned.\nAssign CampaignMissionCatalog or use the demo mission below.");
            GUI.color = Color.white;

            if (useRuntimeDemoIfCatalogEmpty &&
                ImGuiGameUi.GameMenuButton(new Rect(box.x + 20f, box.y + 126f, box.width - 40f, 48f), "Start Demo Skirmish"))
            {
                StartDemoSkirmish();
            }
        }

        private string BuildMissionProgressDotsLine()
        {
            if (catalog == null || catalog.Count == 0)
            {
                return string.Empty;
            }

            int missionCount = catalog.Count;
            string line = "Progress ";
            for (int i = 0; i < missionCount; i++)
            {
                MissionDefinition mission = catalog.GetMissionAt(i);
                if (mission == null)
                {
                    line += "[?] ";
                    continue;
                }

                bool cleared = CampaignProgressStorage.IsMissionCompleted(mission.MissionId);
                bool hasBonus = !string.IsNullOrEmpty(mission.OptionalBonusObjectiveId);
                bool bonusDone = CampaignProgressStorage.IsBonusObjectiveCompleted(mission.MissionId);

                if (!hasBonus)
                {
                    line += cleared ? "[●] " : "[○] ";
                }
                else if (cleared && bonusDone)
                {
                    line += "[●★] ";
                }
                else if (cleared)
                {
                    line += "[● ] ";
                }
                else
                {
                    line += "[○] ";
                }
            }

            return line.TrimEnd();
        }

        private void DrawClearButton(float pad)
        {
            if (ImGuiGameUi.GameMenuButton(new Rect(pad, Screen.height - 62f, 260f, 48f), "Reset Campaign Progress"))
            {
                CampaignProgressStorage.ClearAllProgress(catalog);
            }
        }

        private static void StartDemoSkirmish()
        {
            MissionDefinition demo = ScriptableObject.CreateInstance<MissionDefinition>();
            demo.AssignRuntimeCampaign(
                "Demo Skirmish",
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
            CampaignSceneLoadUtility.TryLoadSceneByName("NewSampleScene", "Demo Skirmish");
        }

        private void StartMission(int orderIndex, MissionDefinition mission)
        {
            if (mission == null)
            {
                Debug.LogWarning("[Campaign] Mission is null.");
                return;
            }

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(mission);
            core.PendingMissionOrderIndex = orderIndex;
            string scene = string.IsNullOrEmpty(mission.GameplaySceneName) ? "NewSampleScene" : mission.GameplaySceneName;
            CampaignSceneLoadUtility.TryLoadSceneByName(scene, $"Mission: {mission.DisplayName}");
        }

        private static PersistentGameCore EnsurePersistentCore()
        {
            if (PersistentGameCore.Instance != null)
            {
                return PersistentGameCore.Instance;
            }

            PersistentGameCore existingInScene = Object.FindAnyObjectByType<PersistentGameCore>(FindObjectsInactive.Include);
            if (existingInScene != null)
            {
                return existingInScene;
            }

            GameObject go = new GameObject("PersistentGameCore");
            return go.AddComponent<PersistentGameCore>();
        }
    }
}
