# Battle Aces — 가독성(C축): 색·계층·문구

코드 단일 출처: `BattleAcesReadability`(미니맵·코어 HP·하단 HUD), `ImGuiGameUi`(자원 스트립·공용 팔레트), `DemoPresentationCopy`(목표/F1 문구).  
아트 톤·포인트 컬러: `BattleAcesArtDirection`, `BATTLE_ACES_ART_DIRECTION.md`.

## 1. 자원 스트립(좌측 IMGUI)

| 요소 | 위치(대략) | `ImGuiGameUi` 색 | 역할 |
|------|------------|------------------|------|
| 보유 크레딧 | strip 좌측, 큰 글자 | `ResourceHighlight` | 1차 시선 |
| +수입/s | strip 우측 상단 줄 | `TextMuted` | 2차 |
| 적 추정 | strip 우측 하단 줄 | `TextMuted` | 2차 |
| 배경 | `DrawPanelFrame` fill | `EconomyStripPanelBg` | 대비 |
| 테두리 | frame border | `EconomyStripBorder` | 구분 |

## 2. 목표 문구 계층

| 계층 | 위치 | 내용 |
|------|------|------|
| 1차 | 상단 UGUI (`BattleAcesObjectiveUgui`) | 작전 제목·주 목표 한 줄·보조(동적) |
| 2차 | F1 (`BattleAcesInGameHelp`) | 상단 바가 켜진 전투 중: **주 목표 문구 생략**, 「보조 목표·팁」+ `GetGameplayHint` 만 |
| 좌측 패널 | `BattleAcesHudOverlay` | 상단 바 ON 시 `LeftHudObjectiveFromTopBarKo` 한 줄로 대체 |

## 3. 코어 HP 위험 구간 → 반영 위치

상수는 `BattleAcesReadability` (`PlayerCoreWarningNormalized` 0.5 / `PlayerCoreCriticalNormalized` 0.28, 적 코어는 `EnemyCore*` 0.55 / 0.32).

| 구간 | 미니맵 점 | 하단 선택 HUD(코어만) |
|------|-----------|------------------------|
| 정상 | 기본 색·기본 크기 | `TextTitle` |
| 경고 | 중간 색·약간 큰 점 | `AccentGold` |
| 위험 | 강한 경고색·가장 큰 점 | `DefeatTint` |

※ 승리 임박 **사운드**(1회)는 `BattleAcesReadability.EnemyCoreVictoryImminentAudioNormalized` (**0.22**) — 시각 위험(≤32%)보다 이른 청각 힌트. `BattleAcesMatchController` 가 재생.

## 4. 미니맵 점 — 일반 / 색약 / 코드·아트 출처

일반 모드는 `BattleAcesArtDirection.PointTeal`(아군·티얼 축)과 `EnemyEmber`(적)를 그대로 쓰고, HP 구간만 크기·보간으로 강조합니다.  
색약 모드(`GameUserSettings.ColorblindFriendlyMinimap`)는 **같은 규율**(티얼·앰버 축)을 유지한 채 파랑·시안 / 주황·노랑 쪽으로만 더 벌려 적록 혼동을 줄입니다.

| 대상 | 일반 모드 | 색약 모드 | 코드(`BattleAcesArtDirection`) |
|------|-----------|-----------|--------------------------------|
| 아군 코어 점 | 티얼 → HP↓ 앰버 느낌 보간 | `MinimapColorblindAllyCore*` (Ok/Warn/Critical) | 위 상수 3종 |
| 적 코어 점 | 앰버 계열 | `MinimapColorblindEnemyCore*` | 위 상수 3종 |
| 아군 유닛 점 | 티얼·건메탈 보간, 선택 시 크게 | `MinimapColorblindAllyUnitNormal` / `Selected` | α는 `BattleAcesReadability`에서 조정 |
| 적 유닛 점 | 앰버 보간 | `MinimapColorblindEnemyUnit` | α 동일 |

