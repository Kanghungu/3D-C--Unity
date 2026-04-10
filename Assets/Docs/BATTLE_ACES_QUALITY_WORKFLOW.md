# Battle Aces · 품질 로드맵 실행 워크플로

전방위 퀄 로드맵(체크리스트 1~200)을 저장소에서 실행할 때 쓰는 **요약 절차**입니다. 세부 항목은 팀 내 플랜 문서를 따릅니다.

## 스프린트 스코프 한 줄 (복붙용)

> 데모 N차: 금속·잿빛 안개·티얼 단일 포인트 유지. 새 악센트 색/발광 소재 추가 없음. **이번 주 고칠 것 1~2개만** (조명 / UI / 지형 / 카메라 중 택일).

코드 기본 문장: `BattleAcesArtQualityScope.ExampleScopeLockLineKorean`

에디터: **Game → Battle Aces → 아트 → 스프린트 스코프 한 줄(템플릿) 클립보드 복사** — 위 예시 문장 + `BattleAcesDemoQualitySprintFocus.HeroScreenScopeLineKorean`(이번 스프린트 화면 포커스)를 한 번에 복사합니다. (`BattleAcesArtQualityScope.BuildScopeLockClipboardKorean`)

## 스프린트 시작 체크 (약 30초)

매 스프린트(또는 이슈/브랜치) 시작 시 한 번만 점검합니다. **방향이 엇나가지 않게** 고정 축과 튜닝 축을 구분한 뒤, 이번 주에는 **1~2스팟만** 고칩니다.

1. `BATTLE_ACES_ART_DIRECTION.md` 한 줄과 팔레트 표를 다시 읽는다.
2. 아래 **품질 축 분류** 표에서 오늘 손댈 일이 **고정 축**인지 **튜닝 축**인지 구분한다(고정 축은 문구·절차 위주, 팔레트 추가는 금지).
3. 에디터 **Game → Battle Aces → 아트 → 스프린트 시작 가드레일(고정·튜닝 축) 클립보드 복사** — 이슈 본문 상단에 붙여 두면 무한 폴리싱을 줄일 수 있다. (`BattleAcesArtQualityScope.BuildSprintStartGuardrailsClipboardKorean`)

## 품질 축 분류 (종류별 변동)

“Battle Aces처럼” 가려면 **정체성·절차는 고정**하고, **숫자·순서·에셋은 반복 튜닝**으로 둡니다. 전부 뒤집히지 않으며, 아래처럼 종류가 다릅니다.

### 고정 축 (잘 안 뒤집힘)

| 항목 | 설명 |
|------|------|
| 아트 한 줄 | `BATTLE_ACES_ART_DIRECTION.md` · `BattleAcesArtDirection.OneLinerKorean` |
| 팔레트 규율 | 티얼 단일 포인트·적은 `EnemyEmber` — 새 강채도 악센트·발광 남발 금지 |
| 폴더·레이어 경계 | `AGENTS.md` — `BattleAces` / `Prototype` / `Campaign` 역할 |
| 집 테스트 루프 | 약 10초 캡처·주간 **1~2스팟만** 수정 (`ReferenceScreenshots/` · `BattleAcesArtQualityScope`) |
| 데모 우선순위 | 한 판 루프·가독성 — 대량 콘텐츠·멀티 후순위 |

### 반복·후순위 축 (종류별로 바뀜)

| 항목 | 설명 |
|------|------|
| 대기·안개·조명 수치 | `BattleAcesClassicDuelAtmosphereTuning` · 사용자 안개 슬라이더 |
| 이미터·머티리얼 | `ReadablePrimitiveMaterialUtility` 클램프·프리미티브 튜닝 |
| UI 밀도·UGUI 이관 순서 | `BATTLE_ACES_UGUI_BOUNDARY.md` — 화면마다 순서 조정 가능 |
| 카메라 줌·쉐이크 | `RTSCameraController` 등 **세션별** 미세 조정 |
| Phase 8+ | 저폴리 메시·VFX·커스텀 셰이더 — 시간·레퍼에 따라 도입/보류 |
| 성능·빌드 | Profiler·GC·IL2CPP·Steam 패키지 — **측정 후** 결정 |

### Phase 8 에셋 게이트 (저폴리·얇은 VFX)

방향성을 유지한 채 메시·파티클로만 퀄을 올릴 때의 **착수 조건**입니다. 코드 상수: `BattleAcesArtQualityScope.Phase8AssetGateReminderKorean`.

- `target_sprint_reference.png`와 주간 해상도 쌍(`weekly_compare_1920x1080.png` / `weekly_compare_1280x720.png`)이 최소 1회 이상 갱신·비교된 뒤(레퍼와 나란히 볼 것).
- 외부 무드보드 레퍼 2~3장이 폴더에 있고, **티얼 단일 포인트·앰버 보조** 규율을 깨지 않는다는 합의가 있을 것.
- 한 스프린트에 **소량**(유닛 1종·코어 링·얇은 히트 스파크 등 1~2종)만 — 새 강채도 악센트·풀커스텀 PBR 셰이더 대량 투입은 보류.

## 전방위 퀄 로드맵 진행 상태 (저장소 기준)

체크리스트 1~200 전항을 “완료”로 두지 않고, **이미 반영된 것**과 **사람/도구가 필요한 것**만 구분합니다.

### 반영됨 (코드·문서)

