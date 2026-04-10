# 주간 목표 화면 캡처 (10초 루틴)

스프린트 포커스 화면은 코드 상수 `BattleAcesDemoQualitySprintFocus`와 동일합니다.  
지금 기준: **승패 결과 카드** 한 화면만 매주 같은 조건으로 비교합니다.

## 1. 목표 스크린샷 1장

1. 포커스 화면이 **만족스러울 때** 한 번 찍는다.
2. 이 폴더에 저장한다: **`target_sprint_reference.png`** (이름 고정, PNG 권장).
3. 외부 대작 레퍼 2~3장은 기존 `README.md` 규칙대로 — 목표 캡처와 **나란히** 두고 비교.

### 해상도 쌍 (팀 규칙 고정)

같은 장면을 **1920×1080**과 **1280×720**에서 각각 한 장씩 이 폴더에 보관합니다. Unity **Game** 뷰 해상도를 해당 값으로 맞춘 뒤 캡처합니다.

- **파일명(코드 상수와 동일)**: `BattleAcesArtQualityScope.WeeklyCompareScreenshotFileName1080p` → `weekly_compare_1920x1080.png`
- **파일명**: `BattleAcesArtQualityScope.WeeklyCompareScreenshotFileName720p` → `weekly_compare_1280x720.png`

다른 이름으로 저장하지 않습니다(문서·체크리스트·비교 습관 일치).

### 변경 전후 비교(권장)

동일 파일명으로 덮어쓰기 전에, 지난 주 `weekly_compare_*.png`를 OS에서 한 번 복사해 `weekly_compare_1920x1080_prev.png` 등으로 남겨 두면 diff가 쉽습니다. Git으로 이전 버전을 볼 수 있어도, **한 폴더에서 나란히** 두 장을 두는 습관이 빠릅니다.

## 2. 매주 (약 10초)

1. 전투 한 판을 끝내 **결과 카드**가 뜬 상태에서 녹화하거나 연속 스크린샷.
2. 길이는 **약 10초** — 폰트·구분선·버튼·통계 한 줄이 읽히는지만 본다.
3. `BATTLE_ACES_ART_DIRECTION.md` 팔레트를 깨는지(새 강채도 색 남발)만 체크.
4. 차이가 나면 **이번 주에 고칠 항목 1~2개만** 이슈/메모에 적는다.

## 3. 에디터 바로가기

- 폴더 열기: **Game → Battle Aces → 아트 → 레퍼런스 스크린샷 폴더 열기**
- 검수 체크리스트 복사: **품질 검수 체크리스트(한글) 클립보드 복사** (해상도 쌍 파일명·스프린트 포커스 포함)
- **스프린트 스코프 한 줄(템플릿) 클립보드 복사**: `ExampleScopeLockLineKorean` + 이번 스프린트 화면 포커스 문장 — 이슈/메모 상단에 붙여 무한 폴리싱 방지
- **스프린트 시작 가드레일(고정·튜닝 축) 클립보드 복사**: `BuildSprintStartGuardrailsClipboardKorean` — Phase8 게이트 문구 포함

## 주간 수행 기록 (형식 고정)

동일 형식으로 누적하려면 [`weekly_perform_log.md`](weekly_perform_log.md) 에 한 줄씩 추가합니다.

`YYYY-MM-DD | 이번 주 고친 카테고리: 조명 / UI / 지형 / 카메라 / 그래픽 중 1개`

예: `2026-04-10 | 이번 주 고친 카테고리: UI`
