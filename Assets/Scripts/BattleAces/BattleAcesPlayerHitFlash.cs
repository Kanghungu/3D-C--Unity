using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>아군이 피해를 입었을 때 화면 가장자리 IMGUI 플래시 강도(0~1).</summary>
    public static class BattleAcesPlayerHitFlash
    {
        private static float flash01;

        /// <summary>코어·유닛 공통 — 피해량 대비 체력 비율로 호출.</summary>
        public static void NotifyPlayerDamage(float normalizedIntensity01)
        {
            float add = Mathf.Clamp01(normalizedIntensity01);
            flash01 = Mathf.Max(flash01, Mathf.Lerp(0.22f, 0.92f, add));
        }

        public static float ConsumeDrawAlpha()
        {
            return flash01;
        }

        public static void TickDecay(float unscaledDeltaTime)
        {
            // 빠르게 사라지되 잔광은 약간 유지
            flash01 = Mathf.MoveTowards(flash01, 0f, unscaledDeltaTime * 2.85f);
        }
    }
}
