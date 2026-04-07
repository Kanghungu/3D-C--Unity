using Game.UI;
using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>아군 피격 시 전면 붉은 플래시(IMGUI, 다른 HUD 위).</summary>
    public sealed class BattleAcesScreenFlashHud : MonoBehaviour
    {
        private void Update()
        {
            BattleAcesPlayerHitFlash.TickDecay(Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            float a = BattleAcesPlayerHitFlash.ConsumeDrawAlpha();
            if (a <= 0.001f)
            {
                return;
            }

            GUI.depth = -4000;
            float edge = Mathf.Lerp(0.08f, 0.38f, a);
            Color c = new Color(0.92f, 0.1f, 0.06f, edge);
            ImGuiGameUi.DrawFilledRect(new Rect(0f, 0f, Screen.width, Screen.height), c);
        }
    }
}
