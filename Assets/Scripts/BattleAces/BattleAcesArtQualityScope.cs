using System.Text;

namespace Game.BattleAces
{
    /// <summary>
    /// Captures the guardrails for the current Battle Aces art-quality sprint:
    /// reference folders, screenshot cadence, and the categories we are allowed to tune.
    /// </summary>
    public static class BattleAcesArtQualityScope
    {
        /// <summary>Project folder for target captures and local reference images.</summary>
        public const string ReferenceScreenshotsAssetFolder = "Assets/Docs/ReferenceScreenshots";

        /// <summary>Recommended number of external reference images kept in the folder.</summary>
        public const int SuggestedReferenceImageCount = 3;

        /// <summary>Weekly comparison capture width for a 1080p Game view.</summary>
        public const int WeeklyCompareCaptureWidth1080p = 1920;

        /// <summary>Weekly comparison capture height for a 1080p Game view.</summary>
        public const int WeeklyCompareCaptureHeight1080p = 1080;

        /// <summary>Weekly comparison capture width for a 720p Game view.</summary>
        public const int WeeklyCompareCaptureWidth720p = 1280;

        /// <summary>Weekly comparison capture height for a 720p Game view.</summary>
        public const int WeeklyCompareCaptureHeight720p = 720;

        /// <summary>1080p weekly comparison file name stored under the reference folder.</summary>
        public const string WeeklyCompareScreenshotFileName1080p = "weekly_compare_1920x1080.png";

        /// <summary>720p weekly comparison file name stored under the reference folder.</summary>
        public const string WeeklyCompareScreenshotFileName720p = "weekly_compare_1280x720.png";

        /// <summary>Recommended placeholders for external mood-board images.</summary>
        public static readonly string[] SuggestedExternalReferenceFileNames =
        {
            "reference_external_mood_01.png",
            "reference_external_mood_02.png",
            "reference_external_mood_03.png",
        };

        /// <summary>Recommended short review capture length in seconds.</summary>
        public const float RecommendedPlayCaptureReviewSeconds = 10f;

        /// <summary>Example scope lock line copied into notes, PRs, or sprint logs.</summary>
        public const string ExampleScopeLockLineKorean =
            "데모 1차: 금속/지형 밀도 개선, 티얼 단일 포인트 유지. 새 발광 소재 추가 없음. 파티클 대규모 개편은 다음 마일스톤.";

        /// <summary>Categories allowed for the current cheap-spot review pass.</summary>
        public static readonly string[] CheapSpotReviewCategoriesKorean =
        {
            "조명 (방향광·그림자·색온도)",
            "UI (가독성·패널 계층)",
            "지형/맵 볼륨",
            "카메라 (거리·FOV·구도)",
        };

        /// <summary>Axes that stay fixed during the sprint.</summary>
        public static readonly string[] SprintFixedAxisReminderLinesKorean =
        {
            "아트 한 줄: " + BattleAcesArtDirection.OneLinerKorean,
            "컬러 규율: PointTeal / EnemyEmber 중심 유지, 새 포인트 컬러와 과한 발광 추가 금지",
            "폴더 경계: BattleAces / Prototype / Campaign (AGENTS.md 기준)",
            "리뷰 캡처: 약 10초 플레이를 찍고 이번 주에는 싸 보이는 곳 1~2개만 수정",
        };

        /// <summary>Axes we may tune within the sprint.</summary>
        public static readonly string[] SprintTunableAxisReminderLinesKorean =
        {
            "대기·안개·노출값: BattleAcesClassicDuelAtmosphereTuning 및 관련 슬라이더",
            "머티리얼/이미션 강도: ReadablePrimitiveMaterialUtility 범위 안에서만",
            "UI 패널 계층/UGUI 순서: BATTLE_ACES_UGUI_BOUNDARY.md",
            "카메라 줌/원근감: RTSCameraController 범위",
            "성능: Profiler 패스 후 조정치(PROFILER_PASS_TEMPLATE.md)",
        };

        /// <summary>Reminder for asset-heavy phase 8 work.</summary>
        public const string Phase8AssetGateReminderKorean =
            "Phase8(대형 메쉬·신규 VFX)는 target_sprint_reference와 weekly_compare 캡처가 먼저 정렬된 뒤에만 투입합니다. " +
            "그 전에는 톤·구도·판독성 규율 유지, 새 메시나 셰이더 확장은 금지합니다.";

