using Game.Campaign.Data;
using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
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

            GUI.skin.label.fontSize = 14;

            if (drawImGuiMission)
            {
                DrawMissionHeaderBlock(ref y, maxW);
            }
            else if (missionContext == null)
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 22f),
                    "Skirmish | Destroy the enemy core | R restart after result");
                y += 22f;
            }

            if (economy != null)
            {
                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(
                    new Rect(x0, y, maxW, 22f),
                    $"Credits {economy.PlayerCredits:0} (+{economy.PlayerTotalIncomePerSecond:0.##}/s) | Enemy {economy.EnemyCredits:0}");
                y += 22f;
            }

            if (playerCore != null)
            {
                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 20f),
                    "1-8 train | Right-click move/attack | T Y U upgrades | V target mode");
                y += 20f;

                GUI.color = ImGuiGameUi.TextTitle;
                GUI.Label(new Rect(x0, y, maxW, 20f), BuildDeckLine());
                y += 20f;

                GUI.color = ImGuiGameUi.TextMuted;
                GUI.Label(
                    new Rect(x0, y, maxW, 20f),
                    $"Queue {playerCore.QueueCount} | F1 help | P / 1 / 2 / 3 time");
            }

            GUI.color = Color.white;

            if (match != null && match.IsFinished && missionContext == null)
            {
                DrawSkirmishResultOverlay();
            }
        }

        private string BuildDeckLine()
        {
            if (playerCore == null)
            {
                return string.Empty;
            }

            string line = "Deck ";
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
                Game.Units.UnitArchetype.Spearman => "Spear",
                Game.Units.UnitArchetype.ShieldInfantry => "Shield",
                Game.Units.UnitArchetype.Rifleman => "Rifle",
                Game.Units.UnitArchetype.Artillery => "Arty",
                Game.Units.UnitArchetype.Fighter => "Fighter",
                Game.Units.UnitArchetype.SpecialWarrior => "Elite",
                Game.Units.UnitArchetype.RoyalGuard => "Guard",
                Game.Units.UnitArchetype.Outrider => "Rider",
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
                ? "Victory | Enemy core destroyed"
                : "Defeat | Your core was destroyed";

            Rect box = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.38f, 420f, 112f);
            ImGuiGameUi.DrawPanelFrame(box, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderAccent, 2f);

            GUI.skin.label.fontSize = 22;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(box.x + 16f, box.y + 18f, box.width - 32f, 34f), msg);
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(box.x + 16f, box.y + 58f, box.width - 32f, 24f), "Press R to restart.");
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
            if (missionContext == null && !showMissionBlock)
            {
                return 118f;
            }

            if (!drawImGuiMission && showMissionBlock)
            {
                return missionContext != null &&
                       missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense
                    ? 144f
                    : 118f;
            }

            if (showMissionBlock && missionContext != null &&
                missionContext.ObjectiveKind == MissionObjectiveKind.SanctuaryDefense)
            {
                return 186f;
            }

            return showMissionBlock ? 164f : 118f;
        }

        private void DrawMissionHeaderBlock(ref float y, float maxW)
        {
            string primary = MissionObjectiveDisplayText.GetPrimaryLine(missionContext.ObjectiveKind);
            string hint = MissionObjectiveDisplayText.GetGameplayHint(missionContext.ObjectiveKind);
            string extra = GetDefenseCountdownLine();

            float blockH = string.IsNullOrEmpty(extra) ? 50f : 68f;
            float x0 = PanelPad;

            ImGuiGameUi.DrawFilledRect(new Rect(x0, y, maxW, 2f), ImGuiGameUi.BorderAccent);

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(x0, y + 6f, maxW, 22f), $"{missionContext.DisplayName} | {primary}");

            GUI.skin.label.fontSize = 13;
            GUI.color = ImGuiGameUi.TextMuted;
            string description = string.IsNullOrEmpty(extra) ? hint : $"{hint}\n{extra}";
            GUI.Label(new Rect(x0, y + 28f, maxW, 40f), description);

            GUI.skin.label.fontSize = 14;
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
                return "Timer starts when the mission begins.";
            }

            float elapsed = Time.time - campaignFlow.GameplayStartTime;
            float remain = Mathf.Max(0f, missionContext.DefenseDurationSeconds - elapsed);
            return $"Time left {remain:0}s / {missionContext.DefenseDurationSeconds:0}s";
        }
    }
}
