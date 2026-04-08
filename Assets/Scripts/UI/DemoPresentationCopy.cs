namespace Game.UI
{
    /// <summary>
    /// 데모 약속 문구 단일 출처 — 메뉴·HUD·승패·일시정지에서 같은 말을 쓰기 위함.
    /// </summary>
    public static class DemoPresentationCopy
    {
        /// <summary>메인/데모 상단 한 줄 — 무엇을 하면 이기는지</summary>
        public const string RoundGoalOneLineKo =
            "한 판 목표: 적 본진(코어)을 무너뜨리면 승리 · 아군 코어가 먼저 무너지면 패배";

        /// <summary>끝난 뒤 어디로 가는지(키 + 버튼)</summary>
        public const string AfterMatchExitOneLineKo =
            "끝난 뒤: R 같은 판 재시작 · Esc 또는 「메인/캠페인 메뉴」로 나가기";

        /// <summary>짧은 데모 전용(토스트·좌측 HUD)</summary>
        public const string PracticeToastLineKo =
            "이번 판: 적 코어 격파가 승리 · 끝나면 R 재시작 · Esc 메인";

        /// <summary>입력 안내 — 결과 카드·일시정지 공통 순서(R → Esc → 조작)</summary>
        public static string BuildPauseAndResultInputBlockKo()
        {
            return
                "승패 화면과 같은 키\n" +
                "· R — 같은 판 즉시 재시작 (결과 카드 왼쪽 버튼과 동일)\n" +
                "· Esc — 메인 또는 캠페인 메뉴 (오른쪽 버튼과 동일)";
        }

        public const string KeyboardOnlyNoticeKo =
            "입력: 키보드·마우스 기준 데모입니다. (게임패드 미지원)";

        public const string CreditsOneLineKo =
            "Battle Aces RTS 데모 — 1인 개발 프로토타입";

        public static string BuildVersionFooterKo()
        {
            return $"{CreditsOneLineKo}  |  빌드 {UnityEngine.Application.version}";
        }
    }
}
