namespace Game.Campaign.Data
{
    /// <summary>
    /// 스토리 미션 중심 목표 종류(3~5종 먼저 고정 후 콘텐츠 확장).
    /// </summary>
    public enum MissionObjectiveKind
    {
        /// <summary>적 코어(보스 거점) 파괴 — 기본 RTS 승리.</summary>
        DestroyEnemyCore = 0,

        /// <summary>성역 방어 — 일정 시간 생존 또는 웨이브 견딤.</summary>
        SanctuaryDefense = 1,

        /// <summary>성유물 호위 — 유물을 목표 구역까지 이동.</summary>
        EscortRelic = 2,

        /// <summary>점령 — 거점/유물 확보(유지 시간 등은 미션별 튜닝).</summary>
        SeizeRelicOrNode = 3,

        /// <summary>이단 본거지 제거 — 특정 구조물/강화 거점 파괴.</summary>
        DestroyHeresyStronghold = 4,

        /// <summary>성유물 회수 후 철수 — 유물 확보 + 철수 구역 도달.</summary>
        RecoverRelicAndEvacuate = 5
    }
}
