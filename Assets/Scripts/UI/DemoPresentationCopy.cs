using Game.Settings;

namespace Game.UI
{
    /// <summary>
    /// 한 판 톤 통일 — 브리핑·HUD·일시정지·승패·토스트·F1·상단 목표가 같은 목소리(한/영).
    /// 용어: 아군 거점 = 「지휘 코어」, 적 거점 = 「적 코어」; 표식 설명은 색 대신 「적대/아군 표식」(아트: <c>BattleAcesArtDirection</c> 티얼·앰버).
    /// 문체: 짧은 군사 브리핑 + 차가운 SF 종교전 분위기(과장·이모지 없음).
    /// </summary>
    public static class DemoPresentationCopy
    {
        private static bool IsKorean => GameUserSettings.Language == GameLanguage.Korean;

        // --- 핵심 목표 한 줄(좌측 HUD 폴백·튜토리얼 톤) ---
        public static string RoundGoalOneLineKo =>
            IsKorean
                ? "주 목표: 적 코어를 먼저 무너뜨리면 승리. 지휘 코어가 먼저 붕괴하면 패배."
                : "Primary objective: destroy the enemy core first to win. If your command core falls first, you lose.";

        public static string AfterMatchExitOneLineKo =>
            IsKorean
                ? "전투 종료 후: R로 같은 작전 재시작, Esc로 메뉴로 나갑니다."
                : "After the battle: R restarts this operation, Esc returns to the menu.";

        public static string PracticeToastLineKo =>
            IsKorean
                ? "연습 작전: 적 코어까지 압박한 뒤, 승패 화면에서 R로 같은 판을 반복해 감을 익히십시오."
                : "Practice: push to the enemy core, then press R on the result screen to repeat the same match.";

        public static string KeyboardOnlyNoticeKo =>
            IsKorean
                ? "조작: 이 빌드는 키보드·마우스 기준입니다."
                : "Controls: keyboard and mouse in this build.";

        public static string CreditsOneLineKo =>
            IsKorean
                ? "Battle Aces RTS 데모 | 1인 개발 Unity 프로토타입"
                : "Battle Aces RTS demo | solo Unity prototype";

        public static string BuildVersionFooterKo()
        {
            return IsKorean
                ? $"{CreditsOneLineKo} | 빌드 {UnityEngine.Application.version}"
                : $"{CreditsOneLineKo} | build {UnityEngine.Application.version}";
        }

        /// <summary>일시정지 패널 제목 — 결과 카드·브리핑과 동일한 작전 톤</summary>
        public static string PausePanelTitleKo =>
            IsKorean ? "일시정지 (P로 재개)" : "Paused (press P to resume)";

        /// <summary>브리핑 카드 하단 — 카메라는 Space가 작전 시작에 쓰임(RTSCameraController 가 브리핑 중 포커스 차단)</summary>
        public static string BriefingContinueFooterKo =>
            IsKorean
                ? "Space / Enter / 좌클릭 — 작전 개시 (브리핑 중 Space 는 카메라 이동에 사용되지 않습니다)"
                : "Space / Enter / left click — begin operation (Space does not pan the camera during briefing)";

        /// <summary>브리핑 상단 바 접두 — UGUI 상단과 동일 용어</summary>
        public static string BriefingTopBarPrefixKo =>
            IsKorean ? "작전 목표" : "Operation objective";

        /// <summary>상단 UGUI 목표 줄이 있을 때 좌측 패널에 넣는 안내(목표 문구 중복 방지)</summary>
        public static string LeftHudObjectiveFromTopBarKo =>
            IsKorean
                ? "주 목표는 화면 상단 줄을 따릅니다."
                : "Primary objective follows the top bar.";

        /// <summary>F1 패널 역할 헤더 — 1차 목표는 상단 UGUI</summary>
        public static string HelpF1SectionRoleLine =>
            IsKorean ? "F1 · 조작 요약 / 보조 목표·팁" : "F1 · Controls / secondary objectives & tips";

        /// <summary>F1에서 주 목표 문구를 반복하지 않을 때 한 줄 안내</summary>
        public static string HelpF1PrimaryOnTopBarLine =>
            IsKorean
                ? "주 목표 문구는 화면 상단 목표 바(1차)를 따릅니다."
                : "Primary objective text is on the top bar (tier 1).";

        /// <summary>F1 압축 모드에서 GetGameplayHint 위 짧은 머리글</summary>
        public static string HelpF1SecondaryHintsHeader =>
            IsKorean ? "보조 목표·팁" : "Secondary hints";

