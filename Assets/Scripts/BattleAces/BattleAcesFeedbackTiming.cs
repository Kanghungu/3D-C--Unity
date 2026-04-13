namespace Game.BattleAces
{
    /// <summary>
    /// 즉시 체감 피드백 — unscaled 초 기준으로 승패·카메라·쿨다운을 한곳에서 맞춤.
    /// 표·회귀 체크: <c>Assets/Docs/BATTLE_ACES_IMMEDIATE_FEEDBACK.md</c>
    /// </summary>
    public static class BattleAcesFeedbackTiming
    {
        /// <summary>승패 스팅 재생 직후 결과 카드(IMGUI)를 띄우기까지 대기 — VictoryPresentation 카메라 양보와 동일 값</summary>
        public const float ResultCardDelayAfterStingUnscaled = 0.14f;

        /// <summary>유닛 피격 히트음 스팸 방지</summary>
        public const float UnitHitSoundCooldownUnscaled = 0.1f;

        /// <summary>현재 선택된 유닛 피격 — 히트음을 조금 더 자주 허용해 타격감 유지</summary>
        public const float UnitHitSoundCooldownSelectedUnscaled = 0.062f;

        /// <summary>아군(비선택) 피격 히트음 — 전체 스팸보다는 조금 촘촘히</summary>
        public const float UnitHitSoundCooldownPlayerAllyUnscaled = 0.078f;

        /// <summary>지휘 코어 피격 HUD 한 줄·(기존) 화면 플래시와 겹치지 않게 타격음 쪽 쿨다운</summary>
        public const float PlayerCoreHitFeedbackCooldownUnscaled = 0.4f;

        /// <summary>미니맵 클릭 링·톤 — 시야 이동은 매번, 소리/링 추가만 쿨다운</summary>
        public const float MinimapClickAudioRippleCooldownUnscaled = 0.12f;

        /// <summary>유닛 선택 수 변경 틱음 — 드래그 박스·미니맵 연속 선택 시 스팸 완화</summary>
        public const float SelectionChangeAudioCooldownUnscaled = 0.085f;
    }
}
