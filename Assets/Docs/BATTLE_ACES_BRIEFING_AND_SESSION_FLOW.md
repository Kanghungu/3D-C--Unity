# Battle Aces — 브리핑·승패·카메라·문구 흐름

한 판 톤 통일(A축)용 참고. 실제 문자열 단일 출처는 `Assets/Scripts/UI/DemoPresentationCopy.cs`.

## 용어

- 아군 거점: **지휘 코어** (본진·본대 등 혼용 지양)
- 적 승리 조건 대상: **적 코어**

## 브리핑

- **작전 시작**: Space / Enter / 좌클릭 (`BattleMissionFlow`)
- **카메라**: 브리핑이 떠 있는 동안 `RTSCameraController` 는 **Space 로 교전 포커스(시야 이동)** 를 하지 않음. Space 는 브리핑 확인과 겹치지 않게 막혀 있음.
- 상단 강조 줄 문구 접두: `DemoPresentationCopy.BriefingTopBarPrefixKo` 와 UGUI 상단 목표 바가 같은 톤.

## 전투 HUD

- 상단 **UGUI 목표 줄**이 켜진 동안(`BattleAcesObjectiveUgui`), 좌측 IMGUI 패널에는 주 목표 문구를 **반복하지 않고** 안내 한 줄만 표시 (`LeftHudObjectiveFromTopBarKo`).

## 일시정지

- 제목: `DemoPresentationCopy.PausePanelTitleKo`
- R / Esc 블록: `BuildPauseAndResultInputBlockKo()` (승패 카드와 같은 순서·톤)

## 승패

1. **오디오**: `ProceduralAudioUtility.PlayResultSting` (`BattleMissionFlow.OnMatchEnded`)
2. **결과 카드(IMGUI)**: `showResultOverlay` 후 `DrawResultScreen` — 제목 → 종료 사유 → (패배 시) 재시도 힌트 → **본문** → 통계 → 보조 목표 → 다음 단계 안내 → **R / Esc** → 짧은 개발 메모 → 버튼
3. **승리 카메라**: `BattleAcesVictoryPresentation` 은 스팅 직후 약 **0.12s** 양보 후 당김 연출

## 믹서 기본값

- `GameUserSettings.DefaultBattleAmbientVolume` / `DefaultResultStingVolume` — UI·스팅과의 균형용 기본값(플레이어 설정·PlayerPrefs가 있으면 그쪽이 우선).

## 마무리 체크리스트 — `DemoPresentationCopy` 연동

| 구간 | 파일 | 쓰는 문구(요지) |
|------|------|------------------|
| 브리핑·결과 카드·R/Esc 등 | `BattleMissionFlow.cs` | 브리핑 푸터, 폴백 본문, 결과 제목·본문·다음 단계·버튼 힌트 |
| 좌측 전술 패널(눈썹·폴백 제목·입력 한 줄) | `BattleAcesHudOverlay.cs` | `HudLeftPanelEyebrow`, `HudSkirmishFallback*`, `HudCombatInputHintOneLine`, 스커미시 결과 헤드라인 |
| 상단 UGUI 스트립(캠페인) | `BattleAcesObjectiveUgui.cs` | `UguiCampaignStripPrefix` / `UguiCampaignStripWhenNameMissing` |
| 미니맵 캡션 | `BattleAcesMinimap.cs` | `MinimapCaptionOneLine` |
| 일시정지 | `BattleAcesPauseOverlay.cs` | `PausePanelTitleKo`, `BuildPauseAndResultInputBlockKo`, `PauseBattleControlsBody` |
| 첫 작전 체크리스트 | `BattleAcesFirstPlayGuide.cs` | `FirstPlayChecklist*` / `FirstPlayCheck*` |
| 용어·장문 조작 설명 | `BattleAcesInGameHelp.cs` | 파일 내 직접 문자열(지휘 코어·적 코어 톤 유지) |

새 HUD/오버레이 문구를 넣을 때는 가능하면 `DemoPresentationCopy`에 한·영을 같이 두고 위 표처럼 소비처를 적어 두면 A축이 깨지지 않습니다.
