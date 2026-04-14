# Battle Aces — 유닛 시각 확장 규격 (P3: 메시·LOD·팀 스킨)

**목적**: 데모 이후 덱 전 아키타입을 **메시 우선**으로 올리되, 에셋 누락·저사양에서도 **프리미티브 폴백**으로 한 판이 깨지지 않게 한다.  
**팔레트**: `BATTLE_ACES_ART_DIRECTION.md` · 티얼/앰버 축 유지. 새 강채도 남발 금지.

## 1. 에셋 경로·네이밍 (구현과 동기)

| 항목 | 규칙 |
|------|------|
| 보병 3종 기본 Resources 경로(확장자 없음) | 창병: `Units/Spearman` → 없으면 `PrototypeUnits/test` / 방패: `Units/ShieldInfantry` / 소총: `Units/Rifleman` — 코드: `PrototypeUnitVisualResourceCatalog` |
| 정의별 덮어쓰기 | `UnitDefinition.OptionalUnitVisualResourcePath` 가 비어 있지 않으면 **해당 경로만** 먼저 로드 |
| 팀 변형 | `PrototypeUnitVisualTintUtility` — `MaterialPropertyBlock` 으로 `_BaseColor` / `_Color` 틴트 |
| LOD | 창병 메시에 프리팹 `LODGroup` 없으면 런타임 2단(원거리 컬) 시범 추가 — `PrototypeEntityVisualFactory.TryAddSpearmanRuntimeLodCull` |

## 2. 코드 연동 순서 (현재)

1. `UnitDefinition.OptionalUnitVisualResourcePath` (선택) + `PrototypeUnitVisualResourceCatalog.TryLoadUnitVisualPrefab`.
2. `PrototypeEntityFactory` → `PrototypeEntityVisualFactory.BuildUnitSilhouette` 에서 **메시 성공 시 분기 종료**, 실패 시 프리미티브 실루엣.
3. `BA_AnimScaleRoot` 부모 규칙 유지 — 공격 스케일 클립이 루트 `UnitDefinition.scale` 을 덮어쓰지 않게 한다.

## 3. LOD

- RTS 줌: **전장 카메라 거리** 기준 2~3단. 미니맵용 초저폴리는 별도 규칙으로 후순위.
- 첫 스프린트: **LOD 없음 + 단일 메시** 허용, 두 번째 스프린트에서 LOD 추가.

## 4. 팀 스킨

- 아군: `PointTeal` 계열 포인트만 강조, 베이스는 건메탈 톤.
- 적: `EnemyEmber` 보조, 베이스는 따뜻한 어두운 톤.
- 구현: `PrototypeUnitVisualTintUtility` (`MaterialPropertyBlock`).

## 5. 스프린트 쪼개기 (1인)

권장 순서: **보병 3종(창·방패·소총) → 기동(선봉 등) → 포·대형**. 매 스프린트마다 `NewSampleScene` 한 판 + 720p 캡처 회귀.

## 6. 완료 정의 (P3)

- 덱의 전투 아키타입마다 메시 또는 합의된 대체 비주얼이 로드된다.
- 최소 한 줌 구간에서 LOD 전환이 재현·검증된다(또는 “단일 메시 예외”를 문서에 명시).
- 동일 메시로 플레이어/적 팀이 팔레트 규율 안에서 구분된다.