        /// <summary>F1 전체 미션 블록(상단 바 없을 때) 섹션 제목</summary>
        public static string HelpMissionThisOperationHeader =>
            IsKorean ? "이번 작전" : "This operation";

        public static string HelpSummarySectionTitle =>
            IsKorean ? "핵심 요약" : "Quick reference";

        public static string HelpDetailToggleExpand =>
            IsKorean ? "자세히 보기 ▼ (전체 조작 목록)" : "More ▼ (full control list)";

        public static string HelpDetailToggleCollapse =>
            IsKorean ? "접기 ▲ (전체 목록 숨김)" : "Less ▲ (hide full list)";

        public static string HelpF1FooterCloseHint =>
            IsKorean ? "다시 F1 을 누르면 닫습니다." : "Press F1 again to close.";

        public static string HelpF1CloseButton =>
            IsKorean ? "닫기 (F1)" : "Close (F1)";

        /// <summary>F1 패널 큰 제목 — 상단 목표 바와 같은 티얼 악센트로 표시</summary>
        public static string HelpF1PanelTitle =>
            IsKorean ? "조작·목표 안내" : "Controls & objectives";

        /// <summary>F1 핵심 요약 — Battle Aces 조작 + 고정 키 안내 한 줄</summary>
        public static string BuildHelpCoreSummarySixLines()
        {
            if (!IsKorean)
            {
                return
                    "• Left HUD: credits (large #) · deck 1–8 · build queue · rally (Alt+right-click ground) · T/Y/U core upgrades — primary objective on the top bar\n" +
                    "• Select: click/drag (allies only) · right-click move/attack · Ctrl+A all · Esc clear\n" +
                    "• Camera: WASD · arrows · edge · wheel (no rotate) · Space / Home · comma = rally view\n" +
                    "• Tactical map (lower-right): click/drag · Ctrl+click · Shift box select · Shift+M size\n" +
                    "• P pause · [ ] / numpad ± speed · O settings · V auto-target · H/G/B (with units selected)\n" +
                    "• F1 this panel · R restart only on the result screen\n" +
                    "• Keys are fixed for Battle Aces; rebinding is not in this demo (see O settings hint).";
            }

            return
                "• 왼쪽 HUD: 자원(큰 숫자)·덱 1~8·생산 큐·집결(Alt+지면 우클릭)·T/Y/U 지휘 코어 강화 — 주 목표는 상단 바\n" +
                "• 선택: 좌클릭/드래그(아군만) · 우클릭 이동·공격 · Ctrl+A 전체 · Esc 해제\n" +
                "• 카메라: WASD·화살표·가장자리·휠(회전 없음) · Space/ Home · ,(쉼표) 집결 지점으로 시야\n" +
                "• 우하단 전술 지도: 클릭·드래그 · Ctrl+클릭·Shift 드래그 선택 · Shift+M 크기\n" +
                "• P 일시정지 · [ ]·숫자패드 ± 배속 · O 설정 · V 자동 표적 · H/G/B(유닛 선택 시)\n" +
                "• F1 이 창 · 승패 화면에서만 R 재시작\n" +
                "• Battle Aces 조작 키는 고정 — 이 데모에서 재바인딩 없음(O 설정 안내 참고).";
        }

        /// <summary>설정(O) 패널 — Battle Aces 범위 밖인 키 재바인딩을 짧게 고지</summary>
        public static string SettingsInputNoRebindHint =>
            IsKorean
                ? "Battle Aces 조작 키는 이 빌드에서 고정입니다. 키 재바인딩·Steam 입력 재정의 등은 데모 범위 밖입니다."
                : "Battle Aces controls are fixed in this build. Key rebinding and Steam input remap are outside this demo’s scope.";

        /// <summary>일시정지(P) 패널 본문 뒤에 붙이는 고정 키 안내</summary>
        public static string PauseControlsFixedKeysNotice =>
            IsKorean
                ? "조작 키는 Battle Aces 데모에서 고정이며, 여기서 재바인딩할 수 없습니다."
                : "Controls are fixed for the Battle Aces demo; you cannot rebind keys here.";

