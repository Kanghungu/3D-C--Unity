# Battle Aces · 아트 방향 (한 장 분량)

코드 상수: `Game.BattleAces.BattleAcesArtDirection`
품질·스코프 워크플로: `Game.BattleAces.BattleAcesArtQualityScope` · 레퍼 폴더 `Assets/Docs/ReferenceScreenshots/`
스프린트 ‘한 화면’ 포커스·UGUI 결정: `Game.BattleAces.BattleAcesDemoQualitySprintFocus` · [`BATTLE_ACES_UGUI_BOUNDARY.md`](BATTLE_ACES_UGUI_BOUNDARY.md)
가독성 수치: `BATTLE_ACES_READABILITY.md`

## 한 줄

**SF 종교 전쟁 — 차가운 금속·잿빛 안개, 포인트 컬러는 의식용 티얼(청록) 하나만.**

영문: *Cold religious-war SF: gunmetal, ash, fog; one ritual teal accent only.*

## 레퍼런스 1~2 (무엇을 가져올지만)

1. **《듄》(2021)** 집전·석조 장면 — 색 수를 줄이고, 차가운 재질과 모래 톤으로 무게만 준다.
2. **《Destiny》 타워/레이드 로비 류** — 금속 베이스는 차갑게, **강한 채도는 한 요소**(UI·신호·광원)에만 모은다.

## 팔레트 규칙

| 역할 | 방향 | 코드 |
|------|------|------|
| 베이스 | 건메탈·잿빛 | `GunmetalDark` ~ `GunmetalLift`, `AshStone` |
| 포인트 | 티얼(청록) 단일 | `PointTeal` — 비콘·선택·의식 연출 |
| 적 보조 | 티얼과 분리된 낮은 앰버 | `EnemyEmber` — 남용 금지 |
| 안개·하늘 | 푸른 잿빛 | `FogHorizon`, `AmbientSky` |

## 미니맵 색약 모드(접근성)

- **목적**: 적록색약 등에서도 아군/적을 **색상**으로 더 쉽게 구분.
- **규율**: 위 표의 티얼·앰버 축을 깨지 않고, 파랑·시안 / 주황·노랑 쪽으로만 명도·채도를 벌림.
- **코드**: `BattleAcesArtDirection.MinimapColorblind*` 상수 → `BattleAcesReadability`가 미니맵 점에 적용.
- **문서**: `BATTLE_ACES_READABILITY.md` §4.

## 라이트·에셋 선택 팁

- **메탈릭/스무스**: 베이스는 낮게, 스펙큘은 키 라이트 한 방향에만 읽히게.
- **새 유닛·건물**: 알베도 채도를 올리기 전에 `PointTeal` / `EnemyEmber`만 쓸 수 있는지 먼저 검토.
- **스크린샷**: 안개 + 단일 티얼 포인트가 있으면 “한 판”이 무대처럼 읽힌다.

## 기대치 고정 · 집 테스트 루프

1. **레퍼런스 2~3장 + 우리 목표 캡처 1~2장**을 `Assets/Docs/ReferenceScreenshots/`에 둔다(자세한 규칙은 해당 폴더 `README.md`).
2. 플레이 **약 10초** 캡처로, 조명·UI·지형·카메라 중 **싸 보이는 곳 1~2곳만** 표시해 고친다.
3. **새 포인트 컬러·새 발광 소재**는 넣지 않는다 — 티얼 단일 포인트 규율이 깨지면 전체가 싸 보인다.
4. Unity 메뉴 **Game → Battle Aces → 아트**에서 폴더 열기·체크리스트·**스프린트 포커스** 클립보드 복사 가능.
5. 스프린트 목표 화면 주간 비교: `ReferenceScreenshots/WEEKLY_CAPTURE.md` · 목표 파일명 `target_sprint_reference.png`.
6. 품질 로드맵 실행 요약(스코프 문장·720p 쌍·회귀 10줄·캡처 단축키): [`BATTLE_ACES_QUALITY_WORKFLOW.md`](BATTLE_ACES_QUALITY_WORKFLOW.md).
7. **고정 축 vs 튜닝 축**(종류별 변동·진행 상태 표): 동일 [`BATTLE_ACES_QUALITY_WORKFLOW.md`](BATTLE_ACES_QUALITY_WORKFLOW.md) — 「품질 축 분류」「전방위 퀄 로드맵 진행 상태」.
