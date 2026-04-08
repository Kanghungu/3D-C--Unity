using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.Campaign
{
    public sealed class CampaignMenuController : MonoBehaviour
    {
        /// <summary>메인 → 캠페인(미션·진행) / 데모(한 판 전투) 분리</summary>
        private enum MainMenuLayer
        {
            Root,
            Campaign,
            Demo
        }

        [SerializeField] private CampaignMissionCatalog catalog;

        [Tooltip("카탈로그가 비어 있을 때도 데모 탭에서 한 판 전투를 허용합니다. 끄면 카탈로그가 있을 때만 데모 버튼이 동작합니다.")]
        [SerializeField] private bool useRuntimeDemoIfCatalogEmpty = true;

        private MainMenuLayer currentLayer = MainMenuLayer.Root;

        private void Update()
        {
            // 챕터 1: 하위 화면에서 Esc 로 메인 복귀(전투 전 메뉴만)
            if (currentLayer == MainMenuLayer.Root)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                currentLayer = MainMenuLayer.Root;
            }
        }

        private void OnGUI()
        {
            ImGuiGameUi.BeginScaledGui();
            DrawMenuBackground();

            float pad = 36f;
            float panelW = Mathf.Min(920f, Screen.width - pad * 2f);

            switch (currentLayer)
            {
                case MainMenuLayer.Root:
                    DrawRootLayer(pad, panelW);
                    break;
                case MainMenuLayer.Campaign:
                    DrawCampaignLayer(pad, panelW);
                    break;
                case MainMenuLayer.Demo:
                    DrawDemoLayer(pad, panelW);
                    break;
            }

            DrawVersionFooter(pad, panelW);
            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawVersionFooter(float pad, float panelW)
        {
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, Screen.height - 24f, panelW, 22f),
                DemoPresentationCopy.BuildVersionFooterKo());
            GUI.color = Color.white;
        }

        /// <summary>메인 — 캠페인 / 데모 선택만</summary>
        private void DrawRootLayer(float pad, float panelW)
        {
            GUI.skin.label.fontSize = 34;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 32f, panelW, 48f), "Orbital Command");

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 76f, panelW, 26f), "한 판 데모 — 여기서 시작");
            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, 102f, panelW, 40f),
                "캠페인(●○)은 옵션입니다. 데모·캠페인 하위 화면에서 Esc 또는 「← 메인 (Esc)」으로 복귀합니다.");

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(
                new Rect(pad, 138f, panelW, 52f),
                DemoPresentationCopy.RoundGoalOneLineKo + "\n" + DemoPresentationCopy.AfterMatchExitOneLineKo);
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 15;

            float btnH = 64f;
            float gap = 18f;
            float startY = Mathf.Max(208f, Screen.height * 0.34f);
            // 데모를 위에 두어 진입·메뉴 흐름이 프로젝트 목표와 맞춤
            Rect demoBtn = new Rect(pad, startY, panelW, btnH);
            Rect campaignBtn = new Rect(pad, startY + btnH + gap, panelW, btnH);

            if (ImGuiGameUi.GameMenuButton(demoBtn, "데모 (본전)\n한 판 전투 · 난이도 선택 · 브리핑 없음"))
            {
                currentLayer = MainMenuLayer.Demo;
            }

            if (ImGuiGameUi.GameMenuButton(campaignBtn, "캠페인 (옵션)\n미션·브리핑 · 진행 저장"))
            {
                currentLayer = MainMenuLayer.Campaign;
            }

            if (catalog != null && catalog.Count > 0)
            {
                Rect continueBtn = new Rect(pad, startY + (btnH + gap) * 2f, panelW, btnH);
                if (ImGuiGameUi.GameMenuButton(continueBtn, "캠페인 이어하기\n진행 중인 미션부터 바로 시작"))
                {
                    int idx = CampaignProgressStorage.GetSuggestedContinueMissionOrderIndex(catalog);
                    MissionDefinition cont = idx >= 0 ? catalog.GetMissionAt(idx) : null;
                    if (cont != null)
                    {
                        StartMission(idx, cont);
                    }
                    else
                    {
                        Debug.LogWarning("[Campaign] 이어하기: 유효한 미션을 찾지 못했습니다.");
                    }
                }
            }
        }

        private void DrawBackToMainButton(float pad, float y)
        {
            if (ImGuiGameUi.GameMenuButton(new Rect(pad, y, 220f, 44f), "← 메인 (Esc)"))
            {
                currentLayer = MainMenuLayer.Root;
            }
        }

        /// <summary>캠페인 — 미션 목록(챕터 순서)만, 스커미시는 데모 측</summary>
        private void DrawCampaignLayer(float pad, float panelW)
        {
            DrawBackToMainButton(pad, 28f);

            GUI.skin.label.fontSize = 28;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 78f, panelW, 40f), "캠페인");

            string progressDots = BuildMissionProgressDotsLine();
            float contentTop = 118f;
            if (!string.IsNullOrEmpty(progressDots))
            {
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(new Rect(pad, contentTop, panelW, 24f), progressDots);
                contentTop += 28f;
            }

            GUI.color = Color.white;

            if (catalog == null || catalog.Count == 0)
            {
                DrawCampaignEmptyPanel(pad, panelW, contentTop);
                return;
            }

            float listTop = contentTop + 8f;
            float listH = Mathf.Max(100f, Screen.height - listTop - 72f);
            Rect listPanel = new Rect(pad, listTop, panelW, listH);
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
                string label =
                    $"{i + 1}. {MissionObjectiveDisplayText.ResolveMissionDisplayName(mission)} | {MissionObjectiveDisplayText.GetShortLabelForMenu(mission)}";
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

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, Screen.height - 118f, panelW, 44f),
                "진행 저장: 미션 클리어·해금만 기억합니다. 전투 중간 저장(세이브)은 후순위로 검토합니다.");
            GUI.color = Color.white;

            DrawClearButton(pad);
        }

        /// <summary>데모 — 스커미시 난이도만(전투 씬 직행)</summary>
        private void DrawDemoLayer(float pad, float panelW)
        {
            DrawBackToMainButton(pad, 28f);

            GUI.skin.label.fontSize = 28;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(pad, 78f, panelW, 40f), "데모");

            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, 118f, panelW, 40f),
                DemoPresentationCopy.RoundGoalOneLineKo);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, 156f, panelW, 36f),
                "맵·덱은 동일하고, 난이도별로 적 생산 간격·개장 부스트만 다릅니다. " +
                DemoPresentationCopy.AfterMatchExitOneLineKo);
            GUI.color = Color.white;

            if (!CanShowStandaloneDemo())
            {
                Rect box = new Rect(pad, 198f, panelW, 120f);
                ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);
                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(box.x + 16f, box.y + 16f, box.width - 32f, 88f),
                    "카탈로그가 비어 있고 「빈 카탈로그일 때 데모 허용」이 꺼져 있습니다.\n" +
                    "CampaignMissionCatalog를 연결하거나 Inspector에서 해당 옵션을 켜 주세요.");
                GUI.color = Color.white;
                return;
            }

            float listTop = DrawDemoSkirmishRow(pad, panelW, hintLineY: 198f);

            float yAfterRow = listTop + 10f;
            Rect keyBox = new Rect(pad, yAfterRow, panelW, 56f);
            ImGuiGameUi.DrawPanelFrame(keyBox, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 1f);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(keyBox.x + 12f, keyBox.y + 8f, keyBox.width - 24f, 44f),
                DemoPresentationCopy.KeyboardOnlyNoticeKo + "\n" + DemoPresentationCopy.AfterMatchExitOneLineKo);
            GUI.color = Color.white;

            yAfterRow += 64f;
            Rect variantBtn = new Rect(pad, yAfterRow, panelW, 52f);
            if (ImGuiGameUi.GameMenuButton(
                    variantBtn,
                    "데모 변주 (압박 물결)\n보통 난이도 · 적 패턴만 더 공격적 (같은 맵·덱)"))
            {
                StartSkirmishVariantAggressive(SkirmishDifficultyTier.Normal);
            }

            yAfterRow += 58f;
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, yAfterRow, panelW, 48f),
                "캠페인·첫 미션 연습 시 난이도: Normal 권장(튜토리얼 밸런스). Easy는 빠른 확인용, Hard는 압박 테스트용.");
            GUI.color = Color.white;

            if (catalog == null || catalog.Count == 0)
            {
                Rect hint = new Rect(pad, yAfterRow + 50f, panelW, 40f);
                GUI.skin.label.fontSize = 13;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(hint, "캠페인 미션이 없어도 위 버튼으로 전투 씬을 바로 열 수 있습니다.");
                GUI.color = Color.white;
            }
        }

        /// <summary>미션 목록이 있거나, 빈 카탈로그 데모 허용 시 데모 시작 가능</summary>
        private bool CanShowStandaloneDemo()
        {
            if (catalog != null && catalog.Count > 0)
            {
                return true;
            }

            return useRuntimeDemoIfCatalogEmpty;
        }

        private static void DrawMenuBackground()
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.03f, 0.04f, 0.07f, 1f));
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width * 0.42f, Screen.height), new Color(0.05f, 0.07f, 0.12f, 0.55f));
        }

        /// <summary>데모 화면 — 난이도 3버튼, 마지막 줄 아래 Y 반환</summary>
        private static float DrawDemoSkirmishRow(float pad, float panelW, float hintLineY)
        {
            const float hintH = 40f;
            const float btnH = 44f;
            const float innerGap = 6f;

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(pad, hintLineY, panelW, hintH),
                "데모 시작 · 브리핑 없음 · ●○ 미반영\n" +
                MissionDefinition.BuildSkirmishDemoMenuStatsLine());
            GUI.color = Color.white;

            float btnY = hintLineY + hintH + innerGap;
            DrawSkirmishThreeButtonsInRow(pad, btnY, panelW, btnH);
            return btnY + btnH + 14f;
        }

        /// <summary>한 줄에 쉬움·보통·어려움 (간격 균등)</summary>
        private static void DrawSkirmishThreeButtonsInRow(float padX, float y, float totalW, float btnH)
        {
            const float gapX = 8f;
            float btnW = (totalW - gapX * 2f) / 3f;
            float x0 = padX;
            float x1 = padX + btnW + gapX;
            float x2 = padX + (btnW + gapX) * 2f;

            if (ImGuiGameUi.GameMenuButton(
                    new Rect(x0, y, btnW, btnH),
                    "쉬움\n생산×" + MissionDefinition.FormatSkirmishThinkMultiplierForButton(SkirmishDifficultyTier.Easy)))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Easy);
            }

            if (ImGuiGameUi.GameMenuButton(
                    new Rect(x1, y, btnW, btnH),
                    "보통\n생산×" + MissionDefinition.FormatSkirmishThinkMultiplierForButton(SkirmishDifficultyTier.Normal)))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Normal);
            }

            if (ImGuiGameUi.GameMenuButton(
                    new Rect(x2, y, btnW, btnH),
                    "어려움\n생산×" + MissionDefinition.FormatSkirmishThinkMultiplierForButton(SkirmishDifficultyTier.Hard)))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Hard);
            }
        }

        /// <summary>캠페인 탭에서 카탈로그 없을 때 — 데모 탭 안내</summary>
        private void DrawCampaignEmptyPanel(float pad, float panelW, float topY)
        {
            Rect box = new Rect(pad, topY, panelW, 220f);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);
            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(box.x + 20f, box.y + 20f, box.width - 40f, 120f),
                "캠페인 카탈로그가 비어 있습니다.\n" +
                "Inspector에 CampaignMissionCatalog를 넣거나,\n" +
                "「← 메인」 후 「데모」에서 한 판 전투를 시작하세요.");
            GUI.color = Color.white;
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

        /// <summary>런타임 스커미시 미션 — NewSampleScene · 브리핑 생략 · 해금/완료 저장 없음</summary>
        private static void StartSkirmishVsAi(SkirmishDifficultyTier tier)
        {
            MissionDefinition skirmish = ScriptableObject.CreateInstance<MissionDefinition>();
            skirmish.AssignSkirmishVsAiRuntime(tier);

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(skirmish);
            core.PendingMissionOrderIndex = -1;
            string loadTag = tier switch
            {
                SkirmishDifficultyTier.Easy => "Skirmish vs AI (Easy)",
                SkirmishDifficultyTier.Hard => "Skirmish vs AI (Hard)",
                _ => "Skirmish vs AI (Normal)"
            };
            CampaignSceneLoadUtility.TryLoadSceneByName("NewSampleScene", loadTag);
        }

        /// <summary>동일 스커미시 규칙 + 적 AI 패턴만 변주(2회차 플레이 동기)</summary>
        private static void StartSkirmishVariantAggressive(SkirmishDifficultyTier tier)
        {
            MissionDefinition skirmish = ScriptableObject.CreateInstance<MissionDefinition>();
            skirmish.AssignSkirmishVsAiRuntime(tier);
            string suffix = tier switch
            {
                SkirmishDifficultyTier.Easy => "easy",
                SkirmishDifficultyTier.Hard => "hard",
                _ => "normal"
            };
            skirmish.ApplySkirmishVariantEnemyPattern(
                "aggressive_push",
                "skirmish_demo_variant_aggressive_" + suffix,
                "데모 변주 — 압박 물결 (" + (tier == SkirmishDifficultyTier.Easy ? "쉬움" : tier == SkirmishDifficultyTier.Hard ? "어려움" : "보통") + ")");

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(skirmish);
            core.PendingMissionOrderIndex = -1;
            CampaignSceneLoadUtility.TryLoadSceneByName("NewSampleScene", "Skirmish variant aggressive_push");
        }

        private void StartMission(int orderIndex, MissionDefinition mission)
        {
            if (mission == null)
            {
                Debug.LogWarning("[Campaign] Mission is null.");
                return;
            }

            CampaignProgressStorage.SaveLastCampaignMissionOrderIndex(orderIndex);

            PersistentGameCore core = EnsurePersistentCore();
            core.SetActiveMission(mission);
            core.PendingMissionOrderIndex = orderIndex;
            string scene = string.IsNullOrEmpty(mission.GameplaySceneName) ? "NewSampleScene" : mission.GameplaySceneName;
            CampaignSceneLoadUtility.TryLoadSceneByName(
                scene,
                $"Mission: {MissionObjectiveDisplayText.ResolveMissionDisplayName(mission)}");
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
