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

            const float h = 52f;
            float w = Mathf.Min(480f, Screen.width - 24f);
            float x = (Screen.width - w) * 0.5f;
            float bottomY = Screen.height - h - 18f;

            if (database != null &&
                playerCore.TryGetNextProductionPreview(out UnitArchetype nextArch, out float secLeft))
            {
                UnitDefinition nextDef = database.GetDefinition(nextArch);
                string unitName = nextDef != null ? nextDef.DisplayName : nextArch.ToString();
                string prodLine = secLeft > 0.05f
                    ? $"다음 유닛: {unitName}  ({secLeft:0.0}초)"
                    : $"다음 유닛: {unitName}  (대기)";

                GUI.skin.label.fontSize = 14;
                GUI.color = ImGuiGameUi.AccentGold;
                GUI.Label(new Rect(x, bottomY - 22f, w, 20f), prodLine);
                GUI.color = Color.white;
            }

            Rect r = new Rect(x, bottomY, w, h);
            ImGuiGameUi.DrawPanelFrame(r, ImGuiGameUi.PanelBgDeep, ImGuiGameUi.BorderCool, 1.5f);

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

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 20f), "유닛을 선택하면 상세 스탯이 표시됩니다.");
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(r.x + 10f, r.y + 26f, r.width - 20f, 22f), hpLine);
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

            GUI.skin.label.fontSize = 15;
            GUI.color = ImGuiGameUi.AccentGold;
            string title = def != null ? def.DisplayName : unit.Archetype.ToString();
            GUI.Label(new Rect(r.x + 10f, r.y + 6f, r.width - 20f, 22f), title);

            GUI.color = ImGuiGameUi.TextTitle;
            string hp = uh != null ? $"체력  {uh.CurrentHealth:0} / {uh.MaxHealth:0}" : "체력 —";
            string infoLine = economy != null
                ? $"{hp}   ·   재훈련 비용(참고)  {cost}   ·   자원  {economy.PlayerCredits:0}"
                : $"{hp}   ·   재훈련 비용(참고)  {cost}";

            GUI.Label(new Rect(r.x + 10f, r.y + 26f, r.width - 20f, 22f), infoLine);
            GUI.color = Color.white;
        }
    }
}
