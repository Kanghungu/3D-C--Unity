namespace Game.BattleAces
{
    /// <summary>
    /// “대작 감각”을 1인 스코프로 쓸 때 — 이번 스프린트에 퀄을 몰아줄 화면 1곳과 UGUI 1차 후보를 코드로 고정.
    /// 문서 경로는 <see cref="WeeklyCaptureDocRelativePath"/>, <see cref="UguiBoundaryDocRelativePath"/>.
    /// </summary>
    public static class BattleAcesDemoQualitySprintFocus
    {
        /// <summary>이번 스프린트 ‘대작급으로 밀 화면’ 단일 지정 — 제목 한 줄(한국어)</summary>
        public const string HeroScreenShortTitleKo = "승패 결과 카드";

        /// <summary>동일 — 영문 라벨</summary>
        public const string HeroScreenShortTitleEn = "Win/Lose result card";

        /// <summary>범위 고정 문장 — 이슈·PR 상단에 붙여 무한 폴리싱 방지</summary>
        public const string HeroScreenScopeLineKorean =
            "이번 스프린트 포커스: 전투 종료 후 승패 결과 카드(요약·통계·재시작/메뉴 버튼)만 상용 데모 한 화면 수준으로 밀 것. " +
            "메인 메뉴·전투 HUD·맵 연출은 가독성 유지 수준으로 두고, 이 화면에만 타이포·여백·사운드·연속성을 집중.";

        /// <summary>범위 고정 문장 — 영문</summary>
        public const string HeroScreenScopeLineEnglish =
            "This sprint quality focus: the post-match result card only (summary, stats, restart/menu to demo-store quality). " +
            "Main menu, combat HUD, and map stay readable; polish budget goes here.";

        /// <summary>구현 앵커 — 리뷰어가 열 파일</summary>
        public const string HeroScreenImplementationHint =
            "IMGUI: BattleMissionFlow.DrawResultScreen — 이후 UGUI 1차 이관 후보도 동일 화면(BATTLE_ACES_UGUI_BOUNDARY.md).";

        /// <summary>이 폴더에 수동으로 넣을 **목표 스크린샷** 파일명(주간 비교 기준)</summary>
        public const string TargetSprintReferenceScreenshotFileName = "target_sprint_reference.png";

        /// <summary>주간 10초 캡처 절차 문서(프로젝트 상대 경로)</summary>
        public const string WeeklyCaptureDocRelativePath = "Assets/Docs/ReferenceScreenshots/WEEKLY_CAPTURE.md";

        /// <summary>UGUI 이관 우선순위 결정 문서</summary>
        public const string UguiBoundaryDocRelativePath = "Assets/Docs/BATTLE_ACES_UGUI_BOUNDARY.md";

        /// <summary>첫 UGUI 이관 후보 — <see cref="HeroScreenShortTitleKo"/> 와 동일 화면</summary>
        public const string UguiFirstMigrationCandidateKo =
            "승패 결과 오버레이(BattleMissionFlow) — 모달·경계가 명확해 Canvas 한 장으로 옮기기 쉬움.";

        public const string UguiFirstMigrationCandidateEn =
            "Post-match result overlay (BattleMissionFlow): bounded modal, good first UGUI migration.";

        /// <summary>이슈 제목·PR 본문에 붙이기용 — 포커스만 짧게</summary>
        public static string BuildSprintFocusClipboardKorean()
        {
            return HeroScreenShortTitleKo + "\n" + HeroScreenScopeLineKorean + "\n\n" + HeroScreenImplementationHint;
        }
    }
}
