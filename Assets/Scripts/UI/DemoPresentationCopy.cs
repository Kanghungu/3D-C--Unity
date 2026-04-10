using Game.Settings;

namespace Game.UI
{
    /// <summary>
    /// Shared presentation copy for briefing, HUD, pause, result, and onboarding text.
    /// Keeps the Battle Aces demo voice consistent across IMGUI and UGUI surfaces.
    /// </summary>
    public static class DemoPresentationCopy
    {
        private static bool IsKorean => GameUserSettings.Language == GameLanguage.Korean;

        // --- Match goal / quick HUD copy ---
        public static string RoundGoalOneLineKo =>
            IsKorean
                ? "주 목표: 적 코어를 먼저 파괴하면 승리합니다. 지휘 코어가 먼저 무너지면 패배합니다."
                : "Primary objective: destroy the enemy core first to win. If your command core falls first, you lose.";

        public static string AfterMatchExitOneLineKo =>
            IsKorean
                ? "전투 종료 후 R로 같은 작전을 다시 시작하고 Esc로 메뉴로 돌아갑니다."
                : "After the battle: R restarts this operation, Esc returns to the menu.";

        public static string PracticeToastLineKo =>
            IsKorean
                ? "연습 전술: 적 코어까지 압박한 뒤 결과 화면에서 R로 같은 판을 반복해 감을 익히십시오."
                : "Practice: push to the enemy core, then press R on the result screen to repeat the same match.";

        public static string KeyboardOnlyNoticeKo =>
            IsKorean
                ? "조작: 이 빌드는 키보드와 마우스 기준입니다."
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

        /// <summary>Pause overlay title shared with the result-card tone.</summary>
        public static string PausePanelTitleKo =>
            IsKorean ? "일시정지 (P로 해제)" : "Paused (press P to resume)";

        /// <summary>Briefing footer hint for starting gameplay.</summary>
        public static string BriefingContinueFooterKo =>
            IsKorean
                ? "Space / Enter / 좌클릭으로 작전 개시 (브리핑 중 Space는 카메라 이동에 쓰이지 않습니다)"
                : "Space / Enter / left click to begin operation (Space does not pan the camera during briefing)";

        /// <summary>Top-bar prefix shared with the UGUI objective strip.</summary>
        public static string BriefingTopBarPrefixKo =>
            IsKorean ? "작전 목표" : "Operation objective";

        /// <summary>Left-HUD note that avoids duplicating primary objective text.</summary>
        public static string LeftHudObjectiveFromTopBarKo =>
            IsKorean
                ? "주 목표 문구는 화면 상단 목표 바를 따릅니다."
                : "Primary objective follows the top bar.";

        /// <summary>Explains the F1 help section role.</summary>
        public static string HelpF1SectionRoleLine =>
            IsKorean ? "F1 · 조작 요약 / 보조 목표와 팁" : "F1 · Controls / secondary objectives & tips";

        /// <summary>Reminder that the primary objective sits on the top bar.</summary>
        public static string HelpF1PrimaryOnTopBarLine =>
            IsKorean
                ? "주 목표 문구는 상단 목표 바(1차)에 표시됩니다."
                : "Primary objective text is on the top bar (tier 1).";

        /// <summary>Header for the lightweight secondary-hints section in F1 help.</summary>
        public static string HelpF1SecondaryHintsHeader =>
            IsKorean ? "보조 목표와 팁" : "Secondary hints";

        /// <summary>Fallback header for the mission section in F1 help.</summary>
        public static string HelpMissionThisOperationHeader =>
            IsKorean ? "이번 작전" : "This operation";

        public static string HelpSummarySectionTitle =>
            IsKorean ? "조작 요약" : "Quick reference";

        public static string HelpDetailToggleExpand =>
            IsKorean ? "자세히 보기 · 전체 조작 목록" : "More · full control list";

        public static string HelpDetailToggleCollapse =>
            IsKorean ? "접기 · 전체 목록 숨기기" : "Less · hide full list";

        public static string HelpF1FooterCloseHint =>
            IsKorean ? "다시 F1을 누르면 닫습니다." : "Press F1 again to close.";

        public static string HelpF1CloseButton =>
            IsKorean ? "닫기 (F1)" : "Close (F1)";

