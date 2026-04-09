using System;

namespace Game.Campaign.Data
{
    public static class MissionObjectiveDisplayText
    {
        /// <summary>캠페인 메뉴·상단 바 — 번들 미션이면 번들 표시명</summary>
        public static string ResolveMissionDisplayName(MissionDefinition mission)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                return DemoChapter6SingleMatchBundle.DisplayName;
            }

            return mission.DisplayName ?? string.Empty;
        }

        public static string GetPrimaryLine(MissionObjectiveKind kind)
        {
            return kind switch
            {
                MissionObjectiveKind.DestroyEnemyCore => "적 코어 섬멸",
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
                    "적대 표식 = 적 코어.\n먼저 격파하면 승리.",
                MissionObjectiveKind.SanctuaryDefense =>
                    "방어 타이머를 채우면 승리.\n지휘 코어가 먼저 붕괴하면 패배.",
                MissionObjectiveKind.EscortRelic =>
                    "성유물을 목표 구역까지 호위.\n성유물이 파괴되면 패배.",
                MissionObjectiveKind.SeizeRelicOrNode =>
                    "점령 구역(보라 표시)을 일정 시간 유지.\n아군이 구역 안에 있을 때만 진행되며, 점령 중 소량 회복됩니다.",
                MissionObjectiveKind.DestroyHeresyStronghold =>
                    "이단 본거지(분홍 표시, 적 코어와 별개)를 격파.\n지휘 코어가 먼저 붕괴하면 패배.",
                MissionObjectiveKind.RecoverRelicAndEvacuate =>
                    "성유물 확보 후 철수 구역까지 이동.\n성유물을 잃으면 패배.",
                _ => string.Empty
            };
        }

        /// <summary>미션 에셋 + 데모 번들(챕터6 등) 우선</summary>
        public static string GetPrimaryLine(MissionDefinition mission)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                return DemoChapter6SingleMatchBundle.ObjectivePrimaryLine;
            }

            return GetPrimaryLine(mission.ObjectiveKind);
        }

        public static string GetGameplayHint(MissionDefinition mission)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                return DemoChapter6SingleMatchBundle.ObjectiveHint;
            }

            // 대표 미션1 — 브리핑·패배 힌트와 한 세트로「별 목표 없음」을 상단/F1에도 짧게 박아 둠
            if (mission.ObjectiveKind == MissionObjectiveKind.DestroyEnemyCore
                && string.Equals(mission.MissionId, "mission_01_skirmish", StringComparison.Ordinal)
                && string.IsNullOrEmpty(mission.OptionalBonusObjectiveId))
            {
                return "적대 표식 = 적 코어.\n먼저 격파하면 승리.\n(선택 별 목표 없음)";
            }

            return GetGameplayHint(mission.ObjectiveKind);
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

        public static string GetShortLabelForMenu(MissionDefinition mission)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            if (DemoChapter6SingleMatchBundle.Matches(mission))
            {
                return "요새 돌파";
            }

            return GetShortLabelForMenu(mission.ObjectiveKind);
        }
    }
}
