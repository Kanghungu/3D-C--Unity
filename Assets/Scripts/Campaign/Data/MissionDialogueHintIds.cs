using System.Text.RegularExpressions;

namespace Game.Campaign.Data
{
    /// <summary>
    /// 대사 테이블 키에 붙이는 미션 짧은 접미사 — 예: mission_03_escort → m03 → hint_retry_*_m03
    /// </summary>
    public static class MissionDialogueHintIds
    {
        private static readonly Regex MissionIndexRegex = new Regex(
            @"^mission_(\d+)_",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>mission_03_escort → m03. 매칭 실패 시 null.</summary>
        public static string TryGetShortMissionSuffix(MissionDefinition mission)
        {
            if (mission == null || string.IsNullOrEmpty(mission.MissionId))
            {
                return null;
            }

            Match m = MissionIndexRegex.Match(mission.MissionId);
            if (!m.Success)
            {
                return null;
            }

            string digits = m.Groups[1].Value;
            if (digits.Length >= 2)
            {
                return "m" + digits;
            }

            return "m" + digits.PadLeft(2, '0');
        }
    }
}
