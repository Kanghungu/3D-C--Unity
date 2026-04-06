using Game.Audio;
using Game.Campaign;
using Game.Campaign.Core;
using Game.Campaign.Data;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.BattleAces
{
    /// <summary>
    /// 브리핑 → 전투 → 결과(대사) → 메뉴/재시작. BA_Systems 에 붙인다.
    /// </summary>
    public sealed class CampaignBattleFlow : MonoBehaviour
    {
        private MissionDefinition mission;
        private BattleAcesMatchController match;
        private BattleAcesEconomy economyRef;
        private bool briefingActive;
        private bool gameplayStarted;
        private float gameplayStartTime;
        private float gameplayStartUnscaledTime;
        private bool showResultOverlay;
        private float resultPlaySeconds;
        private int resultPlayerCredits;

        public bool IsGameplayStarted => gameplayStarted;
        public float GameplayStartTime => gameplayStartTime;

        /// <summary>브리핑 전체화면 중 — 미니맵·F1 등 차단용</summary>
        public bool IsBriefingBlocking => mission != null && briefingActive;

        public static CampaignBattleFlow Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }

        public void Initialize(MissionDefinition m, BattleAcesMatchController mc, BattleAcesEconomy economy = null)
        {
            mission = m;
            match = mc;
            economyRef = economy;
            if (match != null)
            {
                match.MatchEnded += OnMatchEnded;
            }

            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.enabled = false;
                rtc.SetControlEnabled(false);
                rtc.ForcePauseGameplay();
            }
            else
            {
                Time.timeScale = 0f;
            }

            briefingActive = mission != null;
            if (!briefingActive)
            {
                StartGameplayClock();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (match != null)
            {
                match.MatchEnded -= OnMatchEnded;
            }

            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!briefingActive || mission == null)
            {
                return;
            }

            Keyboard kb = Keyboard.current;
            bool confirmKey = kb != null &&
                              (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame);
            UnityEngine.InputSystem.Mouse pointer = UnityEngine.InputSystem.Mouse.current;
            bool confirmClick = pointer != null && pointer.leftButton.wasPressedThisFrame;

            if (confirmKey || confirmClick)
            {
                briefingActive = false;
                StartGameplayClock();
            }
        }

        private void StartGameplayClock()
        {
            gameplayStarted = true;
            gameplayStartTime = Time.time;
            gameplayStartUnscaledTime = Time.unscaledTime;
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.enabled = true;
                rtc.SetControlEnabled(true);
                rtc.ResumeFromPause(rtc.CurrentSpeedStep);
            }
            else
            {
                Time.timeScale = 1f;
            }

            ProceduralAudioUtility.PlayUiConfirm();
        }

        private void OnMatchEnded(BattleAcesMatchController.MatchState st)
        {
            resultPlaySeconds = Mathf.Max(0f, Time.unscaledTime - gameplayStartUnscaledTime);
            if (economyRef != null)
            {
                resultPlayerCredits = Mathf.RoundToInt(economyRef.PlayerCredits);
            }

            ProceduralAudioUtility.PlayResultSting(st == BattleAcesMatchController.MatchState.Victory);
            showResultOverlay = true;
            RtsTimeControl rtc = RtsTimeControl.Instance;
            if (rtc != null)
            {
                rtc.SetControlEnabled(false);
            }

            if (st != BattleAcesMatchController.MatchState.Victory || mission == null)
            {
                return;
            }

            PersistentGameCore core = PersistentGameCore.Instance;
            int orderIndex = core != null && core.PendingMissionOrderIndex >= 0
                ? core.PendingMissionOrderIndex
                : mission.CampaignSortOrder;

            CampaignProgressStorage.RegisterMissionWin(orderIndex, mission.MissionId);
        }

        private void OnGUI()
        {
            PersistentGameCore core = PersistentGameCore.Instance;

            if (mission != null && briefingActive)
            {
                DrawBriefingScreen(core);
                return;
            }

            if (mission == null || match == null || !showResultOverlay || !match.IsFinished)
            {
                return;
            }

            DrawResultScreen(core);
        }

        /// <summary>작전 브리핑 — 상단 목표 바 + 중앙 데이터 패널.</summary>
        private void DrawBriefingScreen(PersistentGameCore core)
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            string text = core != null && !string.IsNullOrEmpty(mission.BriefingDialogueId)
                ? core.TryGetDialogue(mission.BriefingDialogueId)
                : null;

            if (string.IsNullOrEmpty(text))
            {
                text = $"《 {mission.DisplayName} 》\n\n작전 목표: {MissionObjectiveDisplayText.GetPrimaryLine(mission.ObjectiveKind)}\n\n이단을 몰아내고 교단의 뜻을 이 땅에 세우십시오.";
            }

            float cardW = Mathf.Min(760f, Screen.width - 48f);
            float cardH = Mathf.Min(400f, Screen.height - 160f);
            float cardX = (Screen.width - cardW) * 0.5f;
            float cardY = 72f;
            Rect card = new Rect(cardX, cardY, cardW, cardH);

            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);

            GUI.skin.label.fontSize = 17;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(card.x + 20f, card.y + 16f, card.width - 40f, card.height - 56f), text);
            GUI.color = Color.white;

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 20f, card.yMax - 44f, card.width - 40f, 32f),
                "Space · Enter · 클릭 — 작전 개시");
            GUI.color = Color.white;

            DrawBriefingTopObjectiveBar(mission);
        }

        /// <summary>승패 오버레이 — 중앙 카드 + 스타일 버튼.</summary>
        private void DrawResultScreen(PersistentGameCore core)
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            bool won = match.State == BattleAcesMatchController.MatchState.Victory;
            string did = won ? mission.VictoryDialogueId : mission.DefeatDialogueId;
            string body = core != null && !string.IsNullOrEmpty(did) ? core.TryGetDialogue(did) : null;
            if (string.IsNullOrEmpty(body))
            {
                body = won ? "승리하였다. 성스러운 의지가 이 땅을 비춘다." : "패배하였다. 그러나 신앙은 꺼지지 않는다.";
            }

            float cardW = Mathf.Min(700f, Screen.width - 48f);
            float cardH = Mathf.Min(478f, Screen.height - 80f);
            Rect card = new Rect((Screen.width - cardW) * 0.5f, (Screen.height - cardH) * 0.5f, cardW, cardH);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, won ? ImGuiGameUi.BorderAccent : ImGuiGameUi.BorderCool, 2f);

            string title = won ? "전술 승리" : "전술 패배";
            GUI.skin.label.fontSize = 26;
            GUI.color = won ? ImGuiGameUi.VictoryTint : ImGuiGameUi.DefeatTint;
            GUI.Label(new Rect(card.x + 24f, card.y + 18f, card.width - 48f, 40f), title);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            string endReasonLine = match != null
                ? FormatMatchEndReasonLine(match.LastEndReason, won)
                : string.Empty;
            if (!string.IsNullOrEmpty(endReasonLine))
            {
                GUI.Label(new Rect(card.x + 24f, card.y + 52f, card.width - 48f, 22f), endReasonLine);
            }

            GUI.color = ImGuiGameUi.TextTitle;
            GUI.skin.label.fontSize = 17;
            float bodyH = card.height - 200f;
            float bodyTop = string.IsNullOrEmpty(endReasonLine) ? 64f : 78f;
            GUI.Label(new Rect(card.x + 24f, card.y + bodyTop, card.width - 48f, bodyH), body);

            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextMuted;
            string statsLine =
                $"플레이 시간  {resultPlaySeconds:0.0}초   ·   종료 시 자원  {resultPlayerCredits}";
            GUI.Label(new Rect(card.x + 24f, card.y + bodyTop + bodyH + 6f, card.width - 48f, 28f), statsLine);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 24f, card.y + bodyTop + bodyH + 34f, card.width - 48f, 22f), "R 키 — 같은 미션 즉시 재시작");
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 14;

            float btnY = card.yMax - 72f;
            float btnW = (card.width - 64f) * 0.5f;
            Rect retry = new Rect(card.x + 24f, btnY, btnW - 8f, 48f);
            Rect menu = new Rect(card.x + 32f + btnW, btnY, btnW - 8f, 48f);

            if (ImGuiGameUi.GameMenuButton(retry, "같은 미션 재시작"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().path);
            }

            if (ImGuiGameUi.GameMenuButton(menu, "캠페인 메뉴로"))
            {
                Time.timeScale = 1f;
                string menuScene = core != null ? core.CampaignMenuSceneName : "CampaignMenu";
                CampaignSceneLoadUtility.TryLoadSceneByName(menuScene, "캠페인 결과 → 메뉴");
            }
        }

        /// <summary>Battle Aces 승패 화면 한 줄 — MatchEndReason 요약.</summary>
        private static string FormatMatchEndReasonLine(BattleAcesMatchController.MatchEndReason reason, bool won)
        {
            if (won)
            {
                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.VictoryEnemyCoreDestroyed => "종료 사유: 적 교단 코어 격파",
                    BattleAcesMatchController.MatchEndReason.VictoryMissionObjective => "종료 사유: 작전 목표 달성",
                    _ => "종료 사유: 전술 승리"
                };
            }

            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "종료 사유: 아군 코어 붕괴",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "종료 사유: 성유물/핵심 목표 손실",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "종료 사유: 작전 실패",
                _ => "종료 사유: 전술 패배"
            };
        }

        /// <summary>최상단 목표 바 — 다른 UI보다 위에 그림.</summary>
        private static void DrawBriefingTopObjectiveBar(MissionDefinition m)
        {
            if (m == null)
            {
                return;
            }

            const float barH = 54f;
            ImGuiGameUi.DrawTopAccentBar(barH, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderAccent);

            int prevSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 19;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(
                new Rect(22f, 14f, Screen.width - 44f, 36f),
                $"작전 목표 — {MissionObjectiveDisplayText.GetPrimaryLine(m.ObjectiveKind)}");
            GUI.skin.label.fontSize = prevSize;
            GUI.color = Color.white;
        }
    }
}
