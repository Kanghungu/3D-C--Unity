namespace Game.BattleAces
{
    /// <summary>
    /// Classic Duel 데모 전장의 안개·주변광·방향광 보정 수치 한곳 모음.
    /// 톤은 <see cref="BattleAcesArtDirection"/> — 여기서는 거리·강도 계수만 조정.
    /// </summary>
    /// <remarks>
    /// Bloom·컬러 그레이딩: Built-in 은 <c>BattleAcesDemoStageScreenTone</c>, URP 는
    /// <c>BattleAcesUrpClassicDuelVolumeBootstrap</c> — 새 LUT·강채도 악센트 추가 금지(<c>BATTLE_ACES_ART_DIRECTION.md</c>).
    /// </remarks>
    public static class BattleAcesClassicDuelAtmosphereTuning
    {
        // —— 거리 안개(Linear) — BattleAcesDemoStagePresentation.ApplyAtmosphereAndCamera ——

        // 조금 멀리 밀어 중거리 실루엣이 안개에 덜 씻기게(과하면 지평선만 떠 보임 — 주간 캡처로 되돌리기)
        // 중거리 실루엣이 안개에 덜 씻기게 소폭(주간 캡처로 되돌리기)
        public const float FogStartHalfExtentFactor = 0.255f;

        public const float FogStartDistanceMin = 36f;

        public const float FogStartDistanceMax = 84f;

        public const float FogEndHalfExtentFactor = 0.92f;

        public const float FogEndDistanceMin = 150f;

        public const float FogEndDistanceMax = 320f;

        public const float FogDistanceMinimumGap = 12f;

        public const float FogDistanceFloor = 8f;

        public const float CameraFarClipBeyondFogEnd = 48f;

        // —— Trilight 주변광 ——
        // 프리미티브 실루엣이 지평선 안개에 묻히지 않게 살짝만 올림(과하면 플라스틱 느낌 — 주간 캡처로 되돌리기 쉬움)
        // 주간 캡처로 되돌리기 쉬움 — 실루엣만 살짝(채도 추가 아님)
        public const float AmbientIntensity = 1.175f;

        // —— 방향광 — TryWarmDirectionalLight: 씬 원본 색이 들쭉날쭉해도 팔레트 쪽으로 수렴 ——

        /// <summary>0=흰색, 1=<see cref="BattleAcesArtDirection.KeyLightTint"/> — 키 라이트 기준색</summary>
        public const float DirectionalColorKeyTintMix = 0.58f;

        /// <summary>씬에 박힌 방향광 색에서 목표색으로 당기는 비율(높을수록 고정됨)</summary>
        public const float DirectionalColorSnapTowardsPalette = 0.78f;

        public const float DirectionalIntensityScale = 1f;

        public const float DirectionalIntensityMin = 0.72f;

        public const float DirectionalIntensityMax = 1.42f;

        public const float DirectionalShadowStrengthBias = 0.06f;

        public const float DirectionalShadowStrengthMin = 0.62f;

        public const float DirectionalShadowStrengthMax = 0.9f;
    }
}
