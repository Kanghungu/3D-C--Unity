using UnityEngine;

namespace Game.BattleAces
{
    /// <summary>
    /// Battle Aces·프로토 SF RTS 공통 **아트 방향 한 줄**과 팔레트 고정값.
    /// 에셋·라이트·런타임 연출은 여기서 벗어나지 않는 것이 이상적(데모는 <see cref="BattleAcesDemoStagePresentation"/>).
    /// </summary>
    public static class BattleAcesArtDirection
    {
        /// <summary>기획·에셋 검수용 한 줄(한국어)</summary>
        public const string OneLinerKorean =
            "SF 종교 전쟁 — 차가운 금속·잿빛 안개, 포인트 컬러는 의식용 티얼(청록) 하나만.";

        /// <summary>동일 문장 영문(외부 레퍼런스 공유용)</summary>
        public const string OneLinerEnglish =
            "Cold religious-war SF: gunmetal, ash, fog; one ritual teal accent only.";

        /// <summary>무드 키워드 — 라이팅·무드보드 검색 시</summary>
        public const string MoodKeywords = "cold metal, ash fog, cathedral silence, single teal beacon";

        // —— 레퍼런스(이미지 1~2장 분량의 ‘무엇을 가져올지’만 고정; 저작물 복제 아님) ——

        /// <summary>레퍼런스 A: 색 수·재질 대비(석·모래·짙은 실내)</summary>
        public const string ReferencePrimary =
            "영화 《듄》(2021) 예배·집전 장면 — 색을 적게 쓰고, 차가운 석재와 모래 톤으로 무게감.";

        /// <summary>레퍼런스 B: SF UI·베이스 캠프의 ‘한 가지 악센트’ 규율</summary>
        public const string ReferenceSecondary =
            "《Destiny》 타워/레이드 로비류 — 차가운 금속 베이스 + UI·광원에 단일 포인트 컬러만 강하게.";

        // —— 포인트 컬러(티얼) — 아군 비콘·UI 강조·의식 연출에만 ——

        /// <summary>의식·아군 식별 포인트 — UI(IMGUI AccentCyan)·명령 링·상태 표시와 동일 계열로 통일</summary>
        public static readonly Color PointTeal = new Color(0.18f, 0.88f, 0.98f, 1f);

        /// <summary>이미터·글로우 강도 가이드(0~1 스케일)</summary>
        public const float PointTealEmissionTypical = 0.82f;

        /// <summary>적대는 티얼과 겹치지 않게 — 잿빛 속 낮은 앰버(보조, 절제)</summary>
        public static readonly Color EnemyEmber = new Color(0.92f, 0.38f, 0.22f, 1f);

        // —— 차가운 금속·중립 ——

        public static readonly Color GunmetalDark = new Color(0.12f, 0.13f, 0.16f, 1f);

        public static readonly Color GunmetalMid = new Color(0.18f, 0.2f, 0.24f, 1f);

        public static readonly Color GunmetalLift = new Color(0.26f, 0.28f, 0.32f, 1f);

        public static readonly Color AshStone = new Color(0.22f, 0.21f, 0.2f, 1f);

        // —— 대기·안개(데모 무대와 공유) ——

        public static readonly Color FogHorizon = new Color(0.29f, 0.35f, 0.43f, 1f);

        public static readonly Color AmbientSky = new Color(0.34f, 0.4f, 0.52f, 1f);

        public static readonly Color AmbientEquator = new Color(0.2f, 0.22f, 0.26f, 1f);

        public static readonly Color AmbientGround = new Color(0.09f, 0.1f, 0.12f, 1f);

        public static readonly Color CameraBackdrop = new Color(0.18f, 0.21f, 0.28f, 1f);

        /// <summary>키 라이트를 살짝 따뜻하게(금속만 너무 차가우면 피부·스샷이 밋밋)</summary>
        public static readonly Color KeyLightTint = new Color(1f, 0.96f, 0.9f, 1f);

        // —— 지면 그라데이션(차가운 심연 → 잿빛 림; 주황 과다 금지) ——

        public static readonly Color GroundGradientCool = new Color(0.1f, 0.13f, 0.19f, 1f);

        public static readonly Color GroundGradientMid = new Color(0.18f, 0.2f, 0.24f, 1f);

        public static readonly Color GroundGradientAshRim = new Color(0.28f, 0.25f, 0.23f, 1f);

        /// <summary>비 Classic 레이아웃 복귀용 단순 주변광</summary>
        public static readonly Color AmbientFlatNeutral = new Color(0.2f, 0.22f, 0.26f, 1f);

        /// <summary>지형·장애물 실루엣(무대)</summary>
        public static readonly Color TerrainBerm = new Color(0.14f, 0.15f, 0.18f, 1f);

        public static readonly Color ObstacleMonolith = new Color(0.12f, 0.14f, 0.2f, 1f);

        public static readonly Color ObstacleSlab = new Color(0.16f, 0.17f, 0.22f, 1f);

        // —— 미니맵 색약 모드(`GameUserSettings.ColorblindFriendlyMinimap`) — 티얼·앰버 규율 유지, 명도·색상만 벌려 적록 혼동 완화 ——
        // 문서: `Assets/Docs/BATTLE_ACES_READABILITY.md` §4 · 코드 소비: `BattleAcesReadability`

        /// <summary>색약: 아군 코어 정상 — 티얼을 파랑 쪽으로 밀어 적 앰버와 분리</summary>
        public static readonly Color MinimapColorblindAllyCoreOk =
            Color.Lerp(PointTeal, new Color(0.14f, 0.56f, 1f, 1f), 0.55f);

        /// <summary>색약: 아군 코어 경고 — 시안·하늘 계열(잿빛 팔레트와 동일 축)</summary>
        public static readonly Color MinimapColorblindAllyCoreWarn =
            Color.Lerp(PointTeal, AmbientSky, 0.42f);

        /// <summary>색약: 아군 코어 위험 — 노랑 경고대(티얼과 구분, 적 코어 위험색과 겹치지 않게)</summary>
        public static readonly Color MinimapColorblindAllyCoreCritical =
            new Color(1f, 0.76f, 0.14f, 1f);

        /// <summary>색약: 적 코어 정상 — 앰버를 주황으로 살짝 밀어 아군 청과 대비</summary>
        public static readonly Color MinimapColorblindEnemyCoreOk =
            Color.Lerp(EnemyEmber, new Color(0.98f, 0.32f, 0.1f, 1f), 0.28f);

        /// <summary>색약: 적 코어 경고</summary>
        public static readonly Color MinimapColorblindEnemyCoreWarn =
            Color.Lerp(EnemyEmber, new Color(1f, 0.58f, 0.2f, 1f), 0.42f);

        /// <summary>색약: 적 코어 위험 — 밝은 노랑 띠</summary>
        public static readonly Color MinimapColorblindEnemyCoreCritical =
            Color.Lerp(EnemyEmber, new Color(1f, 0.95f, 0.42f, 1f), 0.38f);

        /// <summary>색약: 아군 유닛 점(선택)</summary>
        public static readonly Color MinimapColorblindAllyUnitSelected =
            Color.Lerp(PointTeal, Color.white, 0.26f);

        /// <summary>색약: 아군 유닛 점(비선택)</summary>
        public static readonly Color MinimapColorblindAllyUnitNormal =
            Color.Lerp(PointTeal, AmbientSky, 0.32f);

        /// <summary>색약: 적 유닛 점</summary>
        public static readonly Color MinimapColorblindEnemyUnit =
            Color.Lerp(EnemyEmber, new Color(1f, 0.55f, 0.12f, 1f), 0.45f);
    }
}
