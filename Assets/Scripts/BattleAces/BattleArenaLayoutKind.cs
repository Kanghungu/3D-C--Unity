namespace Game.BattleAces
{
    /// <summary>
    /// 전장 지형 프리셋 — 평면 크기·코어 위치·장애물(내비·시야)를 한 세트로 적용.
    /// 스타 투혼은 4인 대칭이나, 데모는 1v1 코어이므로 「중앙 교차로 + 측면 우회」만 차용.
    /// </summary>
    public enum BattleArenaLayoutKind
    {
        /// <summary>기존 기본 — 넓은 평지, 장애물 없음</summary>
        ClassicDuel = 0,

        /// <summary>투혼 느낌 — 조금 더 큰 맵, 중앙 십자 복도 + 모서리 쪽 코어</summary>
        CrossroadsSpirit = 1,

        /// <summary>중앙 좁은 초크 — 한 줄 돌파/방어 연습</summary>
        NarrowMidChoke = 2
    }
}
