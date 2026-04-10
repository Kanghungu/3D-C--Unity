# Battle Aces · 레퍼런스 스크린샷 폴더

**목적**: “무한 폴리싱”을 막기 위해 **레퍼런스 2~3장**과 **우리 톤의 목표 스크린샷**만 이 폴더에 둡니다.

## 넣을 것

> **플레이스홀더 PNG**가 있으면 절차적 그라데이션으로 만든 **임시**입니다. 실제 레퍼 스틸·플레이 캡처로 같은 파일명을 덮어쓰세요. 재생성: `_gen_placeholders.ps1`

1. **외부 레퍼런스** 2~3장(필름·게임 스틸 등) — 저작권은 본인이 사용 가능한 범위로만. 복제가 아니라 **색 수·안개·금속 질감**만 가져온다는 메모용. 권장 파일명은 코드 `BattleAcesArtQualityScope.SuggestedExternalReferenceFileNames`(`reference_external_mood_01.png` …) — 실제로는 2~3장만 써도 됨.
2. **우리 빌드 캡처** 1~2장 — “이 정도면 데모 1차 목표”라고 박아둘 **고정 목표 화면**.
3. **스프린트 목표 1장** — 파일명 고정: **`target_sprint_reference.png`** (`BattleAcesDemoQualitySprintFocus.TargetSprintReferenceScreenshotFileName`, 절차는 [`WEEKLY_CAPTURE.md`](WEEKLY_CAPTURE.md)).
4. **주간 해상도 쌍** — 동일 장면을 1080p·720p로 각각: `weekly_compare_1920x1080.png`, `weekly_compare_1280x720.png` (`BattleAcesArtQualityScope` 상수와 동일).
5. **주간 절차 로그(텍스트)** — [`weekly_perform_log.md`](weekly_perform_log.md) (한 줄씩만 추가).

## 작업 루프 (집 테스트 기준)

1. 플레이 모드에서 **약 10초** 분량 화면을 녹화하거나 스크린샷을 찍는다.
2. 아래 네 가지 중 **어디가 싸 보이는지**만 표시한다. (전부 고치려 하지 않기)
   - 조명 (방향광·그림자·색온도)
   - UI (가독성·밀도·색 일관)
   - 지형·머티리얼
   - 카메라 (거리·FOV·클리어 컬러)
3. **새 포인트 컬러·새 발광 소재**를 추가하지 않는다. 티얼·앰버·잿빛 축은 `BATTLE_ACES_ART_DIRECTION.md` / `BattleAcesArtDirection` 고정.

## 에디터에서 폴더 열기

메뉴: **Game → Battle Aces → 아트 → 레퍼런스 스크린샷 폴더 열기**

## 코드 상수

- 폴더 경로 문자열: `BattleAcesArtQualityScope.ReferenceScreenshotsAssetFolder`
- 검수용 체크리스트 복사: **Game → Battle Aces → 아트 → 품질 검수 체크리스트(한글) 클립보드 복사**

## Git LFS·용량

레퍼 PNG/JPG는 **가급적 장당 수 MB 이하**를 권장합니다. 대용량 스틸을 넣을 때는 저장소 **LFS 정책**에 맞게 관리하세요.

## UI 레이어 표

IMGUI vs UGUI `sortingOrder` 정리: [`UI_LAYER_ORDER.md`](UI_LAYER_ORDER.md)

## 실행 워크플로 요약

스코프 한 줄·720p 쌍 캡처·회귀 10줄·캡처 단축키: [`../BATTLE_ACES_QUALITY_WORKFLOW.md`](../BATTLE_ACES_QUALITY_WORKFLOW.md)
