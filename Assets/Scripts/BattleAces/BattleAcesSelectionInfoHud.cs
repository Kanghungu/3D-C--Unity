using Game.Prototype;
using Game.Selection;
using Game.Settings;
using Game.UI;
using Game.Units;
using UnityEngine;

namespace Game.BattleAces
{
    public sealed class BattleAcesSelectionInfoHud : MonoBehaviour
    {
        private BattleAcesCore playerCore;
        private BattleAcesEconomy economy;
        private PrototypeGameDatabase database;

        private bool IsKorean => GameUserSettings.Language == GameLanguage.Korean;

        public void Bind(BattleAcesCore core, BattleAcesEconomy eco, PrototypeGameDatabase db)
        {
            playerCore = core;
            economy = eco;
            database = db;
        }

        private void OnGUI()
        {
            if (playerCore == null)
            {
                return;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController matchCtrl) && matchCtrl.IsFinished)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();

            const float h = 74f;
            float w = Mathf.Min(460f, Screen.width - 24f);
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 16f;
            Rect panel = new Rect(x, y, w, h);

            ImGuiGameUi.DrawGlassPanel(panel, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
            ImGuiGameUi.DrawFilledRect(
                new Rect(panel.x, panel.y, panel.width, 20f),
                new Color(ImGuiGameUi.AccentCyan.r, ImGuiGameUi.AccentCyan.g, ImGuiGameUi.AccentCyan.b, 0.08f));

            PrototypeSelectionController selection = PrototypeSelectionController.Instance;
            if (selection != null && selection.SelectedUnits.Count == 1)
            {
                DrawSingleUnit(panel, selection.SelectedUnits[0]);
            }
            else
            {
                DrawCoreOnly(panel);
            }

            ImGuiGameUi.EndScaledGui();
        }

        private void DrawCoreOnly(Rect panel)
        {
            UnitHealth health = playerCore.Health;
            string hpLine = health != null
                ? (IsKorean
                    ? $"아군 코어  {health.CurrentHealth:0} / {health.MaxHealth:0}"
                    : $"Command Core  {health.CurrentHealth:0} / {health.MaxHealth:0}")
                : (IsKorean ? "아군 코어" : "Command Core");

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 4f, panel.width - 24f, 14f),
                IsKorean ? "유닛 선택 시 상세 정보 표시" : "Detailed unit info appears when one unit is selected");

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 24f, panel.width - 24f, 22f), hpLine);

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 49f, panel.width - 24f, 18f),
                IsKorean
                    ? "상단 목표 바, 우하단 지도, 조작은 F1에서 확인"
                    : "Use the top objective bar, lower-right map, and F1 guide for control help");
        }

        private void DrawSingleUnit(Rect panel, SelectableUnit unit)
        {
            if (unit == null || database == null)
            {
                DrawCoreOnly(panel);
                return;
            }

            UnitDefinition definition = database.GetDefinition(unit.Archetype);
            UnitHealth health = unit.GetComponent<UnitHealth>();
            int cost = definition != null ? BattleAcesEconomy.GetTrainCost(definition) : 0;
            string title = definition != null ? definition.DisplayName : unit.Archetype.ToString();
            string hpText = health != null
                ? $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}"
                : "HP --";

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 4f, panel.width - 24f, 14f),
                IsKorean ? "선택된 전력" : "Selected unit");

            GUI.skin.label.fontSize = 16;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 22f, panel.width - 24f, 22f), title);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 46f, panel.width - 24f, 18f),
                economy != null
                    ? (IsKorean
                        ? $"{hpText} · 생산비 {cost} · 보유 자원 {economy.PlayerCredits:0}"
                        : $"{hpText} · Cost {cost} · Credits {economy.PlayerCredits:0}")
                    : (IsKorean ? $"{hpText} · 생산비 {cost}" : $"{hpText} · Cost {cost}"));
        }
    }
}