        /// <summary>Builds the sprint-start clipboard block used in notes or task tracking.</summary>
        public static string BuildSprintStartGuardrailsClipboardKorean()
        {
            StringBuilder builder = new StringBuilder(768);
            builder.AppendLine("[Battle Aces · 스프린트 시작 가드레일]");
            builder.AppendLine("고정 축(이번 주에는 바꾸지 않음)");
            for (int i = 0; i < SprintFixedAxisReminderLinesKorean.Length; i++)
            {
                builder.Append("  · ");
                builder.AppendLine(SprintFixedAxisReminderLinesKorean[i]);
            }

            builder.AppendLine();
            builder.AppendLine("조정 가능한 축(수치와 순서만 조정, 정체성은 유지)");
            for (int i = 0; i < SprintTunableAxisReminderLinesKorean.Length; i++)
            {
                builder.Append("  · ");
                builder.AppendLine(SprintTunableAxisReminderLinesKorean[i]);
            }

            builder.AppendLine();
            builder.AppendLine("이번 주 작업 규칙");
            builder.AppendLine("  · 고칠 곳은 카테고리 기준 1~2개만 (CheapSpotReviewCategoriesKorean 참고)");
            builder.AppendLine("  · 스코프 한 줄: " + ExampleScopeLockLineKorean);
            builder.AppendLine();
            builder.AppendLine("[Phase8 에셋 게이트]");
            builder.AppendLine("  · " + Phase8AssetGateReminderKorean);
            return builder.ToString();
        }

        /// <summary>Builds a checklist block for notes, PRs, or short sprint logs.</summary>
        public static string BuildClipboardChecklistKorean()
        {
            StringBuilder builder = new StringBuilder(512);
            builder.AppendLine("[Battle Aces 품질 체크리스트]");
            builder.AppendLine(BattleAcesArtDirection.OneLinerKorean);
            builder.AppendLine("(스프린트 시작 시에는 고정/조정 가능 축 전체를 Battle Aces 스프린트 시작 가드레일 메뉴에서 복사)");
            builder.AppendLine();
            builder.Append("레퍼/목표 스크린샷 폴더: ");
            builder.AppendLine(ReferenceScreenshotsAssetFolder);
            builder.Append("플레이 캡처 권장 길이(초): ");
            builder.AppendLine(RecommendedPlayCaptureReviewSeconds.ToString("0"));
            builder.AppendLine();
            builder.AppendLine("싸 보이는 곳 분류(이번 주에는 1~2개만 선택)");
            for (int i = 0; i < CheapSpotReviewCategoriesKorean.Length; i++)
            {
                builder.Append(i + 1);
                builder.Append(". ");
                builder.AppendLine(CheapSpotReviewCategoriesKorean[i]);
            }

            builder.AppendLine();
            builder.AppendLine("금지: 포인트 컬러(티얼) 외 신규 강조색, 과한 발광 소재, 대규모 셰이더 확장.");
            builder.AppendLine("예시 스코프 문장:");
            builder.AppendLine(ExampleScopeLockLineKorean);
            builder.AppendLine();
            builder.AppendLine("주간 비교 캡처");
            builder.Append("1080p: ");
            builder.Append(WeeklyCompareCaptureWidth1080p.ToString());
            builder.Append("x");
            builder.Append(WeeklyCompareCaptureHeight1080p.ToString());
            builder.Append(" 파일명 ");
            builder.AppendLine(WeeklyCompareScreenshotFileName1080p);
            builder.Append("720p: ");
            builder.Append(WeeklyCompareCaptureWidth720p.ToString());
            builder.Append("x");
            builder.Append(WeeklyCompareCaptureHeight720p.ToString());
            builder.Append(" 파일명 ");
            builder.AppendLine(WeeklyCompareScreenshotFileName720p);
            builder.AppendLine("(Unity Game 뷰 해상도를 같은 값으로 맞춘 뒤 동일 구간을 각각 촬영)");
            builder.AppendLine();
            builder.Append("외부 레퍼 권장 파일명 2~3장: ");
            builder.AppendLine(string.Join(", ", SuggestedExternalReferenceFileNames));
            builder.AppendLine();
            builder.AppendLine("이번 스프린트 한 화면 포커스");
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.HeroScreenShortTitleKo);
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.HeroScreenScopeLineKorean);
            builder.Append("목표 캡처 파일명: ");
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.TargetSprintReferenceScreenshotFileName);
            builder.Append("주간 문서: ");
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.WeeklyCaptureDocRelativePath);
            builder.Append("UGUI 1차 후보: ");
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.UguiFirstMigrationCandidateKo);
            return builder.ToString();
        }

        /// <summary>Builds a short scope-lock note for logs or PR descriptions.</summary>
        public static string BuildScopeLockClipboardKorean()
        {
            StringBuilder builder = new StringBuilder(384);
            builder.AppendLine("[Battle Aces · 스프린트 스코프 고정]");
            builder.AppendLine(ExampleScopeLockLineKorean);
            builder.AppendLine();
            builder.AppendLine(BattleAcesDemoQualitySprintFocus.HeroScreenScopeLineKorean);
            return builder.ToString();
        }
    }
}
