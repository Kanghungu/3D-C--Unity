# Battle Aces — 즉시 피드백(B축) 정리

플레이어가 **당장** 알아차릴 수 있는 소리·HUD·짧은 이펙트. 상수는 `Assets/Scripts/BattleAces/BattleAcesFeedbackTiming.cs`.

## unscaled 타이밍(승패·전역)

| 항목 | 초 | 비고 |
|------|-----|------|
| 스팅 → 결과 카드 | `0.14` | `BattleMissionFlow` + `BattleAcesVictoryPresentation` 동일 |
| 유닛 히트음 쿨다운 | `0.1` | `UnitHealth` |
| 지휘 코어 피격 묶음 | `0.4` | 소리+전역 플래시+HUD 한 줄 — `CoreStructureHitSound` |
| 미니맵 링·톤 | `0.12` | 시야 이동은 매번 — `BattleAcesMinimap` |
| 생산·강화 거절 힌트 | `0.38` | `BattleAcesCore` `PlayerEconomyRejectCooldownSeconds` |

## 소리 (`ProceduralAudioUtility`)

| 상황 | 메서드 |
|------|--------|
| 덱 주문 큐 진입 성공 | `PlayDeckOrderQueued` |
| 생산 완료 스폰 | `PlayProductionComplete` / 적 `PlayEnemyProductionComplete` |
| T/Y/U 강화 성공 | `PlayCoreUpgradeApplied` |
| 거절·명령 불가 | `PlayUiCommandRejected` |
| 지휘 코어 피격 | `PlayCoreHit` |
| 적 코어 피격 | `PlayStructureHit` |
| 유닛 피격 | `PlayUnitHitLight` |
| 미니맵 클릭 | `PlayUiMinimapPing` |
| 승패 | `PlayResultSting` |

## HUD 막대 (`BattleAcesHudOverlay`)

| 위치(대략) | 용도 |
|-------------|------|
| 하단 ~48px | 지휘 코어 피격(주황) |
| ~58px | 명령 거절 |
| ~92px | 집결 설정 |
| ~126px | 생산/강화 거절(금색) |
| ~158px | 덱 주문 성공(청록) |

## 월드 이펙트

- 아군 유닛 생산 완료: `BattleAcesCore.SpawnPlayerProductionReadyRing` (짧은 실린더 링).
- 유닛 피격: `UnitHealth` 메시 밝기 펄스(코어 `BattleAcesCore` 제외).

## 마무리 체크(회귀)

1. 영어 설정: 1~8 실패·T/Y/U 실패 막대가 **영문**인지.
2. 승·패 직후 **스팅 다음**에 카드가 뜨는지(빈 화면 1프레임 수준).
3. 지휘 코어 연타 피격 시 HUD·소리가 **0.4s** 묶음인지.
4. 미니맵 연타 시 **톤/링만** 촘촘해지지 않는지.