        /// <summary>F1 「자세히」전체 목록 — 한/영 단일 출처</summary>
        public static string BuildBattleAcesF1ExpandedDetailBody()
        {
            if (!IsKorean)
            {
                return
                    "— Battle Aces demo scope —\n" +
                    "· Fixed controls — no key rebinding inside this demo.\n" +
                    "P: pause · [ ] or numpad ±: speed steps\n" +
                    "O: settings (volume · UI scale · fullscreen, etc.)\n" +
                    "Left click / drag: allies only · Ctrl+A: all living allies · Esc: clear selection\n" +
                    "Right click: move / attack enemies & targets\n" +
                    "Alt + right-click ground: rally — ritual teal ring · comma: camera to rally point\n" +
                    "1–8: deck build orders (short reject sound if blocked)\n" +
                    "T / Y / U: command core production / armor / economy upgrades (costs on left HUD)\n" +
                    "H / G / B: hold / defend (near ally point) / retreat — with units selected\n" +
                    "Camera: WASD · arrows · edge · wheel zoom (no keyboard rotate · WASD ignored while Ctrl held)\n" +
                    "Space: battle view · Home: command core\n" +
                    "Tactical map: click/drag pan · Ctrl+click nearby allies · Shift+drag box select · Shift+M size\n" +
                    "V: auto-target toggle · F1: this panel\n" +
                    "R: restart same scene only on the victory/defeat screen\n" +
                    "Ctrl+F2–F5: assign control groups · F2–F5: recall (double-tap: camera to that group)\n" +
                    "※ F1 is help-only, so there is no F1 control group.\n\n" +
                    "— Not in this demo —\n" +
                    "Multiplayer, tech trees, buildings beyond the command core, click-select buildings for orders, campaign-external map editing, key rebinding, etc.";
            }

            return
                "— Battle Aces 데모에 있는 것만 —\n" +
                "· 조작 키는 고정이며, 이 데모 범위에서는 재바인딩할 수 없습니다.\n" +
                "P: 일시정지 · [ / ] 또는 숫자패드 - +: 배속 단계\n" +
                "O: 설정(볼륨·UI 크기·전체화면 등)\n" +
                "좌클릭·드래그: 아군만 선택 · Ctrl+A: 살아 있는 아군 전체 · Esc: 선택 해제\n" +
                "우클릭: 이동 / 적·목표 공격\n" +
                "Alt+지면 우클릭: 집결(랠리) — 티얼(청록) 링 · ,(쉼표): 집결 지점으로 카메라\n" +
                "1~8: 덱 생산 주문(자원·큐 제한 시 짧은 거절음)\n" +
                "T / Y / U: 지휘 코어 생산·장갑·자원 강화(왼쪽 HUD 비용 표시)\n" +
                "H / G / B: 홀드 / 수비(가까운 아군 거점) / 후퇴 — 유닛 선택 시\n" +
                "카메라: WASD·화살표·가장자리 · 휠 줌(키보드 회전 없음 · Ctrl 누른 채 WASD는 카메라 이동 안 함)\n" +
                "Space: 교전 쪽 시야 · Home: 지휘 코어\n" +
                "전술 지도: 클릭·드래그 이동 · Ctrl+클릭 근처 아군 · Shift+드래그 박스 선택 · Shift+M 크기\n" +
                "V: 자동 표적(가까운 적 우선) 토글 · F1: 이 창\n" +
                "R: 승리/패배 결과 화면에서만 같은 씬 재시작\n" +
                "Ctrl+F2~F5: 부대 단축 지정 · F2~F5: 불러오기(더블 탭 시 해당 부대로 카메라)\n" +
                "※ F1은 도움말 전용이라 F1 단축 그룹은 쓰이지 않습니다.\n\n" +
                "— 이 데모에 없음 —\n" +
                "멀티플레이, 기술 트리, 지휘 코어 외 건물 건설, 거점/건물 마우스 선택 후 명령, 캠페인 외 맵 편집, 키 재바인딩 등";
        }

        /// <summary>일시정지·승패 공통 입력 블록(짧은 군사 브리핑체)</summary>
        public static string BuildPauseAndResultInputBlockKo()
        {
            return IsKorean
                ? "입력\n· R — 같은 작전 즉시 재시작\n· Esc — 메뉴로 복귀"
                : "Input\n· R — restart this operation\n· Esc — return to menu";
        }

        /// <summary>스커미시 전용 결과 헤드라인(IMGUI 오버레이)</summary>
        public static void GetSkirmishResultHeadline(bool victory, out string headline)
        {
            headline = victory
                ? (IsKorean ? "승리 — 적 코어 격파" : "Victory — enemy core destroyed")
                : (IsKorean ? "패배 — 지휘 코어 붕괴" : "Defeat — command core lost");
        }

