# Battle Aces · UI 레이어 순서 (IMGUI vs UGUI)

전방위 퀄 로드맵 Phase0 — 브리핑/결과/전투 HUD가 겹칠 때 **어느 쪽이 위인지** 한눈에 고정합니다.

| 구역 | 구현 | 대략적 순서 / 비고 |
|------|------|-------------------|
| 전투 좌측 명령 패널 | `BattleAcesHudOverlay` · IMGUI | `BeginScaledGui` + 일반 depth |
| 선택 정보 패널 | `BattleAcesSelectionInfoHud` · IMGUI | 동일 스택 |
| 미니맵 | `BattleAcesMinimap` · IMGUI | 동일 스택 |
| 일시정지 딤 | `BattleAcesPauseOverlay` · IMGUI | 필요 시 depth 조정 |
| 개발 진단 패널 | `BattleAcesDevelopmentHud` · IMGUI | `GUI.depth = -2000` (최상단) |
| 상단 목표 바 | `BattleAcesObjectiveUgui` · UGUI | Canvas `sortingOrder = 24` |
| 승패 결과 카드 | `BattleAcesResultUgui` · UGUI | Canvas `sortingOrder = 40` (목표 바보다 위) |
| 브리핑 / 미션 흐름 전체 화면 | `BattleMissionFlow` · IMGUI | 결과 지연 구간은 `BattleAcesHudOverlay`와 타이밍 맞춤 |

**캡처용**: 에디터·Development 빌드에서 F12로 전투용 IMGUI 크롬(좌측 패널·미니맵 등) 숨김 — `BattleAcesHudCaptureMode` · 진단 패널 안내 문구 참고.
