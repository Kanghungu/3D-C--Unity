using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.Settings;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Campaign
{
    public sealed class CampaignMenuController : MonoBehaviour
    {
        private enum MainMenuLayer
        {
            Root,
            Campaign,
            Demo
        }

        [SerializeField] private CampaignMissionCatalog catalog;
        [SerializeField] private bool useRuntimeDemoIfCatalogEmpty = true;

        private MainMenuLayer currentLayer = MainMenuLayer.Root;
        private float menuOpenedUnscaled;

        private void OnEnable()
        {
            menuOpenedUnscaled = Time.unscaledTime;
        }

        private void Update()
        {
            if (currentLayer == MainMenuLayer.Root)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                currentLayer = MainMenuLayer.Root;
            }
        }

        private void OnGUI()
        {
            ImGuiGameUi.BeginScaledGui();
            DrawMenuBackground();
            DrawLanguageToggle();

            switch (currentLayer)
            {
                case MainMenuLayer.Root:
                    DrawRootLayer();
                    break;
                case MainMenuLayer.Campaign:
                    DrawCampaignLayer();
                    break;
                case MainMenuLayer.Demo:
                    DrawDemoLayer();
                    break;
            }

            DrawVersionFooter();
            ImGuiGameUi.EndScaledGui();
        }

        private static void DrawVersionFooter()
        {
            Rect footer = new Rect(28f, Screen.height - 30f, Screen.width - 56f, 22f);
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(footer, DemoPresentationCopy.BuildVersionFooterKo());
            GUI.color = Color.white;
        }

        private static void DrawLanguageToggle()
        {
            Rect wrap = new Rect(Screen.width - 250f, 96f, 208f, 42f);
            ImGuiGameUi.DrawGlassPanel(
                wrap,
                new Color(ImGuiGameUi.PanelBgDeep.r, ImGuiGameUi.PanelBgDeep.g, ImGuiGameUi.PanelBgDeep.b, 0.9f),
                new Color(0.26f, 0.34f, 0.42f, 0.72f),
                ImGuiGameUi.AccentCyan);

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(wrap.x + 12f, wrap.y + 12f, 54f, 14f), L("언어", "LANG"));

            Rect track = new Rect(wrap.x + 66f, wrap.y + 8f, wrap.width - 78f, 26f);
            ImGuiGameUi.DrawPanelFrame(track, new Color(0.06f, 0.09f, 0.14f, 0.92f), new Color(0.22f, 0.3f, 0.38f, 0.8f), 1f);

            bool korean = GameUserSettings.Language == GameLanguage.Korean;
            Rect koRect = new Rect(track.x + 4f, track.y + 3f, 56f, 20f);
            Rect enRect = new Rect(track.x + 64f, track.y + 3f, 56f, 20f);

            Color prev = GUI.color;
            if (korean)
            {
                ImGuiGameUi.DrawPanelFrame(koRect, new Color(0.16f, 0.28f, 0.36f, 0.95f), ImGuiGameUi.AccentCyan, 1f);
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(koRect, "KO");
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(enRect, "EN");
            }
            else
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(koRect, "KO");
                ImGuiGameUi.DrawPanelFrame(enRect, new Color(0.22f, 0.2f, 0.12f, 0.95f), ImGuiGameUi.BorderAccent, 1f);
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(enRect, "EN");
            }

            GUI.color = prev;

            if (!korean && GUI.Button(koRect, GUIContent.none, GUIStyle.none))
            {
                GameUserSettings.SetLanguage(GameLanguage.Korean);
                GameUserSettings.Save();
            }

            if (korean && GUI.Button(enRect, GUIContent.none, GUIStyle.none))
            {
                GameUserSettings.SetLanguage(GameLanguage.English);
                GameUserSettings.Save();
            }
        }

        private void DrawRootLayer()
        {
            float margin = Mathf.Lerp(28f, 56f, Mathf.Clamp01(Screen.width / 1800f));
            float contentTop = 52f;
            float leftWidth = Mathf.Min(620f, Screen.width * 0.42f);
            float rightWidth = Mathf.Min(560f, Screen.width * 0.38f);
            float gap = 34f;

            Rect hero = AnimatedRect(new Rect(margin, contentTop, leftWidth, 262f), -20f, -18f, 0f);
            Rect command = AnimatedRect(new Rect(hero.xMax + gap, contentTop + 34f, rightWidth, 436f), 22f, 10f, 0.08f);

            DrawHeroPanel(hero);
            DrawRootCommandPanel(command);
        }

        private void DrawHeroPanel(Rect rect)
        {
            ImGuiGameUi.DrawGlassPanel(rect, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);

            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(new Rect(rect.x + 22f, rect.y + 18f, rect.width - 44f, 22f), L("전략 지휘 인터페이스", "STRATEGIC COMMAND INTERFACE"));

            GUI.skin.label.fontSize = 42;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 44f, rect.width - 40f, 50f), "Orbital Command");

            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 17 : 18;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(rect.x + 22f, rect.y + 102f, rect.width - 44f, 28f), L("싱글플레이 RTS 전투 프로토타입", "Single-player RTS combat prototype"));

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(rect.x + 22f, rect.y + 138f, rect.width - 44f, 72f),
                L(
                    "지금은 짧더라도 직접 조작 가능한 전투가 우선입니다. 카메라를 움직이고 유닛을 지휘하며, 교전에서 살아남아 한 판을 끝까지 완성하세요.",
                    "A short, playable battle flow comes first: move camera, command units, survive contact, and finish a match cleanly."));

            Rect infoBar = new Rect(rect.x + 20f, rect.yMax - 52f, rect.width - 40f, 32f);
            ImGuiGameUi.DrawPanelFrame(infoBar, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, 1f);
            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.ResourceHighlight;
            GUI.Label(infoBar, DemoPresentationCopy.RoundGoalOneLineKo);
            GUI.color = Color.white;
        }

        private void DrawRootCommandPanel(Rect rect)
        {
            ImGuiGameUi.DrawGlassPanel(rect, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, ImGuiGameUi.AccentGold);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(rect.x + 22f, rect.y + 18f, rect.width - 44f, 20f), L("주요 작전", "PRIMARY ACTIONS"));

            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 24 : 26;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(rect.x + 22f, rect.y + 44f, rect.width - 44f, 34f), L("다음 출격을 선택하세요", "Choose your next deployment"));

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(rect.x + 22f, rect.y + 86f, rect.width - 44f, 40f),
                L(
                    "이 메뉴는 단순 런처보다 전술 콘솔처럼 보여야 합니다. 데모가 가장 빠르게 전투에 들어가는 경로입니다.",
                    "This menu should feel like a tactical console, not a plain launcher. Demo is the fastest path to live combat."));

            float buttonX = rect.x + 22f;
            float buttonY = rect.y + 138f;
            float buttonW = rect.width - 44f;
            float buttonH = 62f;
            float buttonGap = 14f;

            if (ImGuiGameUi.GameMenuButton(
                    new Rect(buttonX, buttonY, buttonW, buttonH),
                    L("즉시 전투\n브리핑 없이 바로 한 판 전투에 진입", "Instant Battle\nJump straight into one match with no briefing")))
            {
                currentLayer = MainMenuLayer.Demo;
                menuOpenedUnscaled = Time.unscaledTime;
            }

            if (ImGuiGameUi.GameMenuButton(
                    new Rect(buttonX, buttonY + buttonH + buttonGap, buttonW, buttonH),
                    L("캠페인 미션\n미션 흐름, 진행 저장, 씬 기반 목표", "Campaign Missions\nMission flow, progression, and scene-based objectives")))
            {
                currentLayer = MainMenuLayer.Campaign;
                menuOpenedUnscaled = Time.unscaledTime;
            }

            bool canContinue = catalog != null && catalog.Count > 0;
            if (ImGuiGameUi.GameMenuButton(
                    new Rect(buttonX, buttonY + (buttonH + buttonGap) * 2f, buttonW, buttonH),
                    L("캠페인 이어하기\n다음 해금 미션부터 바로 재개", "Continue Campaign\nResume the next unlocked mission"),
                    canContinue))
            {
                int idx = CampaignProgressStorage.GetSuggestedContinueMissionOrderIndex(catalog);
                MissionDefinition cont = idx >= 0 ? catalog.GetMissionAt(idx) : null;
                if (cont != null)
                {
                    StartMission(idx, cont);
                }
            }

            Rect note = new Rect(rect.x + 22f, rect.yMax - 48f, rect.width - 44f, 24f);
            ImGuiGameUi.DrawPanelFrame(note, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, 1f);
            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(note, L("Esc는 하위 메뉴에서 돌아가기, R은 전투 재시작, F10은 진단 HUD 토글입니다.", "Esc returns from submenus. R restarts after a match. F10 toggles diagnostics."));
            GUI.color = Color.white;
        }

        private void DrawCampaignLayer()
        {
            float margin = 32f;
            Rect header = AnimatedRect(new Rect(margin, 34f, Screen.width - margin * 2f, 112f), 0f, -18f, 0f);
            Rect body = AnimatedRect(new Rect(margin, 160f, Screen.width - margin * 2f, Screen.height - 238f), 0f, 18f, 0.08f);

            DrawSubmenuHeader(
                header,
                L("캠페인 미션", "Campaign Missions"),
                L("같은 전투 규칙 위에 짧은 스토리 미션을 얹는 흐름입니다.", "Short story missions layered on top of the same battle rules."));

            if (catalog == null || catalog.Count == 0)
            {
                ImGuiGameUi.DrawGlassPanel(body, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
                GUI.skin.label.fontSize = 18;
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(body.x + 28f, body.y + 28f, body.width - 56f, 28f), L("미션 카탈로그가 연결되지 않았습니다", "No mission catalog assigned"));

                GUI.skin.label.fontSize = 13;
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(body.x + 28f, body.y + 64f, body.width - 56f, 60f),
                    L("Inspector에서 CampaignMissionCatalog를 연결하거나, 즉시 전투로 빠르게 플레이 테스트를 진행하세요.", "Assign a CampaignMissionCatalog in the inspector, or use Instant Battle for quick playtesting."));
                GUI.color = Color.white;
                return;
            }

            ImGuiGameUi.DrawGlassPanel(body, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentGold);

            string progressDots = BuildMissionProgressDotsLine();
            if (!string.IsNullOrEmpty(progressDots))
            {
                GUI.skin.label.fontSize = 12;
                GUI.color = ImGuiGameUi.ResourceHighlight;
                GUI.Label(new Rect(body.x + 24f, body.y + 18f, body.width - 48f, 20f), progressDots);
            }

            int unlocked = CampaignProgressStorage.GetHighestUnlockedMissionIndex();
            float rowY = body.y + 54f;
            float rowW = body.width - 48f;
            for (int i = 0; i < catalog.Count; i++)
            {
                MissionDefinition mission = catalog.GetMissionAt(i);
                if (mission == null)
                {
                    continue;
                }

                bool canPlay = i <= unlocked;
                string title = $"{i + 1}. {MissionObjectiveDisplayText.ResolveMissionDisplayName(mission)}";
                string subtitle = MissionObjectiveDisplayText.GetShortLabelForMenu(mission);
                if (!canPlay)
                {
                    subtitle += GameUserSettings.Language == GameLanguage.Korean ? " | 잠김" : " | Locked";
                }

                if (ImGuiGameUi.GameMenuButton(new Rect(body.x + 24f, rowY, rowW, 54f), title + "\n" + subtitle, canPlay))
                {
                    StartMission(i, mission);
                }

                rowY += 62f;
            }

            if (ImGuiGameUi.GameMenuButton(new Rect(body.x + 24f, body.yMax - 56f, 280f, 38f), L("캠페인 진행 초기화", "Reset Campaign Progress")))
            {
                CampaignProgressStorage.ClearAllProgress(catalog);
            }
        }

        private void DrawDemoLayer()
        {
            float margin = 32f;
            Rect header = AnimatedRect(new Rect(margin, 34f, Screen.width - margin * 2f, 112f), 0f, -18f, 0f);
            Rect left = AnimatedRect(new Rect(margin, 160f, Mathf.Min(540f, Screen.width * 0.36f), Screen.height - 238f), -28f, 14f, 0.08f);
            Rect right = AnimatedRect(new Rect(left.xMax + 34f, 146f, Screen.width - margin - (left.xMax + 34f), Screen.height - 224f), 32f, 8f, 0.16f);

            DrawSubmenuHeader(
                header,
                L("즉시 전투", "Instant Battle"),
                L("한 판 전투에 바로 진입합니다. 전투 감각과 밸런스 반복 확인에 적합합니다.", "Direct access to one match. Good for moment-to-moment combat iteration."));

            ImGuiGameUi.DrawGlassPanel(left, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, ImGuiGameUi.AccentGold);
            ImGuiGameUi.DrawGlassPanel(right, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);

            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 20 : 22;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(left.x + 24f, left.y + 32f, left.width - 48f, 32f), L("전투 프로필 선택", "Select battle profile"));

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(left.x + 24f, left.y + 68f, left.width - 48f, 44f),
                L("맵과 덱은 동일하고, 난이도는 주로 적 압박과 생산 템포를 바꿉니다.", "Same core map and deck. Difficulty mainly changes AI pressure and production pace."));

            float btnY = left.y + 122f;
            float btnW = left.width - 48f;
            float btnH = 64f;
            float btnGap = 16f;
            bool demoEnabled = CanShowStandaloneDemo();

            if (ImGuiGameUi.GameMenuButton(new Rect(left.x + 24f, btnY, btnW, btnH), L("쉬움 스커미시\n빠른 확인과 낮은 압박", "Easy Skirmish\nFaster verification and lower pressure"), demoEnabled))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Easy);
            }

            if (ImGuiGameUi.GameMenuButton(new Rect(left.x + 24f, btnY + btnH + btnGap, btnW, btnH), L("보통 스커미시\n밸런스 확인용 기본 추천값", "Normal Skirmish\nRecommended baseline for balance checks"), demoEnabled))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Normal);
            }

            if (ImGuiGameUi.GameMenuButton(new Rect(left.x + 24f, btnY + (btnH + btnGap) * 2f, btnW, btnH), L("어려움 스커미시\n압박 테스트용 고강도", "Hard Skirmish\nHigher pressure for stress testing"), demoEnabled))
            {
                StartSkirmishVsAi(SkirmishDifficultyTier.Hard);
            }

            if (ImGuiGameUi.GameMenuButton(new Rect(left.x + 24f, btnY + (btnH + btnGap) * 3f, btnW, 60f), L("공세 변주\n기본 규칙에 더 공격적인 적 패턴 추가", "Aggressive Variant\nNormal rules with a more hostile enemy pattern"), demoEnabled))
            {
                StartSkirmishVariantAggressive(SkirmishDifficultyTier.Normal);
            }

            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 18 : 20;
            GUI.color = ImGuiGameUi.AccentCyan;
            GUI.Label(new Rect(right.x + 24f, right.y + 34f, right.width - 48f, 30f), L("임무 브리프", "Mission Brief"));

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(right.x + 24f, right.y + 78f, right.width - 48f, 42f), DemoPresentationCopy.RoundGoalOneLineKo);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(right.x + 24f, right.y + 138f, right.width - 48f, 64f), DemoPresentationCopy.KeyboardOnlyNoticeKo + "\n\n" + DemoPresentationCopy.AfterMatchExitOneLineKo);

            Rect statCard = new Rect(right.x + 24f, right.y + 232f, right.width - 48f, 104f);
            ImGuiGameUi.DrawPanelFrame(statCard, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, 1f);
            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 10 : 11;
            GUI.color = ImGuiGameUi.ResourceHighlight;
            GUI.Label(new Rect(statCard.x + 10f, statCard.y + 8f, statCard.width - 20f, statCard.height - 16f), MissionDefinition.BuildSkirmishDemoMenuStatsLine());

            if (!demoEnabled)
            {
                Rect warning = new Rect(right.x + 24f, right.yMax - 96f, right.width - 48f, 64f);
                ImGuiGameUi.DrawPanelFrame(warning, new Color(0.18f, 0.1f, 0.1f, 0.92f), ImGuiGameUi.DefeatTint, 1f);
                GUI.skin.label.fontSize = 12;
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(warning, L("미션 카탈로그가 없고 런타임 폴백이 꺼져 있어서 단독 데모를 사용할 수 없습니다.", "Standalone demo is disabled because there is no mission catalog and runtime fallback is turned off."));
                GUI.color = Color.white;
            }
        }

        private void DrawSubmenuHeader(Rect rect, string title, string subtitle)
        {
            ImGuiGameUi.DrawGlassPanel(rect, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);

            Rect backButton = new Rect(rect.x + rect.width - 214f, rect.y + 34f, 166f, 34f);
            if (ImGuiGameUi.GameMenuButton(backButton, L("메인으로 (Esc)", "Back To Main (Esc)")))
            {
                currentLayer = MainMenuLayer.Root;
                menuOpenedUnscaled = Time.unscaledTime;
            }

            GUI.skin.label.fontSize = GameUserSettings.Language == GameLanguage.Korean ? 24 : 28;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 28f, rect.width - 272f, 34f), title);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 72f, rect.width - 272f, 22f), subtitle);

            float sweep = Mathf.Repeat(Time.unscaledTime * 0.18f, 1f);
            ImGuiGameUi.DrawSweepLine(new Rect(rect.x + 16f, rect.y + 14f, rect.width - 32f, 4f), new Color(0.56f, 0.88f, 0.96f, 0.34f), sweep, 64f);
            GUI.color = Color.white;
        }

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
            Rect full = new Rect(0f, 0f, Screen.width, Screen.height);
            ImGuiGameUi.DrawVerticalGradient(full, new Color(0.02f, 0.025f, 0.045f, 1f), new Color(0.005f, 0.008f, 0.016f, 1f), 28);
            ImGuiGameUi.DrawVerticalGradient(new Rect(0f, 0f, Screen.width * 0.48f, Screen.height), new Color(0.05f, 0.08f, 0.14f, 0.24f), new Color(0.01f, 0.03f, 0.06f, 0.02f), 18);
            ImGuiGameUi.DrawFilledRect(new Rect(Screen.width * 0.44f, 0f, 2f, Screen.height), new Color(0.16f, 0.2f, 0.28f, 0.14f));
            ImGuiGameUi.DrawScanLines(full, new Color(0.65f, 0.78f, 0.95f, 0.03f), 26f, 1f);
            ImGuiGameUi.DrawGrid(new Rect(Screen.width * 0.52f, 0f, Screen.width * 0.48f, Screen.height), new Color(0.35f, 0.5f, 0.7f, 0.08f), 64f, 48f, 1f);

            float pulse = 0.045f + Mathf.Sin(Time.unscaledTime * 1.3f) * 0.012f;
            ImGuiGameUi.DrawFilledRect(new Rect(Screen.width * 0.08f, 96f, Screen.width * 0.34f, 1f), new Color(0.42f, 0.76f, 0.88f, pulse));
            ImGuiGameUi.DrawFilledRect(new Rect(Screen.width * 0.6f, Screen.height - 124f, Screen.width * 0.22f, 1f), new Color(0.92f, 0.74f, 0.34f, pulse));
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, 8f), new Color(0.92f, 0.74f, 0.34f, 0.25f));
        }

        private Rect AnimatedRect(Rect rect, float fromOffsetX, float fromOffsetY, float delay)
        {
            float t = Mathf.Clamp01((Time.unscaledTime - menuOpenedUnscaled - delay) / 0.34f);
            t = 1f - Mathf.Pow(1f - t, 3f);
            rect.x += Mathf.Lerp(fromOffsetX, 0f, t);
            rect.y += Mathf.Lerp(fromOffsetY, 0f, t);
            return rect;
        }

        private string BuildMissionProgressDotsLine()
        {
            if (catalog == null || catalog.Count == 0)
            {
                return string.Empty;
            }

            string line = "Progress  ";
            for (int i = 0; i < catalog.Count; i++)
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

                if (cleared && hasBonus && bonusDone)
                {
                    line += "[C+] ";
                }
                else if (cleared)
                {
                    line += "[C] ";
                }
                else
                {
                    line += "[ ] ";
                }
            }

            return line.TrimEnd();
        }

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

            skirmish.ApplySkirmishVariantEnemyPattern("aggressive_push", "skirmish_demo_variant_aggressive_" + suffix, "Aggressive Variant");

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
            CampaignSceneLoadUtility.TryLoadSceneByName(scene, $"Mission: {MissionObjectiveDisplayText.ResolveMissionDisplayName(mission)}");
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

        private static string L(string korean, string english)
        {
            return GameUserSettings.Language == GameLanguage.Korean ? korean : english;
        }
    }
}