        /// <summary>승패 카드 제목</summary>
        public static string GetResultScreenTitle(bool victory) =>
            victory
                ? (IsKorean ? "작전 승리" : "Operation victory")
                : (IsKorean ? "작전 실패" : "Operation failed");

        /// <summary>브리핑 본문 폴백(대사 없을 때)</summary>
        public static string BuildDefaultBriefingBodyKo(string missionDisplayName, string primaryObjectiveLine)
        {
            if (!IsKorean)
            {
                return $"Operation: {missionDisplayName}\n\nPrimary objective: {primaryObjectiveLine}\n\nForm your line and press the enemy salient.";
            }

            return $"작전명: {missionDisplayName}\n\n주 목표: {primaryObjectiveLine}\n\n전열을 정비하고 적대 교두를 압박하십시오.";
        }

        /// <summary>결과 카드 — R 키 안내</summary>
        public static string ResultScreenRKeyLine(bool isFallbackOneMatchDemo, bool skirmishPractice)
        {
            if (!IsKorean)
            {
                if (isFallbackOneMatchDemo)
                {
                    return "R — restart this demo (same as the left button)";
                }

                return skirmishPractice
                    ? "R — restart this skirmish (same as the left button)"
                    : "R — restart this mission (same as the left button)";
            }

            if (isFallbackOneMatchDemo)
            {
                return "R 키 — 같은 데모 즉시 재시작 (왼쪽 버튼과 동일)";
            }

            return skirmishPractice
                ? "R 키 — 같은 스커미시 즉시 재시작 (왼쪽 버튼과 동일)"
                : "R 키 — 같은 미션 즉시 재시작 (왼쪽 버튼과 동일)";
        }

        /// <summary>결과 카드 — Esc 안내</summary>
        public static string ResultScreenEscLine(bool skirmishPractice) =>
            skirmishPractice
                ? (IsKorean
                    ? "Esc 키 — 메인 메뉴로 (오른쪽 버튼과 동일)"
                    : "Esc — main menu (same as the right button)")
                : (IsKorean
                    ? "Esc 키 — 캠페인 메뉴로 (오른쪽 버튼과 동일)"
                    : "Esc — campaign menu (same as the right button)");

        /// <summary>연습/데모 본문 폴백</summary>
        public static string ResultBodyFallbackPractice(bool won, bool isFallbackOneMatchDemo)
        {
            if (!IsKorean)
            {
                if (isFallbackOneMatchDemo)
                {
                    return won
                        ? "Demo win — good for learning command core, deck, and enemy waves."
                        : "Demo loss — adjust production and rally (Alt+right-click), then press R or the button below.";
                }

                return won
                    ? "Skirmish win — solid practice for command flow and waves."
                    : "Skirmish loss — adjust production, rally, and stance, then try again.";
            }

            if (isFallbackOneMatchDemo)
            {
                return won
                    ? "한 판 데모 승리. 지휘 코어·덱·적 물결을 익히기에 적합합니다."
                    : "데모 패배. 생산·집결(Alt+우클릭)을 조정한 뒤 R 또는 아래 버튼으로 같은 데모를 다시 시작하십시오.";
            }

            return won
                ? "연습 전투 승리. 지휘 코어·덱·적 AI를 익히기에 적합합니다."
                : "연습 전투 패배. 생산·집결(Alt+우클릭)·전술을 바꿔 다시 도전하십시오.";
        }

        /// <summary>캠페인 본문 폴백</summary>
        public static string ResultBodyFallbackCampaign(bool won) =>
            won
                ? (IsKorean
                    ? "주 목표를 달성했습니다. 다음 작전을 준비하십시오."
                    : "Primary objective achieved. Prepare for the next operation.")
                : (IsKorean
                    ? "작전이 실패했습니다. 병력을 재정비한 뒤 같은 작전을 재시도하십시오."
                    : "Operation failed. Regroup and try this mission again.");

        /// <summary>연습 모드 안내(캠페인 진행 불변)</summary>
        public static string ResultNextStepPractice(bool isFallbackOneMatchDemo) =>
            isFallbackOneMatchDemo
                ? (IsKorean
                    ? "한 판 데모입니다. 캠페인 진행은 바뀌지 않습니다. 메인 메뉴에서 다시 고르십시오."
                    : "One-match demo — campaign progress is unchanged. Pick again from the main menu.")
                : (IsKorean
                    ? "연습 전투입니다. 캠페인 진행은 바뀌지 않습니다. 메인에서 캠페인 또는 데모를 다시 고르십시오."
                    : "Practice battle — campaign progress is unchanged. Choose again from the main menu.");