- **Phase0**: `ReferenceScreenshots/WEEKLY_CAPTURE.md`(1080+720 파일명 코드 상수 고정), `README.md`, `UI_LAYER_ORDER.md`, 본 문서, `BATTLE_ACES_ART_DIRECTION.md` 워크플로 링크, 에디터 **스프린트 스코프 한 줄(템플릿) 클립보드 복사**
- **Phase1**: `BattleAcesClassicDuelAtmosphereTuning` Bloom/LUT 규율 remarks·Trilight `AmbientIntensity`/방향광 스케일 미세 튜닝, `ReadablePrimitiveMaterialUtility.EmissionIntensityClampMax`, 메뉴 카메라 배경을 `BattleAcesArtDirection` 축으로 통일(`BattleAcesWorldPresentation`), Classic Duel 방향광 소프트 그림자·바이어스 클램프
- **Phase2**: `BattleAcesSceneBootstrapper` 지면 `ReadablePrimitiveMaterialUtility` 베이스, `BattleArenaLayoutBootstrap` 장애물 팔레트 정렬, NavMesh 소스 0건 시 명시적 오류 로그, RTS `SetHeightClamp` 하한 5
- **Phase3**: `BattleAcesCommandCoreVisuals` hull 이미터 소폭, `PrototypeEntityVisualFactory` 팔레트 동기화 주석
- **Phase4**: `BattleAcesHudCaptureMode`·F12, 전투 IMGUI 크롬 숨김 연동, 좌측 패널 가로 클램프, 720p 미니맵 축소, `BattleAcesDevelopmentHud` GUIStyle 캐시, `BattleAcesUguiScaleUtility`로 목표/결과 UGUI와 `UiScale01` 연동, F8/F9 스토어 캡처 카메라 프리셋
- **Phase5**: `RTSCameraController` 전투 쉐이크·`ApplyPresentationView`, 승패 쉐이크·스팅 볼륨 소폭 절제(`BattleAcesCombatJuice`·`ProceduralAudioUtility`)
- **Phase6**: 설정 패널 **3단 그래픽 프리셋**(저사양/균형/고품질) + 변경 시 `RefreshBattleFogIfClassicDuelActive`, `GameUserSettings.ApplyGraphicQualityPreset` 그림자 거리 상수화

**그래픽 프리셋 ↔ 그림자 (코드 기준 한 줄 표)**

| 프리셋 | 그림자 | `shadowDistance` (대략) |
|--------|--------|-------------------------|
| 저사양 (Performance) | 끔 | 18 |
| 균형 (Balanced) | 전부 | 120 |
| 고품질 (High) | 전부 | 168 |

### 미완·수동 (로드맵 원문 기준)

- 레퍼/`target_sprint_reference.png` 등 **이미지 자산** 직접 배치
- Unity **Profiler**로 CPU 스파이크·GC·Canvas Rebuild 점검 (항목 121~123) — 절차는 `Assets/Docs/PROFILER_PASS_TEMPLATE.md`
- **Phase8**(저폴리·VFX 등 에셋 의존)은 **레퍼 스샷 수렴 후** 별도 스프린트에서 진행
- **Phase7~10**: 미션 카피 전수, 크레딧·법적 화면, IL2CPP·Steam 제출, 스토어 스샷·GIF 등

## 해상도 쌍 캡처 (1080p + 720p, 파일명 고정)

동일 플레이 구간을 **1920×1080**과 **1280×720**에서 각각 `Assets/Docs/ReferenceScreenshots/`에 저장합니다.

- `BattleAcesArtQualityScope.WeeklyCompareScreenshotFileName1080p` → **`weekly_compare_1920x1080.png`**
- `BattleAcesArtQualityScope.WeeklyCompareScreenshotFileName720p` → **`weekly_compare_1280x720.png`**

Unity Game 뷰 해상도를 위 픽셀과 맞춘 뒤 캡처합니다. 체크리스트 클립보드 복사 메뉴에도 동일 규칙이 포함됩니다.

## 색약·미니맵 검수

`GameUserSettings`에서 색약 친화 미니맵을 켠 뒤 한 판 플레이. 규칙: `BATTLE_ACES_READABILITY.md` §4, 색상 상수 `BattleAcesArtDirection.MinimapColorblind*`.

## 스토어/캡처 단축키 (에디터·Development 빌드)

- **F10**: 진단 패널 토글 (`BattleAcesDevelopmentHud`)
- **F12**: 전투 IMGUI 크롬 숨김/복구 (`BattleAcesHudCaptureMode`) — 스크린샷용

## 회귀 스모크 (10줄)

1. 카메라 이동·회전·줌
2. 유닛 선택·이동
3. 적 조우·자동 교전
4. 피해·사망
5. 승패까지 도달
6. 재시작 또는 메뉴 복귀
7. `Time.timeScale` 정상
8. 720p에서 좌측 패널·버튼 가림 없음
9. 그래픽 프리셋 3단(저사양/균형/고품질) 전환 후 전투 씬 안개·그림자 이상 없음
10. 캠페인 메뉴 진입 시 안개/포그 해제(`BattleAcesWorldPresentation`)

## 런치 전 최소 (Phase9 요약)

- 옵션: 볼륨·감도·전체화면·품질
- `IL2CPP` 스모크 빌드 1회
- 스크린샷 5장 + 짧은 GIF 2개 가이드(`ReferenceScreenshots`·카메라 프리셋)
- 알려진 이슈 5줄 이내
- OSS/폰트 고지 txt

## Git LFS / 용량

레퍼 이미지 PNG는 **장당 수 MB 이하**를 권장. 대용량은 LFS 정책에 맞게 관리합니다.

## 멀티 디스플레이·멀티플레이

비범위 — 문서·이슈에 명시만 합니다.