        /// <summary>Help panel title aligned with top-bar language.</summary>
        public static string HelpF1PanelTitle =>
            IsKorean ? "조작·목표 안내" : "Controls & objectives";

        /// <summary>Compact six-line F1 summary block.</summary>
        public static string BuildHelpCoreSummarySixLines()
        {
            if (!IsKorean)
            {
                return
                    "· Left HUD: credits, deck 1-8, build queue, rally (Alt+right-click ground), T/Y/U core upgrades, primary objective on the top bar\n" +
                    "· Select: click/drag allies only · right-click move/attack · Ctrl+A all · Esc clear\n" +
                    "· Camera: WASD · arrows · edge scroll · wheel zoom (no rotate) · Space / Home · comma = rally view\n" +
                    "· Tactical map (lower-right): click/drag · Ctrl+click nearby allies · Shift box select · Shift+M size\n" +
                    "· P pause · [ ] / numpad - + speed · O settings · V auto-target · H/G/B with units selected\n" +
                    "· F1 help · R restart only on the result screen · controls are fixed in this demo";
            }

            return
                "· 좌측 HUD: 자원, 덱 1~8, 생산 대기열, 집결(Alt+지면 우클릭), T/Y/U 지휘 코어 강화, 주 목표는 상단 바\n" +
                "· 선택: 좌클릭/드래그는 아군만 · 우클릭 이동/공격 · Ctrl+A 전체 · Esc 해제\n" +
                "· 카메라: WASD · 화살표 · 화면 가장자리 · 휠 줌(회전 없음) · Space / Home · 쉼표는 집결 지점 시야\n" +
                "· 전술 지도(우하단): 클릭/드래그 · Ctrl+클릭 근처 아군 · Shift 박스 선택 · Shift+M 크기\n" +
                "· P 일시정지 · [ ] / 숫자패드 - + 배속 · O 설정 · V 자동 표적 · H/G/B는 유닛 선택 시 사용\n" +
                "· F1 도움말 · R 재시작은 결과 화면에서만 · 이 데모는 키 고정";
        }

        /// <summary>Settings hint for the demo's fixed-input scope.</summary>
        public static string SettingsInputNoRebindHint =>
            IsKorean
                ? "Battle Aces 조작은 이 빌드에서 고정입니다. 키 리바인딩과 Steam 입력 재설정은 데모 범위 밖입니다."
                : "Battle Aces controls are fixed in this build. Key rebinding and Steam input remap are outside the Battle Aces demo scope.";

        /// <summary>Short explanation of the three graphics presets shown in settings.</summary>
        public static string SettingsGraphicPresetSectionHint =>
            IsKorean
                ? "저사양: 그림자 최소. 균형: 기본 거리. 고품질: 더 먼 그림자와 부드러운 입자 표현."
                : "Performance: shadows off. Balanced: default shadow distance. Quality: farther shadows and soft particles.";

        /// <summary>SelectionGrid labels: Performance / Balanced / Quality.</summary>
        public static string[] SettingsGraphicPresetGridLabels =>
            IsKorean
                ? new[] { "저사양 (성능)", "균형 (기본)", "고품질 (그래픽)" }
                : new[] { "Performance", "Balanced", "Quality" };

        /// <summary>Pause overlay note explaining that keys are fixed.</summary>
        public static string PauseControlsFixedKeysNotice =>
            IsKorean
                ? "조작은 Battle Aces 데모에서 고정되어 있으며, 이 화면에서는 리바인딩을 지원하지 않습니다."
                : "Controls are fixed for the Battle Aces demo; you cannot rebind keys here.";

