using Game.Settings;

namespace Game.UI
{
    public static class DemoPresentationCopy
    {
        public static string RoundGoalOneLineKo =>
            IsKorean
                ? "목표: 적 코어를 먼저 파괴하면 승리, 아군 지휘 코어가 먼저 무너지면 패배."
                : "Objective: destroy the enemy core before your own command core falls.";

        public static string AfterMatchExitOneLineKo =>
            IsKorean
                ? "전투 종료 후: R로 재시작, Esc로 메뉴로 나갑니다."
                : "After the match: press R to restart or Esc to leave to the menu.";

        public static string PracticeToastLineKo =>
            IsKorean
                ? "연습 루프: 적 코어를 파괴하고, 빠르게 재시작하며 전투를 다듬으세요."
                : "Practice loop: break the enemy core, restart fast, and iterate.";

        public static string KeyboardOnlyNoticeKo =>
            IsKorean
                ? "조작: 현재 프로토타입 빌드는 키보드와 마우스 기준입니다."
                : "Controls: keyboard and mouse only in this prototype build.";

        public static string BuildPauseAndResultInputBlockKo()
        {
            return IsKorean
                ? "일시정지 / 결과 화면 조작\nR  현재 전투 다시 시작\nEsc  캠페인 메뉴로 돌아가기"
                : "Pause / Result Controls\nR  Restart the current battle\nEsc  Return to the campaign menu";
        }

        public static string CreditsOneLineKo =>
            IsKorean
                ? "Battle Aces RTS Demo | 1인 개발 Unity 프로토타입"
                : "Battle Aces RTS Demo | solo-developed Unity prototype";

        public static string BuildVersionFooterKo()
        {
            return $"{CreditsOneLineKo} | build {UnityEngine.Application.version}";
        }

        private static bool IsKorean => GameUserSettings.Language == GameLanguage.Korean;
    }
}
