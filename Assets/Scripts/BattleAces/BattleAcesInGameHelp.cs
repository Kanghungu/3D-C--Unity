using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.BattleAces
{
    public sealed class BattleAcesInGameHelp : MonoBehaviour
    {
        private bool visible;

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return;
            }

            if (BattleAcesMatchController.TryGetInstance(out BattleAcesMatchController baMatch) && baMatch.IsFinished)
            {
                visible = false;
                return;
            }

            if (CampaignBattleFlow.Instance != null && CampaignBattleFlow.Instance.IsBriefingBlocking)
            {
                visible = false;
                return;
            }

            if (kb.f1Key.wasPressedThisFrame)
            {
                visible = !visible;
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            float w = Mathf.Min(520f, Screen.width - 40f);
            float h = Mathf.Min(420f, Screen.height - 80f);
            Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), ImGuiGameUi.DimFullscreen);
            ImGuiGameUi.DrawPanelFrame(card, ImGuiGameUi.PanelBgLift, ImGuiGameUi.BorderCool, 2f);

            GUI.skin.label.fontSize = 20;
            GUI.color = ImGuiGameUi.AccentGold;
            GUI.Label(new Rect(card.x + 18f, card.y + 14f, card.width - 36f, 32f), "Controls");
            GUI.skin.label.fontSize = 14;
            GUI.color = ImGuiGameUi.TextTitle;

            const string body =
                "Right-click: move or attack with selected units\n" +
                "1-8: train units from the deck\n" +
                "T / Y / U: core upgrades\n" +
                "P / 1 / 2 / 3: time control\n" +
                "Space: focus camera on action\n" +
                "Home: focus camera on your core\n" +
                "Minimap: click jump, drag pan, Shift drag box select\n" +
                "V: switch auto target mode\n" +
                "F1: open or close this help";

            GUI.Label(new Rect(card.x + 18f, card.y + 52f, card.width - 36f, card.height - 100f), body);
            GUI.color = ImGuiGameUi.TextMuted;
            GUI.Label(new Rect(card.x + 18f, card.yMax - 44f, card.width - 36f, 28f), "Press F1 again to close.");
            GUI.color = Color.white;

            Rect closeBtn = new Rect(card.xMax - 120f, card.y + 12f, 100f, 32f);
            if (ImGuiGameUi.GameMenuButton(closeBtn, "Close (F1)"))
            {
                visible = false;
            }
        }
    }
}