        /// <summary>Expanded F1 help body for the full controls list.</summary>
        public static string BuildBattleAcesF1ExpandedDetailBody()
        {
            if (!IsKorean)
            {
                return
                    "· Battle Aces demo scope\n" +
                    "· Fixed controls: no key rebinding inside this demo.\n" +
                    "P: pause · [ ] or numpad - +: speed steps\n" +
                    "O: settings (volume, three graphics presets, UI scale, fullscreen)\n" +
                    "Left click / drag: allies only · Ctrl+A: all living allies · Esc: clear selection\n" +
                    "Right click: move / attack enemies & targets\n" +
                    "Alt + right-click ground: rally · comma: camera to rally point\n" +
                    "1-8: deck build orders (short reject sound if blocked)\n" +
                    "T / Y / U: command core production / armor / economy upgrades (costs on left HUD)\n" +
                    "H / G / B: hold / defend (near ally point) / retreat with units selected\n" +
                    "Camera: WASD · arrows · edge scroll · wheel zoom (no keyboard rotate, WASD ignored while Ctrl is held)\n" +
                    "Space: battle view · Home: command core\n" +
                    "Tactical map: click/drag pan · Ctrl+click nearby allies · Shift+drag box select · Shift+M size\n" +
                    "V: auto-target toggle · F1: this panel\n" +
                    "R: restart same scene only on the victory/defeat screen\n" +
                    "Ctrl+F2-F5: assign control groups · F2-F5: recall (double-tap: camera to that group)\n" +
                    "· F1 is help-only, so there is no F1 control group.\n\n" +
                    "· Not in this demo\n" +
                    "Multiplayer, long tech trees, buildings beyond the command core, click-select buildings for orders, campaign-external map editing, key rebinding, and similar systems.";
            }

            return
                "· Battle Aces 데모 범위\n" +
                "· 조작은 고정이며, 이 데모 안에서는 키 리바인딩을 지원하지 않습니다.\n" +
                "P: 일시정지 · [ / ] 또는 숫자패드 - +: 배속 단계\n" +
                "O: 설정(볼륨, 그래픽 3단계, UI 크기, 전체 화면)\n" +
                "좌클릭/드래그: 아군만 선택 · Ctrl+A: 생존 아군 전체 · Esc: 선택 해제\n" +
                "우클릭: 이동 / 적 및 목표 공격\n" +
                "Alt+지면 우클릭: 집결 · 쉼표: 집결 지점으로 카메라 이동\n" +
                "1~8: 덱 생산 명령(막히면 짧은 거절음 재생)\n" +
                "T / Y / U: 지휘 코어 생산 / 장갑 / 경제 강화(비용은 좌측 HUD에 표시)\n" +
                "H / G / B: 대기 / 방어(가까운 아군 거점 기준) / 후퇴, 유닛 선택 시 사용\n" +
                "카메라: WASD · 화살표 · 화면 가장자리 · 휠 줌(키보드 회전 없음, Ctrl 중 WASD 이동 억제)\n" +
                "Space: 전투 시야 · Home: 지휘 코어\n" +
                "전술 지도: 클릭/드래그 이동 · Ctrl+클릭 근처 아군 · Shift+드래그 박스 선택 · Shift+M 크기\n" +
                "V: 자동 표적 토글 · F1: 이 패널\n" +
                "R: 승패 결과 화면에서만 같은 씬 재시작\n" +
                "Ctrl+F2~F5: 부대 지정 · F2~F5: 부대 호출(두 번 누르면 해당 부대로 카메라 이동)\n" +
                "· F1은 도움말 전용이라 F1 부대 그룹은 없습니다.\n\n" +
                "· 이 데모에 없는 것\n" +
                "멀티플레이, 장기 테크 트리, 추가 건물 건설, 건물 직접 선택 명령, 외부 맵 편집, 키 리바인딩 등";
        }

        /// <summary>Shared input block used by pause and result surfaces.</summary>
        public static string BuildPauseAndResultInputBlockKo()
        {
            return IsKorean
                ? "입력\n· R · 같은 작전 즉시 재시작\n· Esc · 메뉴로 복귀"
                : "Input\n· R · restart this operation\n· Esc · return to menu";
        }

        /// <summary>Skirmish result headline used by the IMGUI fallback overlay.</summary>
        public static void GetSkirmishResultHeadline(bool victory, out string headline)
        {
            headline = victory
                ? (IsKorean ? "승리 · 적 코어 격파" : "Victory · enemy core destroyed")
                : (IsKorean ? "패배 · 지휘 코어 붕괴" : "Defeat · command core lost");
        }

        /// <summary>Result screen title.</summary>
        public static string GetResultScreenTitle(bool victory) =>
            victory
                ? (IsKorean ? "작전 승리" : "Operation victory")
                : (IsKorean ? "작전 실패" : "Operation failed");

