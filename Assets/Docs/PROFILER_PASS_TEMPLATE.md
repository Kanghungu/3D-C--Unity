# Profiler 패스 템플릿 (Battle Aces)

Unity **에디터**에서만 의미 있습니다. 아래는 **복붙 후 메모 채우기**용입니다. 실측은 로컬에서 실행합니다.

## 1. 준비

- 대상 씬: Battle Aces 전투(예: `NewSampleScene`) 한 판 분량
- 창: **Window → Analysis → Profiler**
- 기록: CPU Usage 우선, 필요 시 Memory

## 2. CPU 패스 (약 2분)

1. Profiler **Record** 시작
2. 전투 시작 ~ 교전 ~ 승패 화면까지 **연속 플레이**(약 2분)
3. Record 중지 후 타임라인에서 **스파이크 구간** 3곳만 표시

## 3. 기록할 항목 (5줄)

- 날짜 / Unity 버전 / 빌드 타입(에디터 Play vs Development Standalone)
- 평균 CPU ms(대략) / GC Alloc가 눈에 띄는 프레임 있음 여부
- 스파이크 상위 1~2개 카테고리(예: `OnGUI`, `Physics`, `Rendering`)
- IMGUI(`OnGUI`) 호출이 매 프레임 과도한지 — HUD 열림/닫힘 전후 비교
- 결론: **이번 주 조치 없음** / **다음에 볼 파일·함수 이름**

## 4. GC Alloc · UI Rebuild 의심 경로 (체크리스트)

- [ ] `OnGUI` / IMGUI 레이아웃이 매 프레임 새 `GUIStyle`·문자열 연산을 만들지 않음
- [ ] `Update` 루프에서 LINQ·임시 배열 할당 남발 없음
- [ ] 오디오·파티클 스폰이 매 프레임 반복되지 않음
- [ ] UGUI(`Canvas`·`Graphic`) **Rebuild** 가 결과 카드·상단 바 표시 시에만 튀지 않는지 — Profiler UI 카테고리

## 5. 알려진 한계

- IL2CPP 릴리스 빌드는 에디터 Profiler와 수치가 다를 수 있음 — 런치 전 **스모크 빌드 1회**는 `STEAM_DEMO_CHECKLIST.md` 참고

## 6. 측정 기록 누적

날짜·결론을 Git에 남기려면 [`profiler_pass_log.md`](profiler_pass_log.md) 에 템플릿 블록을 추가합니다(추측 수치는 넣지 않음).
