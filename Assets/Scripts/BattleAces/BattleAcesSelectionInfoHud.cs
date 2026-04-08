using Game.Prototype;
using Game.Selection;
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

            ImGuiGameUi.BeginScaledGui();

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController matchCtrl) && matchCtrl.IsFinished)
            {
                ImGuiGameUi.EndScaledGui();
                return;
            }

            const float h = 54f;
            float w = Mathf.Min(400f, Screen.width - 24f);
            float x = (Screen.width - w) * 0.5f;
            float bottomY = Screen.height - h - 14f;
            // 생산·큐 요약은 BattleAcesHudOverlay 왼쪽 패널에만 표시(한 화면 중복 방지)

            Rect r = new Rect(x, bottomY, w, h);
            ImGuiGameUi.DrawHudCardWithLeftStripe(r, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, ImGuiGameUi.HudStripeTactical, 3f);

            PrototypeSelectionController sel = PrototypeSelectionController.Instance;
            if (sel != null && sel.SelectedUnits.Count == 1)
            {
                DrawSingleUnit(r, sel.SelectedUnits[0]);
                ImGuiGameUi.EndScaledGui();
                return;
            }

            DrawCoreOnly(r);
            ImGuiGameUi.EndScaledGui();
        }

        private void DrawCoreOnly(Rect r)
        {
            UnitHealth h = playerCore.Health;
            string hpLine = h != null
                ? $"아군 코어  {h.CurrentHealth:0} / {h.MaxHealth:0}"
                : "아군 코어 —";

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(r.x + 10f, r.y + 4f, r.width - 20f, 15f), "유닛 선택 시 상세 표시");
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(r.x + 10f, r.y + 20f, r.width - 20f, 20f), hpLine);
            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(r.x + 10f, r.y + 38f, r.width - 20f, 14f),
                "상단 목표 바 · 우하단 지도 · 조작은 F1");
            GUI.color = Color.white;
        }

        private void DrawSingleUnit(Rect r, SelectableUnit unit)
        {
            if (unit == null || database == null)
            {
                return;
            }

            UnitDefinition def = database.GetDefinition(unit.Archetype);
            UnitHealth uh = unit.GetComponent<UnitHealth>();
            int cost = BattleAcesEconomy.GetTrainCost(def);

            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.AccentGold;
            string title = def != null ? def.DisplayName : unit.Archetype.ToString();
            GUI.Label(new Rect(r.x + 10f, r.y + 4f, r.width - 20f, 20f), title);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            string hp = uh != null ? $"HP {uh.CurrentHealth:0}/{uh.MaxHealth:0}" : "HP —";
            string infoLine = economy != null
                ? $"{hp}  ·  재훈련 {cost}  ·  크레딧 {economy.PlayerCredits:0}"
                : $"{hp}  ·  재훈련 {cost}";

            GUI.Label(new Rect(r.x + 10f, r.y + 20f, r.width - 20f, 20f), infoLine);
            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(r.x + 10f, r.y + 38f, r.width - 20f, 14f),
                "우클릭 명령 · 생산/자원은 왼쪽 HUD · F1 전체 조작");
            GUI.color = Color.white;
        }
    }
}
