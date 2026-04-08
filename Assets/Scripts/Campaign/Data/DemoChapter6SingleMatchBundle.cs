using Game.Units;

namespace Game.Campaign.Data
{
    /// <summary>
    /// 데모 챕터 6 — 표시명·목표 문구·덱·적 패턴을 한 파일에서만 고칩니다.
    /// Mission_06 에셋은 ID·대사·씬 참조용; 실제 전투 수치는 여기가 우선합니다.
    /// </summary>
    public static class DemoChapter6SingleMatchBundle
    {
        public const string MissionId = "mission_06_fortress";

        /// <summary>캠페인 메뉴가 앞에 번호를 붙이므로 여기서는 번호 없이 제목만</summary>
        public const string DisplayName = "요새 돌파 — 단일 데모 전장";

        public const MissionObjectiveKind ObjectiveKind = MissionObjectiveKind.DestroyEnemyCore;

        public const string ObjectivePrimaryLine = "공중 요새 압박 하에 적 코어 격파";

        public const string ObjectiveHint =
            "붉은 표식 = 적 코어.\n" +
            "이 한 판 덱·요새 슬롯은 아래 번들 고정. 건물 클릭 선택은 없습니다.";

        public const string EnemyPatternId = "fortress_break";

        /// <summary>0이면 기본 18.25초 경로 유지</summary>
        public const float EnemyBrainThinkIntervalOverride = 0f;

        public const float EnemyBrainThinkIntervalMultiplier = 0.9f;

        public const bool AirborneCitadelFocus = true;

        /// <summary>미러 레이아웃 사용 여부(번들 단일 소스)</summary>
        public const bool MirroredLayoutVariant = false;

        /// <summary>필드 중간 대사 트리거 박스 — 이 데모에선 맵을 비우기 위해 끔</summary>
        public const bool SpawnStoryFieldTriggers = false;

        private static readonly UnitArchetype[] PlayerDeckEight =
        {
            UnitArchetype.Spearman,
            UnitArchetype.ShieldInfantry,
            UnitArchetype.Rifleman,
            UnitArchetype.MobileFortress,
            UnitArchetype.Fighter,
            UnitArchetype.SpecialWarrior,
            UnitArchetype.RoyalGuard,
            UnitArchetype.Outrider
        };

        public static bool Matches(MissionDefinition mission)
        {
            return mission != null &&
                   !string.IsNullOrEmpty(mission.MissionId) &&
                   string.Equals(mission.MissionId, MissionId, System.StringComparison.Ordinal);
        }

        public static UnitArchetype[] GetPlayerDeckEightCopy()
        {
            var copy = new UnitArchetype[8];
            System.Array.Copy(PlayerDeckEight, copy, 8);
            return copy;
        }
    }
}
