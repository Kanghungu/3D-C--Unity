# Battle Aces · 런치 패키지·유지보수 (Phase7~10 요약)

전방위 퀄 로드맵의 **콘텐츠·출시·운영** 구간을 한 파일로 묶은 체크리스트입니다. 세부 카피·미션 데이터는 `MissionDefinition`·`DemoPresentationCopy` 쪽을 직접 편집합니다.

## Phase7 — 미션·카피·톤

- 대표 미션(폴백·스커미시·캠페인 1개)마다 **첫 60초** 난이도·HUD 문구가 같은 세계관 톤인지 확인.
- 상단 목표 UGUI(`BattleAcesObjectiveUgui`)와 좌측 IMGUI 패널 **문구 중복**이 늘지 않게 유지.
- 튜토리얼/인게임 도움말 밀도 — `BattleAcesFirstPlayGuide`·`BattleAcesInGameHelp` 한 번에 한 메시지 원칙.

## Phase8 — 선택적 메시·VFX (비용 큰 항목)

### Phase8 게이트(진입 조건)

아래를 만족하기 전에는 **메시 스왑·텍스처 세트·VFX 그래프**에 시간을 쓰지 않는 것을 권장합니다.

- [ ] `ReferenceScreenshots/target_sprint_reference.png` 가 **실제 플레이 캡처**로 갱신되어 있고, 레퍼 2~3장과 **한 장 비교**를 최소 1회 했음.
- [ ] Phase1(조명·안개)·Phase4(HUD 밀도)에서 **이번 주 1카테고리** 루프를 2주 이상 돌려, “싸 보임” 원인이 **에셋 부족**인지 **수치**인지 구분했음.
- [ ] `BATTLE_ACES_QUALITY_WORKFLOW.md` 회귀 10줄·720p 쌍 캡처에 **치명적 깨짐 없음**.

- 레퍼 스샷(`ReferenceScreenshots/`)이 Phase1~4에서 수렴한 **뒤에만** 저폴리 메시·단일 커스텀 셰이더 검토.
- 새 포인트 컬러·발광 소재 추가 없음 — `BATTLE_ACES_ART_DIRECTION.md` 규율 유지.

## Phase9 — 스토어 데모 패키지

- 상세 항목: [`STEAM_DEMO_CHECKLIST.md`](STEAM_DEMO_CHECKLIST.md).
- 스크린샷: F12 HUD 숨김 + 에디터/개발 빌드 **F8 전장 / F9 코어 근접** 카메라 프리셋(`BattleAcesStoreCapturePresets`).
- IL2CPP 스모크 1회, 알려진 이슈 5줄, OSS/폰트 고지.

## Phase10 — 1인 유지보수

- 회귀 10줄: [`BATTLE_ACES_QUALITY_WORKFLOW.md`](BATTLE_ACES_QUALITY_WORKFLOW.md).
- Profiler 기록 템플릿: [`PROFILER_PASS_TEMPLATE.md`](PROFILER_PASS_TEMPLATE.md) — 월 1회 이상이면 충분(측정 없는 최적화 금지).
- 기술 부채 상위 3개만 이슈에 유지; 멀티·DLC 전제 없음은 이슈/스토어에 명시.

## 관련 코드(빠른 점프)

| 영역 | 진입점 |
|------|--------|
| 품질·주간 캡처 | `BattleAcesArtQualityScope`, `ReferenceScreenshots/WEEKLY_CAPTURE.md` |
| 그래픽 프리셋 | `GameUserSettings.ApplyGraphicQualityPreset` |
| UGUI 스케일 | `BattleAcesUguiScaleUtility`, 옵션 UI 크기 슬라이더 |
| 오디오 밸런스 | `ProceduralAudioUtility`, 믹서 Expose 이름은 부트스트랩과 일치 |