        /// <summary>Fallback briefing body used when custom text is unavailable.</summary>
        public static string BuildDefaultBriefingBodyKo(string missionDisplayName, string primaryObjectiveLine)
        {
            if (!IsKorean)
            {
                return $"Operation: {missionDisplayName}\n\nPrimary objective: {primaryObjectiveLine}\n\nHold ritual silence, dress the line, and press the enemy salient.";
            }

            return $"작전명: {missionDisplayName}\n\n주 목표: {primaryObjectiveLine}\n\n의식을 정비하고 전열을 갖춘 뒤, 적의 돌출부를 압박하십시오.";
        }

        /// <summary>Result-screen hint for the R key.</summary>
        public static string ResultScreenRKeyLine(bool isFallbackOneMatchDemo, bool skirmishPractice)
        {
            if (!IsKorean)
            {
                if (isFallbackOneMatchDemo)
                {
                    return "R · restart this demo (same as the left button)";
                }

                return skirmishPractice
                    ? "R · restart this skirmish (same as the left button)"
                    : "R · restart this mission (same as the left button)";
            }

            if (isFallbackOneMatchDemo)
            {
                return "R · 같은 데모 즉시 재시작 (좌측 버튼과 동일)";
            }

            return skirmishPractice
                ? "R · 같은 스커미시 즉시 재시작 (좌측 버튼과 동일)"
                : "R · 같은 미션 즉시 재시작 (좌측 버튼과 동일)";
        }

        /// <summary>Result-screen hint for the Esc key.</summary>
        public static string ResultScreenEscLine(bool skirmishPractice) =>
            skirmishPractice
                ? (IsKorean
                    ? "Esc · 메인 메뉴로 (우측 버튼과 동일)"
                    : "Esc · main menu (same as the right button)")
                : (IsKorean
                    ? "Esc · 캠페인 메뉴로 (우측 버튼과 동일)"
                    : "Esc · campaign menu (same as the right button)");

        /// <summary>Fallback result body for demo/skirmish practice modes.</summary>
        public static string ResultBodyFallbackPractice(bool won, bool isFallbackOneMatchDemo)
        {
            if (!IsKorean)
            {
                if (isFallbackOneMatchDemo)
                {
                    return won
                        ? "Demo win · good for learning command core, deck, and enemy waves."
                        : "Demo loss · adjust production and rally (Alt+right-click), then press R or the button below.";
                }

                return won
                    ? "Skirmish win · solid practice for command flow and waves."
                    : "Skirmish loss · adjust production, rally, and stance, then try again.";
            }

            if (isFallbackOneMatchDemo)
            {
                return won
                    ? "데모 승리. 지휘 코어, 덱, 적 파동 흐름을 익히기에 적합합니다."
                    : "데모 패배. 생산, 집결(Alt+우클릭), 전열을 조정한 뒤 R 또는 아래 버튼으로 다시 시작하십시오.";
            }

            return won
                ? "연습 전투 승리. 지휘 흐름과 파동 대응을 익히기에 적합합니다."
                : "연습 전투 패배. 생산, 집결, 전투 자세를 바꿔 다시 도전하십시오.";
        }

        /// <summary>Fallback result body for campaign missions.</summary>
        public static string ResultBodyFallbackCampaign(bool won) =>
            won
                ? (IsKorean
                    ? "주 목표를 달성했습니다. 다음 작전을 준비하십시오."
                    : "Primary objective achieved. Prepare for the next operation.")
                : (IsKorean
                    ? "작전에 실패했습니다. 전열을 재정비한 뒤 같은 작전에 다시 투입하십시오."
                    : "Operation failed. Regroup and try this mission again.");

        /// <summary>Next-step guidance for practice or one-match demo modes.</summary>
        public static string ResultNextStepPractice(bool isFallbackOneMatchDemo) =>
            isFallbackOneMatchDemo
                ? (IsKorean
                    ? "한 판 데모입니다. 캠페인 진행은 바뀌지 않습니다. 메인 메뉴에서 다시 고르십시오."
                    : "One-match demo · campaign progress is unchanged. Pick again from the main menu.")
                : (IsKorean
                    ? "연습 전투입니다. 캠페인 진행은 바뀌지 않습니다. 메인 메뉴에서 캠페인 또는 데모를 다시 고르십시오."
                    : "Practice battle · campaign progress is unchanged. Choose again from the main menu.");

