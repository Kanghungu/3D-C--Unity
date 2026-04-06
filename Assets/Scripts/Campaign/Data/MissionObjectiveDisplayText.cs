namespace Game.Campaign.Data
{
    /// <summary>
    /// 브리핑·HUD·메뉴 공통 목표 문구(④ 가시성). 톤은 CampaignDialogue_KR 브리핑과 맞춘다.
    /// </summary>
    public static class MissionObjectiveDisplayText
    {
        /// <summary>HUD/상단 바 — 한 줄 승리 조건(명사형·간결).</summary>
        public static string GetPrimaryLine(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore => "이단 교단 전진 코어 섬멸",
                MissionObjectiveKind.SanctuaryDefense => "성역 방어 — 제한 시간 생존",
                MissionObjectiveKind.EscortRelic => "성유물 호위 — 녹색 목표 구역 도달",
                MissionObjectiveKind.SeizeRelicOrNode => "거점 점령 — 구역 유지",
                MissionObjectiveKind.DestroyHeresyStronghold => "이단 본거지(강화 구조물) 제거",
                MissionObjectiveKind.RecoverRelicAndEvacuate => "성유물 확보 — 철수 구역 도달",
                _ => kind.ToString()
            };
        }

        /// <summary>전투 중 보조 줄 — 미니맵 표식·실패 조건(짧게).</summary>
        public static string GetGameplayHint(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore =>
                    "미니맵 붉은 표식 = 이단 교단 코어 · 파괴 시 승리 · 아군 코어가 먼저 붕괴하면 실패",
                MissionObjectiveKind.SanctuaryDefense =>
                    "아군 코어 방어 · 남은 시간은 상단 HUD · 아군 코어 붕괴 시 실패",
                MissionObjectiveKind.EscortRelic =>
                    "미니맵 금색 = 성유물 · 녹색 = 목표 구역 · 유물이 먼저 파괴되면 실패",
                MissionObjectiveKind.SeizeRelicOrNode =>
                    "미니맵 보라 = 점령 구역 · 일정 시간 유지 시 승리",
                MissionObjectiveKind.DestroyHeresyStronghold =>
                    "미니맵 분홍 = 이단 본거지 · 적 코어와 별개 목표 · 아군 코어 붕괴 시 실패",
                MissionObjectiveKind.RecoverRelicAndEvacuate =>
                    "유물 생존 필수 · 이후 미니맵 청색 = 철수 구역",
                _ => string.Empty
            };
        }

        /// <summary>캠페인 메뉴 괄호 안 — 짧은 한글.</summary>
        public static string GetShortLabelForMenu(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore => "적 코어 섬멸",
                MissionObjectiveKind.SanctuaryDefense => "성역 방어",
                MissionObjectiveKind.EscortRelic => "성유물 호위",
                MissionObjectiveKind.SeizeRelicOrNode => "거점 점령",
                MissionObjectiveKind.DestroyHeresyStronghold => "이단 본거지",
                MissionObjectiveKind.RecoverRelicAndEvacuate => "유물·철수",
                _ => kind.ToString()
            };
        }
    }
}
