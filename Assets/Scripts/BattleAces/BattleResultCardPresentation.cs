namespace Game.BattleAces
{
    /// <summary>
    /// 승패 결과 카드 한 장에 필요한 문자열·플래그 — IMGUI/UGUI 공통 데이터.
    /// </summary>
    public readonly struct BattleResultCardPresentation
    {
        public readonly bool Won;

        public readonly string Title;

        public readonly string EndReasonLine;

        public readonly string DefeatRetryHint;

        public readonly string Body;

        public readonly string StatsOneLine;

        public readonly bool HasBonusLine;

        public readonly string BonusLine;

        public readonly bool BonusLineUseGold;

        public readonly string NextStepLine;

        public readonly string RKeyLine;

        public readonly string EscLine;

        public readonly string FooterDevNote;

        public readonly string RetryLabel;

        public readonly string MenuButtonLabel;

        public readonly string MenuSceneName;

        public BattleResultCardPresentation(
            bool won,
            string title,
            string endReasonLine,
            string defeatRetryHint,
            string body,
            string statsOneLine,
            bool hasBonusLine,
            string bonusLine,
            bool bonusLineUseGold,
            string nextStepLine,
            string rKeyLine,
            string escLine,
            string footerDevNote,
            string retryLabel,
            string menuButtonLabel,
            string menuSceneName)
        {
            Won = won;
            Title = title ?? string.Empty;
            EndReasonLine = endReasonLine ?? string.Empty;
            DefeatRetryHint = defeatRetryHint ?? string.Empty;
            Body = body ?? string.Empty;
            StatsOneLine = statsOneLine ?? string.Empty;
            HasBonusLine = hasBonusLine;
            BonusLine = bonusLine ?? string.Empty;
            BonusLineUseGold = bonusLineUseGold;
            NextStepLine = nextStepLine ?? string.Empty;
            RKeyLine = rKeyLine ?? string.Empty;
            EscLine = escLine ?? string.Empty;
            FooterDevNote = footerDevNote ?? string.Empty;
            RetryLabel = retryLabel ?? string.Empty;
            MenuButtonLabel = menuButtonLabel ?? string.Empty;
            MenuSceneName = string.IsNullOrEmpty(menuSceneName) ? "CampaignMenu" : menuSceneName;
        }
    }
}
