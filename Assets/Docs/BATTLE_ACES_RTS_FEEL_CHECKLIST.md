# Battle Aces — RTS 무게감 회귀 체크리스트

한 판 데모(`NewSampleScene`) 기준 수동 검증.

## 레거시 (애니 타격 끔)

- [ ] 유닛 생산·이동·우클릭 명령 정상
- [ ] 근접/원거리 피해·사망·승패 루프 정상
- [ ] NavMesh 경로 이상 없음 (능선·벽 포함 맵)

## 이동 프로필

- [ ] 지상: 가속/각속도 변경 시 급회전·출발이 과하지 않은지
- [ ] 비행: 가속 필드 변경 시 출발이 부드러운지

## 애니 타격 (useAnimDrivenStrike 켬 + Animator + AnimStrike 이벤트)

- **전제(코드)**: `UnitDefinition.OptionalAnimatorController`가 있을 때만 `UnitCombatStrikeBridge`가 붙고, 런타임에 Animator에 컨트롤러가 있어야 애니 타격 경로가 켜짐. 컨트롤러 없이 플래그만 켜면 레거시 즉시 타격으로 동작.
- [ ] 타격 프레임과 데미지 숫자/피격음이 맞는지
- [ ] 이벤트 누락 시 `strikeFallbackTimeoutUnscaled` 후에도 데미지가 한 번 들어가는지
- [ ] 선딜 중 `windupMoveSpeedMultiplier` 체감

## 군중 분리 (enableCrowdSeparation)

- **참고**: 이웃 기여 상한(8) + 기본 `separationPushPerSecond`·반경은 밀집 떨림 완화 쪽으로 조정됨.
- [ ] 아군 밀집 시 끼임이 약간 완화되는지
- [ ] 병목/문에서 Nav가 떨리지 않는지

## 가독성

- [ ] 선택 링이 공격 순간 살짝 커지는지 (`RecentAttackPulse`)

## 상성(삼각) HUD

- [ ] 유닛 1기 선택 + 표적이 있을 때 하단 패널에 `상성 유리` / `상성 불리`(또는 영문 favor/weak)가 표적 조합에 맞게 뜨는지

## 사망 애니

- [ ] `Die` 트리거가 있는 컨트롤러에서 사망 연출이 재생되는지 (없으면 무시됨)
- [ ] `AnimCombatProfile.deathDestroyDelayUnscaled` > 0 이고 Animator+Die가 있으면, 파괴가 약간 늦춰져 잔해가 타격 직후와 겹치지 않는지 (0이면 기존처럼 즉시 파괴)