범례 문구: `BattleAcesReadability.BuildMinimapLegendCoreLine` / `BuildMinimapLegendUnitLine` + 고정 4번째 줄(체력 낮음 안내).  
설정 패널(O) 「접근성」과 F1 요약 마지막 줄에 **Battle Aces 조작 키 고정(재바인딩 없음)** 안내가 있습니다.

## 5. 시간·일시정지·배속

- **한 줄**: 좌측 패널 맨 아래 — `RtsTimeControl.GetHudTimeStatusLine()` (한·영, `·` 구분 통일).
- 일시정지 패널 본문은 조작 요약용으로 별도(`BattleAcesPauseOverlay` + `DemoPresentationCopy`).

## 6. 교전 중 유닛 시각 계층

과한 메시 수정 없이 유지:

- **미니맵**: 선택 유닛만 더 크고 밝은 점.
- **월드**: 기존 `SelectableUnit` 선택 링·머티리얼 밝기.
- **하단**: 단일 선택 시 `AccentGold` 제목 등 기존 패널.

추가 아웃라인/펄스는 넣지 않음(성능·가독성 밸런스).

---

## 7. 720p·색약 회귀 체크리스트 (Battle Aces)

Game 뷰를 **1280×720**으로 맞춘 뒤 한 판 분량 확인합니다. 1080p만 보면 놓치기 쉬운 항목입니다.

- [ ] 좌측 전술 패널이 화면 밖으로 잘리지 않음(`BattleAcesHudOverlay` — 가로 `Screen.width` 클램프).
- [ ] 자원 숫자·덱 그리드가 패널 안에 읽힘(세로 720 이하에서 폰트·밀도 완화).
- [ ] 하단 선택 정보 패널이 미니맵·토스트와 심하게 겹치지 않음(`BattleAcesSelectionInfoHud` — 좁은 가로에서 칩 레이아웃).
- [ ] 우하단 전술 지도 크기·여백이 720에서도 클릭 가능(`BattleAcesMinimap`).
- [ ] 설정(O)에서 **색약 친화 미니맵** 켠 뒤, 범례 4줄·코어/유닛 점이 혼동 없이 구분됨(`BattleAcesArtDirection.MinimapColorblind*`).
- [ ] `weekly_compare_1920x1080.png` / `weekly_compare_1280x720.png` 갱신 후 지면·HUD·미니맵 프레임이 팔레트에서 벗어나지 않는지 비교(`WEEKLY_CAPTURE.md`).

---

## 마무리 체크리스트 — 소비 파일

| 규칙 | 주요 파일 |
|------|-----------|
| 자원 스트립 색 | `ImGuiGameUi` `EconomyStrip*` · `BattleAcesHudOverlay.DrawEconomyStrip` |
| 목표 1차/2차·F1 문구 | `BattleAcesObjectiveUgui` · `DemoPresentationCopy` `HelpF1*` · `BattleAcesInGameHelp` |
| 코어 HP·미니맵 점 | `BattleAcesReadability` · `BattleAcesMinimap` · `BattleAcesSelectionInfoHud` |
| 시간 한 줄 | `RtsTimeControl.GetHudTimeStatusLine` · `BattleAcesHudOverlay.DrawTimeStatus` |
| 일시정지 조작 블록 | `DemoPresentationCopy.PauseBattleControlsBody` · 고정 키 `PauseControlsFixedKeysNotice` |
| F1 요약·전개 본문 | `BuildHelpCoreSummarySixLines` · `BuildBattleAcesF1ExpandedDetailBody` |
| 설정 입력 고지 | `DemoPresentationCopy.SettingsInputNoRebindHint` (`GameSettingsMenuOverlay`) |

**회귀(짧게):** 영어 설정에서 F1 요약·전개 본문·닫기·토글이 영문인지, 상단 바 켜진 전투 중 F1 이 주 목표를 반복하지 않는지, 미니맵 범례 4줄이 색약 옵션과 맞는지, O 설정 「입력 안내」·일시정지 패널에 고정 키 문구가 보이는지 확인.
