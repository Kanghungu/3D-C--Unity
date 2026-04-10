using Game.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleAces
{
    /// <summary>
    /// 옵션 <see cref="GameUserSettings.UiScale01"/> 과 런타임 생성 UGUI CanvasScaler 를 맞춤.
    /// IMGUI 는 <c>ImGuiGameUi.BeginScaledGui</c> 가 동일 설정을 읽음.
    /// </summary>
    public static class BattleAcesUguiScaleUtility
    {
        private static readonly Vector2 Reference1080 = new Vector2(1920f, 1080f);

        /// <summary>
        /// 참조 해상도를 UI 배율의 역수로 줄여 ScaleWithScreenSize 결과가 커지게 함(글자·패널 동시 확대).
        /// </summary>
        public static void ApplyUserUiScale(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            float userScale = Mathf.Clamp(GameUserSettings.UiScale01, 0.75f, 1.35f);
            float inv = Mathf.Max(0.2f, 1f / userScale);
            scaler.referenceResolution = new Vector2(Reference1080.x * inv, Reference1080.y * inv);
        }
    }
}
