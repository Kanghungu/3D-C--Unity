namespace Game.BattleAces
{
    /// <summary>
    /// 스토어 스크린샷용 — 전투 IMGUI 크롬(좌측 패널·미니맵 등)만 숨김.
    /// 에디터·Development 빌드에서 F12로 토글(<see cref="BattleAcesDevelopmentHud"/>).
    /// </summary>
    public static class BattleAcesHudCaptureMode
    {
        /// <summary>켜면 주요 전투 HUD IMGUI 그리기 생략 — UGUI 목표/결과는 그대로</summary>
        public static bool SuppressCombatChromeForScreenshot { get; private set; }

        public static void SetSuppressCombatChromeForScreenshot(bool value)
        {
            SuppressCombatChromeForScreenshot = value;
        }

        public static void ToggleSuppressCombatChromeForScreenshot()
        {
            SuppressCombatChromeForScreenshot = !SuppressCombatChromeForScreenshot;
        }
    }
}