        /// <summary>캠페인 다음 단계 폴백</summary>
        public static string ResultNextStepCampaignDefaultKo =>
            IsKorean
                ? "캠페인은 메뉴에서 다음 작전을 고릅니다. 아래 「캠페인 메뉴로」로 돌아갑니다."
                : "Continue the campaign from the menu. Use 「Campaign menu」 below.";

        /// <summary>통계 아래 짧은 개발자 메모(톤만 유지, 중복 최소화)</summary>
        public static string ResultScreenFooterDevNoteKo =>
            IsKorean
                ? "문제가 있으면 종료 후 Unity 콘솔 로그를 확인하십시오."
                : "If something looks wrong, check the Unity console after closing.";

        // --- 좌측 전술 패널(전투 HUD) ---
        public static string HudLeftPanelEyebrow =>
            IsKorean ? "전술 패널" : "Tactical panel";

        public static string HudSkirmishFallbackTitle =>
            IsKorean ? "연습 전술 망" : "Practice tactical net";

        public static string HudSkirmishFallbackSubtitle =>
            IsKorean
                ? "생산·자원·집결을 한 패널에서 유지하십시오."
                : "Keep production, economy, and rally in one panel.";

        public static string HudCombatInputHintOneLine =>
            IsKorean
                ? "1~8 생산 · WASD 시야 · 우클릭 명령 · F 특수기 · Ctrl+A 전체 · F1 도움말"
                : "1-8 build · WASD pan · right-click orders · F ability · Ctrl+A all · F1 help";

        // --- 미니맵 한 줄 캡션 ---
        public static string MinimapCaptionOneLine =>
            IsKorean
                ? "전술 지도 · 클릭/드래그로 시야 · Shift+M 크기 · 좌측 「표식」은 범례(아군 티얼·적 앰버 톤)"
                : "Tactical map · click/drag to pan · Shift+M size · left 「markers」 opens legend (ally teal · enemy ember)";

        // --- 일시정지: 전투 조작 본문(입력 블록 아래) ---
        public static string PauseBattleControlsBody =>
            IsKorean
                ? "전투 조작\n" +
                  "· 이동: 지면 우클릭   · 공격: 적 우클릭\n" +
                  "· 특수기: F   · 집결: Alt + 우클릭\n" +
                  "· 전술 지도: 우하단 클릭/드래그\n" +
                  "· 일시정지: P (토글)   · 배속: [ ] · 숫자패드 ±\n" +
                  "· 도움말: F1   · 설정: O"
                : "In combat\n" +
                  "· Move: right-click ground   · Attack: right-click enemy\n" +
                  "· Ability: F   · Rally: Alt + right-click\n" +
                  "· Tactical map: bottom-right click/drag\n" +
                  "· Pause: P (toggle)   · Speed: [ ] · numpad ±\n" +
                  "· Help: F1   · Settings: O";

        // --- 상단 UGUI 캠페인 스트립(연습 제외) — 「데모」 대신 「작전」 톤 통일 ---
        public static string UguiCampaignStripPrefix =>
            IsKorean ? "작전" : "Operation";

        public static string UguiCampaignStripWhenNameMissing =>
            IsKorean ? "Battle Aces" : "Battle Aces";

        // --- 첫 캠페인 미션 체크리스트(온보딩) ---
        public static string FirstPlayChecklistTitle =>
            IsKorean ? "첫 작전 체크리스트" : "First operation checklist";

        public static string FirstPlayChecklistSubtitle =>
            IsKorean
                ? "F1·상단 목표 바와 겹치지 않게 최소만 표시합니다. 승리: 적 코어 격파. T/Y/U: 지휘 코어 강화."
                : "Minimal overlap with F1 and the top bar. Win by destroying the enemy core. T/Y/U: command core upgrades.";

        public static string FirstPlayChecklistHideButton =>
            IsKorean ? "다시 안 보기" : "Do not show again";

        public static string FirstPlayCheckProd => IsKorean ? "유닛 생산 (키 1~8)" : "Unit production (keys 1–8)";
        public static string FirstPlayCheckMove => IsKorean ? "지면 이동 (우클릭)" : "Move (right-click ground)";
        public static string FirstPlayCheckMinimap => IsKorean ? "전술 지도 클릭 (시야)" : "Tactical map click (camera)";
        public static string FirstPlayCheckRally => IsKorean ? "집결 (Alt+우클릭)" : "Rally (Alt+right-click)";
    }
}
