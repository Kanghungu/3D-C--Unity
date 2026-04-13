# Battle Aces — Animator 규약 (RTS 무게감)

`UnitDefinition`의 **Anim Combat Profile** 필드 문자열과 Animator 컨트롤러 파라미터 이름을 맞춘다.

## 필수 파라미터 타입

| 논리 이름 | 기본 파라미터명 | 타입 | 설명 |
|-----------|-----------------|------|------|
| 이동 속도 | `Speed` | Float | 0~1 근사. `UnitAnimationDriver`가 Nav 속도/비행 이동량으로 갱신 |
| 전투 중 | `InCombat` | Bool | 현재 `UnitCombat.CurrentTarget` 유무 |
| 공격 스윙 | `Attack` | Trigger | 애니 기반 타격 사용 시 공격 시작마다 1회 |
| 사망 | `Die` | Trigger | `UnitHealth.Die` 직전에 1회 |

이름은 인스펙터에서 변경 가능하다.

## 애니메이션 이벤트 (타격 동기)

`AnimCombatProfile.useAnimDrivenStrike`가 **켜져 있으면** 유닛에 `UnitCombatStrikeBridge`가 붙고, 실제 피해/발사체는 **클립 이벤트**까지 지연된다.

1. 공격 클립에서 타격 프레임에 이벤트 추가
2. 함수: `AnimStrike` (문자열 정확히 일치)
3. 수신 오브젝트: 유닛 루트(브릿지가 붙은 GameObject)

이벤트가 오지 않으면 `strikeFallbackTimeoutUnscaled` 초 후 강제 타격(버그 완화).

## 사망

`UnitHealth`가 파괴 직전 `UnitAnimationDriver.NotifyDeath()`를 호출한다. `Die` 트리거를 받는 상태로 전환하려면 컨트롤러에 해당 트랜지션을 구성한다.

## 참고 코드

- `UnitAnimationDriver` — 파라미터 동기·트리거
- `UnitCombatStrikeBridge` — `AnimStrike()` 진입점
- `UnitCombat` (partial `UnitCombat.AnimStrike`) — 대기·타임아웃·실제 `ApplyDamage`
