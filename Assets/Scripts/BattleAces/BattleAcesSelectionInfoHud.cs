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

            if (BattleAcesHudCaptureMode.SuppressCombatChromeForScreenshot)
            {
                return;
            }

            ImGuiGameUi.BeginScaledGui();
            Color previousGuiColor = GUI.color;
            int previousLabelFontSize = GUI.skin.label.fontSize;

            bool compactH = Screen.height <= 720;
            float panelWidth = Mathf.Min(620f, Screen.width - 24f);
            // If five chips get too cramped, split them into two rows on smaller layouts.
            float innerForChips = panelWidth - 24f;
            bool narrowFiveChips = (innerForChips - 12f) / 5f < 76f;

            PrototypeSelectionController selection = PrototypeSelectionController.Instance;
            bool singleUnit = selection != null && selection.SelectedUnits.Count == 1;
            bool tallChipBlock = singleUnit && narrowFiveChips;
            const float combatReadoutExtraHeight = 22f;
            float panelHeight = compactH ? (tallChipBlock ? 128f : 110f) : (tallChipBlock ? 136f : 118f);
            if (singleUnit)
            {
                panelHeight += combatReadoutExtraHeight;
            }

            float x = (Screen.width - panelWidth) * 0.5f;
            // Leave extra room so the panel does not fight the minimap on low-height screens.
            float bottomPad = compactH ? 24f : 16f;
            float y = Screen.height - panelHeight - bottomPad;
            Rect panel = new Rect(x, y, panelWidth, panelHeight);

            ImGuiGameUi.DrawGlassPanel(panel, ImGuiGameUi.PanelBgHud, ImGuiGameUi.BorderCool, ImGuiGameUi.AccentCyan);
            ImGuiGameUi.DrawFilledRect(
                new Rect(panel.x, panel.y, panel.width, 22f),
                new Color(ImGuiGameUi.AccentCyan.r, ImGuiGameUi.AccentCyan.g, ImGuiGameUi.AccentCyan.b, 0.08f));

            if (selection != null && selection.SelectedUnits.Count == 1)
            {
                DrawSingleUnit(panel, selection.SelectedUnits[0], narrowFiveChips);
            }
            else
            {
                DrawCoreOnly(panel);
            }

            GUI.skin.label.fontSize = previousLabelFontSize;
            GUI.color = previousGuiColor;
            ImGuiGameUi.EndScaledGui();
        }

        private void DrawCoreOnly(Rect panel)
        {
            UnitHealth health = playerCore.Health;
            float health01 = health != null && health.MaxHealth > 0.01f ? health.Normalized : 0f;
            BattleAcesReadability.CoreHpBand band = health != null
                ? BattleAcesReadability.GetPlayerCoreBand(health01)
                : BattleAcesReadability.CoreHpBand.Ok;
            Color hpColor = BattleAcesReadability.GetPlayerCoreHudHpColor(band);

            string title = IsKorean ? "지휘 코어" : "Command Core";
            string hpLine = health != null
                ? $"{title}  {health.CurrentHealth:0} / {health.MaxHealth:0}"
                : title;

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 4f, panel.width - 24f, 14f),
                IsKorean ? "유닛 1기를 선택하면 상세 전투 정보가 표시됩니다" : "Select one unit to inspect detailed combat data");

            GUI.skin.label.fontSize = 18;
            GUI.color = hpColor;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 24f, panel.width - 24f, 24f), hpLine);

            ImGuiGameUi.DrawProgressBar(
                new Rect(panel.x + 12f, panel.y + 52f, panel.width - 24f, 12f),
                health01,
                new Color(0.05f, 0.06f, 0.08f, 0.94f),
                hpColor,
                ImGuiGameUi.BorderCool);

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 72f, panel.width - 24f, 18f),
                IsKorean
                    ? "상단 목표 바, 전술 지도, F1 가이드를 함께 보면서 전열을 관리하십시오"
                    : "Use the top objective strip, tactical map, and F1 guide together while managing the frontline");

            if (economy != null)
            {
                DrawStatChip(
                    new Rect(panel.x + 12f, panel.y + 92f, 138f, 18f),
                    IsKorean ? "자원" : "Credits",
                    $"{economy.PlayerCredits:0}",
                    ImGuiGameUi.AccentGold);
                DrawStatChip(
                    new Rect(panel.x + 156f, panel.y + 92f, 154f, 18f),
                    IsKorean ? "수입" : "Income",
                    $"+{economy.PlayerTotalIncomePerSecond:0.#}/s",
                    ImGuiGameUi.AccentCyan);
                DrawStatChip(
                    new Rect(panel.x + 316f, panel.y + 92f, 138f, 18f),
                    IsKorean ? "적 비축" : "Enemy",
                    $"{economy.EnemyCredits:0}",
                    Color.Lerp(ImGuiGameUi.AccentGold, ImGuiGameUi.AccentCyan, 0.2f));
            }
        }

        private void DrawSingleUnit(Rect panel, SelectableUnit unit, bool narrowFiveChips)
        {
            if (unit == null || database == null)
            {
                DrawCoreOnly(panel);
                return;
            }

            UnitDefinition definition = database.GetDefinition(unit.Archetype);
            UnitHealth health = unit.GetComponent<UnitHealth>();
            int cost = definition != null ? BattleAcesEconomy.GetTrainCost(definition) : 0;
            string title = definition != null
                ? definition.DisplayName
                : UnitDefinition.ResolveDisplayName(unit.Archetype);
            float hp01 = health != null && health.MaxHealth > 0.01f ? health.Normalized : 0f;
            string hpText = health != null
                ? $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}"
                : "HP --";

            GUI.skin.label.fontSize = 10;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 4f, panel.width - 24f, 14f),
                IsKorean ? "선택 유닛" : "Selected unit");

            GUI.skin.label.fontSize = 18;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 22f, panel.width - 24f, 22f), title);

            ImGuiGameUi.DrawProgressBar(
                new Rect(panel.x + 12f, panel.y + 48f, panel.width - 24f, 10f),
                hp01,
                new Color(0.05f, 0.06f, 0.08f, 0.94f),
                Color.Lerp(ImGuiGameUi.AccentCyan, ImGuiGameUi.AccentGold, 1f - hp01),
                ImGuiGameUi.BorderCool);

            GUI.skin.label.fontSize = 12;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 62f, panel.width - 24f, 18f),
                economy != null
                    ? (IsKorean
                        ? $"{hpText} · 생산비 {cost} · 보유 자원 {economy.PlayerCredits:0}"
                        : $"{hpText} · Cost {cost} · Credits {economy.PlayerCredits:0}")
                    : (IsKorean ? $"{hpText} · 생산비 {cost}" : $"{hpText} · Cost {cost}"));

            DrawCombatReadoutLine(panel, unit);

            float chipFirstRowTop = panel.yMax - (narrowFiveChips ? 49f : 28f);
            LayoutSingleUnitStatChips(panel, definition, unit, chipFirstRowTop);
        }

        /// <summary>선택 유닛 교전·최근 피해 스냅샷</summary>
        private void DrawCombatReadoutLine(Rect panel, SelectableUnit unit)
        {
            BattleAcesUnitCombatReadout readout = unit.GetComponent<BattleAcesUnitCombatReadout>();
            UnitCombat combat = unit.GetComponent<UnitCombat>();
            string targetLabel = IsKorean ? "표적 없음" : "No target";
            SelectableUnit hostile = null;
            if (combat != null && combat.CurrentTarget != null && combat.CurrentTarget.IsAlive)
            {
                hostile = combat.CurrentTarget.GetComponent<SelectableUnit>();
                if (hostile != null && database != null)
                {
                    UnitDefinition td = database.GetDefinition(hostile.Archetype);
                    targetLabel = td != null
                        ? td.DisplayName
                        : UnitDefinition.ResolveDisplayName(hostile.Archetype);
                }
                else
                {
                    targetLabel = IsKorean ? "적" : "Hostile";
                }
            }

            string engage = readout != null && readout.IsEngagingHostile()
                ? (IsKorean ? "교전" : "Engaged")
                : (IsKorean ? "대기" : "Idle");

            string dpsPart = string.Empty;
            if (readout != null && readout.TryGetRecentDamagePerSecond(out float dps) && dps > 0.05f)
            {
                dpsPart = IsKorean
                    ? $" · 최근 피해 {dps:0.#}/s"
                    : $" · Recent {dps:0.#}/s";
            }

            // 표적이 가하는 피해 기준 상성(받는 피해 배율) — 기호만
            string trianglePart = string.Empty;
            if (hostile != null)
            {
                bool enemyProjectile = hostile.Definition != null && hostile.Definition.UsesProjectile;
                CombatTriangleRules.IncomingDamageBand band = CombatTriangleRules.GetIncomingDamageBand(
                    hostile.Archetype,
                    unit.Archetype,
                    enemyProjectile);
                trianglePart = band switch
                {
                    CombatTriangleRules.IncomingDamageBand.Reduced => IsKorean ? " · 상성 유리" : " · favor",
                    CombatTriangleRules.IncomingDamageBand.Increased => IsKorean ? " · 상성 불리" : " · weak",
                    _ => string.Empty,
                };
            }

            GUI.skin.label.fontSize = 11;
            GUI.color = new Color(ImGuiGameUi.TextTitle.r, ImGuiGameUi.TextTitle.g, ImGuiGameUi.TextTitle.b, 0.92f);
            string line = IsKorean
                ? $"{engage} · 표적 {targetLabel}{trianglePart}{dpsPart}"
                : $"{engage} · Target {targetLabel}{trianglePart}{dpsPart}";
            GUI.Label(new Rect(panel.x + 12f, panel.y + 80f, panel.width - 24f, 18f), line);
        }

        /// <summary>Uses a 3+2 layout on narrow screens so chips stay readable at 720p.</summary>
        private void LayoutSingleUnitStatChips(Rect panel, UnitDefinition definition, SelectableUnit unit, float chipRowTopY)
        {
            const float chipH = 18f;
            const float gap = 3f;
            float innerX = panel.x + 12f;
            float innerW = panel.width - 24f;
            bool useTwoRows = (innerW - gap * 4f) / 5f < 76f;

            if (!useTwoRows)
            {
                float chipW = (innerW - gap * 4f) / 5f;
                float cy = chipRowTopY;
                float cx = innerX;
                DrawStatChip(new Rect(cx, cy, chipW, chipH), "DMG", definition != null ? $"{definition.AttackDamage:0}" : "--", ImGuiGameUi.AccentGold);
                cx += chipW + gap;
                DrawStatChip(new Rect(cx, cy, chipW, chipH), "RNG", definition != null ? $"{definition.AttackRange:0.#}" : "--", ImGuiGameUi.AccentCyan);
                cx += chipW + gap;
                DrawStatChip(
                    new Rect(cx, cy, chipW, chipH),
                    "SPD",
                    definition != null ? $"{definition.MoveSpeed:0.#}" : "--",
                    Color.Lerp(ImGuiGameUi.AccentCyan, Color.white, 0.2f));
                cx += chipW + gap;
                DrawStatChip(
                    new Rect(cx, cy, chipW, chipH),
                    IsKorean ? "유형" : "Type",
                    definition != null
                        ? (definition.IsFlying ? (IsKorean ? "비행" : "Air") : (IsKorean ? "지상" : "Ground"))
                        : "--",
                    Color.Lerp(ImGuiGameUi.AccentGold, ImGuiGameUi.AccentCyan, 0.24f));
                cx += chipW + gap;
                DrawStatChip(new Rect(cx, cy, chipW, chipH), IsKorean ? "명령" : "Order", unit.OrderLabel, ImGuiGameUi.AccentGold);
                return;
            }

            float w3 = (innerW - gap * 2f) / 3f;
            float y0 = chipRowTopY;
            float cx0 = innerX;
            DrawStatChip(new Rect(cx0, y0, w3, chipH), "DMG", definition != null ? $"{definition.AttackDamage:0}" : "--", ImGuiGameUi.AccentGold);
            cx0 += w3 + gap;
            DrawStatChip(new Rect(cx0, y0, w3, chipH), "RNG", definition != null ? $"{definition.AttackRange:0.#}" : "--", ImGuiGameUi.AccentCyan);
            cx0 += w3 + gap;
            DrawStatChip(
                new Rect(cx0, y0, w3, chipH),
                "SPD",
                definition != null ? $"{definition.MoveSpeed:0.#}" : "--",
                Color.Lerp(ImGuiGameUi.AccentCyan, Color.white, 0.2f));

            float y1 = y0 + chipH + gap;
            float w2 = (innerW - gap) / 2f;
            DrawStatChip(
                new Rect(innerX, y1, w2, chipH),
                IsKorean ? "유형" : "Type",
                definition != null
                    ? (definition.IsFlying ? (IsKorean ? "비행" : "Air") : (IsKorean ? "지상" : "Ground"))
                    : "--",
                Color.Lerp(ImGuiGameUi.AccentGold, ImGuiGameUi.AccentCyan, 0.24f));
            DrawStatChip(
                new Rect(innerX + w2 + gap, y1, w2, chipH),
                IsKorean ? "명령" : "Order",
                unit.OrderLabel,
                ImGuiGameUi.AccentGold);
        }

        private static void DrawStatChip(Rect rect, string label, string value, Color accent)
        {
            ImGuiGameUi.DrawHudCardWithLeftStripe(rect, ImGuiGameUi.PanelBgHudCard, ImGuiGameUi.BorderCool, accent, 2f);

            GUI.skin.label.fontSize = 9;
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(rect.x + 7f, rect.y + 2f, rect.width - 14f, 10f), label);

            GUI.skin.label.fontSize = 11;
            GUI.color = ImGuiGameUi.TextTitle;
            GUI.Label(new Rect(rect.x + 7f, rect.y + 8f, rect.width - 14f, 10f), value);
        }
    }
}
