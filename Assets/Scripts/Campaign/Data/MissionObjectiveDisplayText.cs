namespace Game.Campaign.Data
{
    public static class MissionObjectiveDisplayText
    {
        public static string GetPrimaryLine(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore => "이단 전진 코어 섬멸",
                MissionObjectiveKind.SanctuaryDefense => "성역 방어",
                MissionObjectiveKind.EscortRelic => "성유물 호위",
                MissionObjectiveKind.SeizeRelicOrNode => "거점 점령",
                MissionObjectiveKind.DestroyHeresyStronghold => "이단 본거지 파괴",
                MissionObjectiveKind.RecoverRelicAndEvacuate => "성유물 회수 후 철수",
                _ => kind.ToString()
            };
        }

        /// <summary>상단 목표 바 보조 줄 — 짧게 두 줄까지(줄바꿈 포함).</summary>
        public static string GetGameplayHint(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore =>
                    "붉은 표식 = 적 코어.\n먼저 격파하면 승리.",
                MissionObjectiveKind.SanctuaryDefense =>
                    "남은 시간을 버티면 승리.\n아군 코어가 먼저 무너지면 패배.",
                MissionObjectiveKind.EscortRelic =>
                    "성유물을 목표 구역까지.\n성유물이 파괴되면 패배.",
                MissionObjectiveKind.SeizeRelicOrNode =>
                    "보라 구역을 일정 시간 유지.\n구역 안 병력을 유지하십시오.",
                MissionObjectiveKind.DestroyHeresyStronghold =>
                    "분홍 건물 = 이단 본거지(코어와 별개).\n본거지 격파 필요. 아군 코어 먼저 무너지면 패배.",
                MissionObjectiveKind.RecoverRelicAndEvacuate =>
                    "성유물 확보 후 철수 구역까지.\n성유물 손실 시 패배.",
                _ => string.Empty
            };
        }

        public static string GetShortLabelForMenu(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore => "코어 섬멸",
                MissionObjectiveKind.SanctuaryDefense => "성역 방어",
                MissionObjectiveKind.EscortRelic => "성유물 호위",
                MissionObjectiveKind.SeizeRelicOrNode => "거점 점령",
                MissionObjectiveKind.DestroyHeresyStronghold => "본거지 파괴",
                MissionObjectiveKind.RecoverRelicAndEvacuate => "회수 후 철수",
                _ => kind.ToString()
            };
        }
    }
}
