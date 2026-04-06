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

        public static string GetGameplayHint(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore =>
                    "미니맵의 붉은 표식이 적 코어입니다. 적 코어를 먼저 파괴하면 승리합니다.",
                MissionObjectiveKind.SanctuaryDefense =>
                    "남은 시간을 버티면 승리합니다. 아군 코어가 먼저 파괴되면 패배합니다.",
                MissionObjectiveKind.EscortRelic =>
                    "성유물을 목표 구역까지 호위하십시오. 성유물이 파괴되면 패배합니다.",
                MissionObjectiveKind.SeizeRelicOrNode =>
                    "점령 구역을 일정 시간 유지하면 승리합니다.",
                MissionObjectiveKind.DestroyHeresyStronghold =>
                    "적 코어와 별개로 이단 본거지를 파괴해야 합니다. 아군 코어가 먼저 무너지면 패배합니다.",
                MissionObjectiveKind.RecoverRelicAndEvacuate =>
                    "성유물을 확보한 뒤 철수 구역까지 이동시키십시오.",
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
