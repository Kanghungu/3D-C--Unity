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
    /// Î∏åÎ¶¨?????ÑÌà¨ ??Í≤∞Í≥º(?Ä?? ??Î©îÎâ¥/?¨Ïãú?? BA_Systems ??Î∂ôÏù∏??
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

        /// <summary>Î∏åÎ¶¨???ÑÏ≤¥?îÎ©¥ Ï§???ÎØ∏ÎãàÎßµ¬∑F1 ??Ï∞®Îã®??/summary>
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

        /// <summary>?ëÏ†Ñ Î∏åÎ¶¨?????ÅÎã® Î™©Ìëú Î∞?+ Ï§ëÏïô ?∞Ïù¥???®ÎÑê.</summary>
        private void DrawBriefingScreen(PersistentGameCore core)
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            string text = core != null && !string.IsNullOrEmpty(mission.BriefingDialogueId)
                ? core.TryGetDialogue(mission.BriefingDialogueId)
                : null;

            if (string.IsNullOrEmpty(text))
            {
                text = $"??{mission.DisplayName} ??n\n?ëÏ†Ñ Î™©Ìëú: {MissionObjectiveDisplayText.GetPrimaryLine(mission.ObjectiveKind)}\n\n?¥Îã®??Î™∞ÏïÑ?¥Í≥† ÍµêÎã®???ªÏùÑ ???ÖÏóê ?∏Ïö∞??ãú??";
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
                "Space ¬∑ Enter ¬∑ ?¥Î¶≠ ???ëÏ†Ñ Í∞úÏãú");
            GUI.color = Color.white;

            DrawBriefingTopObjectiveBar(mission);
        }

        /// <summary>?πÌå® ?§Î≤Ñ?àÏù¥ ??Ï§ëÏïô Ïπ¥Îìú + ?§Ì???Î≤ÑÌäº.</summary>
        private void DrawResultScreen(PersistentGameCore core)
        {
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);

            bool won = match.State == BattleAcesMatchController.MatchState.Victory;
            string did = won ? mission.VictoryDialogueId : mission.DefeatDialogueId;
            string body = core != null && !string.IsNullOrEmpty(did) ? core.TryGetDialogue(did) : null;
            if (string.IsNullOrEmpty(body))
            {
                body = won ? "?πÎ¶¨?òÏ??? ?±Ïä§?¨Ïö¥ ?òÏ?Í∞Ä ???ÖÏùÑ ÎπÑÏ∂ò??" : "?®Î∞∞?òÏ??? Í∑∏Îü¨???†Ïïô?Ä Í∫ºÏ?ÏßÄ ?äÎäî??";
            }

            float cardW = Mathf.Min(700f, Screen.width - 48f);
            float cardH = Mathf.Min(478f, Screen.height - 80f);
            Rect card = new Rect((Screen.width - cardW) * 0.5f, (Screen.height - cardH) * 0.5f, cardW, cardH);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, won ? ImGuiGameUi.BorderAccent : ImGuiGameUi.BorderCool, 2f);

            string title = won ? "?ÑÏà† ?πÎ¶¨" : "?ÑÏà† ?®Î∞∞";
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
                $"?åÎ†à???úÍ∞Ñ  {resultPlaySeconds:0.0}Ï¥?  ¬∑   Ï¢ÖÎ£å ???êÏõê  {resultPlayerCredits}";
            GUI.Label(new Rect(card.x + 24f, card.y + bodyTop + bodyH + 6f, card.width - 48f, 28f), statsLine);

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 24f, card.y + bodyTop + bodyH + 34f, card.width - 48f, 22f), "Press R to retry this mission.");
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 14;

            float btnY = card.yMax - 72f;
            float btnW = (card.width - 64f) * 0.5f;
            Rect retry = new Rect(card.x + 24f, btnY, btnW - 8f, 48f);
            Rect menu = new Rect(card.x + 32f + btnW, btnY, btnW - 8f, 48f);

            if (ImGuiGameUi.GameMenuButton(retry, "Retry Mission"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().path);
            }

            if (ImGuiGameUi.GameMenuButton(menu, "Campaign Menu"))
            {
                Time.timeScale = 1f;
                string menuScene = core != null ? core.CampaignMenuSceneName : "CampaignMenu";
                CampaignSceneLoadUtility.TryLoadSceneByName(menuScene, "Ï∫†Ìéò??Í≤∞Í≥º ??Î©îÎâ¥");
            }
        }

        /// <summary>Battle Aces ?πÌå® ?îÎ©¥ ??Ï§???MatchEndReason ?îÏïΩ.</summary>
        private static string FormatMatchEndReasonLine(BattleAcesMatchController.MatchEndReason reason, bool won)
        {
            if (won)
            {
                return reason switch
                {
                    BattleAcesMatchController.MatchEndReason.VictoryEnemyCoreDestroyed => "Ï¢ÖÎ£å ?¨Ïú†: ??ÍµêÎã® ÏΩîÏñ¥ Í≤©Ìåå",
                    BattleAcesMatchController.MatchEndReason.VictoryMissionObjective => "Ï¢ÖÎ£å ?¨Ïú†: ?ëÏ†Ñ Î™©Ìëú ?¨ÏÑ±",
                    _ => "Ï¢ÖÎ£å ?¨Ïú†: ?ÑÏà† ?πÎ¶¨"
                };
            }

            return reason switch
            {
                BattleAcesMatchController.MatchEndReason.DefeatPlayerCoreDestroyed => "Ï¢ÖÎ£å ?¨Ïú†: ?ÑÍµ∞ ÏΩîÏñ¥ Î∂ïÍ¥¥",
                BattleAcesMatchController.MatchEndReason.DefeatRelicOrKeyObjectiveLost => "Ï¢ÖÎ£å ?¨Ïú†: ?±Ïú†Î¨??µÏã¨ Î™©Ìëú ?êÏã§",
                BattleAcesMatchController.MatchEndReason.DefeatMissionFailed => "Ï¢ÖÎ£å ?¨Ïú†: ?ëÏ†Ñ ?§Ìå®",
                _ => "Ï¢ÖÎ£å ?¨Ïú†: ?ÑÏà† ?®Î∞∞"
            };
        }

        /// <summary>ÏµúÏÉÅ??Î™©Ìëú Î∞????§Î•∏ UIÎ≥¥Îã§ ?ÑÏóê Í∑∏Î¶º.</summary>
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
                $"?ëÏ†Ñ Î™©Ìëú ??{MissionObjectiveDisplayText.GetPrimaryLine(m.ObjectiveKind)}");
            GUI.skin.label.fontSize = prevSize;
            GUI.color = Color.white;
        }
    }
}