        /// <summary>Default next-step line for campaign missions.</summary>
        public static string ResultNextStepCampaignDefaultKo =>
            IsKorean
                ? "캠페인 메뉴에서 다음 작전을 고르십시오. 아래 버튼으로 메뉴로 돌아갑니다."
                : "Continue the campaign from the menu. Use the campaign menu button below.";

        /// <summary>Small diagnostic footer shown under the result card.</summary>
        public static string ResultScreenFooterDevNoteKo =>
            IsKorean
                ? "문제가 있으면 종료 후 Unity 콘솔 로그를 확인하십시오."
                : "If something looks wrong, check the Unity console after closing.";

        // --- Tactical HUD / minimap copy ---
        public static string HudLeftPanelEyebrow =>
            IsKorean ? "전술 패널" : "Tactical panel";

        public static string HudSkirmishFallbackTitle =>
            IsKorean ? "연습 전술 망" : "Practice tactical net";

        public static string HudSkirmishFallbackSubtitle =>
            IsKorean
                ? "생산, 경제, 집결을 한 패널에서 유지하십시오."
                : "Keep production, economy, and rally in one panel.";

        public static string HudCombatInputHintOneLine =>
            IsKorean
                ? "1~8 생산 · WASD 시야 · 우클릭 명령 · F 특수기 · Ctrl+A 전체 · F1 도움말"
                : "1-8 build · WASD pan · right-click orders · F ability · Ctrl+A all · F1 help";

        public static string MinimapCaptionOneLine =>
            IsKorean
                ? "전술 지도 · 클릭/드래그 시야 · Shift+M 크기 · 좌측 마커 버튼은 범례(아군 티얼 · 적 앰버)"
                : "Tactical map · click/drag to pan · Shift+M size · left marker button opens the legend (ally teal · enemy ember)";

        // --- Pause overlay battle instructions ---
        public static string PauseBattleControlsBody =>
            IsKorean
                ? "전투 조작\n" +
                  "· 이동: 지면 우클릭   · 공격: 적 우클릭\n" +
                  "· 특수기: F   · 집결: Alt + 우클릭\n" +
                  "· 전술 지도: 우하단 클릭/드래그\n" +
                  "· 일시정지: P   · 배속: [ ] / 숫자패드 - +\n" +
                  "· 도움말: F1   · 설정: O"
                : "In combat\n" +
                  "· Move: right-click ground   · Attack: right-click enemy\n" +
                  "· Ability: F   · Rally: Alt + right-click\n" +
                  "· Tactical map: bottom-right click/drag\n" +
                  "· Pause: P   · Speed: [ ] / numpad - +\n" +
                  "· Help: F1   · Settings: O";

        // --- UGUI campaign strip / onboarding copy ---
        public static string UguiCampaignStripPrefix =>
            IsKorean ? "작전" : "Operation";

        public static string UguiCampaignStripWhenNameMissing =>
            IsKorean ? "Battle Aces" : "Battle Aces";

        public static string FirstPlayChecklistTitle =>
            IsKorean ? "첫 작전 체크리스트" : "First operation checklist";

        public static string FirstPlayChecklistSubtitle =>
            IsKorean
                ? "F1과 상단 목표 바와 겹치지 않게 최소한만 표시합니다. 승리 조건은 적 코어 격파이며, T/Y/U는 지휘 코어 강화입니다."
                : "Minimal overlap with F1 and the top bar. Win by destroying the enemy core. T/Y/U: command core upgrades.";

        public static string FirstPlayChecklistHideButton =>
            IsKorean ? "다시 안 보기" : "Do not show again";

        public static string FirstPlayCheckProd => IsKorean ? "유닛 생산 (키 1~8)" : "Unit production (keys 1-8)";
        public static string FirstPlayCheckMove => IsKorean ? "이동 (우클릭 지면)" : "Move (right-click ground)";
        public static string FirstPlayCheckMinimap => IsKorean ? "전술 지도 클릭 (카메라)" : "Tactical map click (camera)";
        public static string FirstPlayCheckRally => IsKorean ? "집결 (Alt+우클릭)" : "Rally (Alt+right-click)";
    }
}
