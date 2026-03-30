# SYSTEM_FLOW_DIAGRAM.md

## 목적
- 메인 지상전 프로토타입의 핵심 시스템이 어떻게 연결되는지 빠르게 이해시키는 문서
- AI가 소스를 대충 보고 없는 시스템을 상상하지 않게 만드는 기준

## 큰 흐름
```text
RTS Camera
   ↓
Selection Controller
   ↓
Selectable Unit
   ├─ SimpleUnitMover
   ├─ UnitCombat
   ├─ UnitHealth
   └─ UnitAbilityState

ProductionStructure
   ↓
PrototypeEntityFactory
   ↓
New Unit Spawn

BaseStructure
   ↓
Enemy Auto Reinforcement

PrototypeMatchController
   ↓
Victory / Defeat
   ↓
HUD
```

## 현재 시스템 해석
- 카메라는 `RTSCameraController`
- 선택은 `PrototypeSelectionController`
- 유닛은 `SelectableUnit` 중심으로 이동, 전투, 체력, 능력이 연결된다.
- 플레이어 생산은 `ProductionStructure`
- 적 증원은 `BaseStructure`
- 승패는 `PrototypeMatchController`
- 표시와 상태 전달은 `PrototypeHUD`

## AI가 시스템을 다룰 때 주의할 점
- 메인 목표는 지상전 프로토타입 루프 유지다.
- 카메라, 선택, 이동, 공격, 생산, 승패가 한 흐름으로 이어지는지 항상 본다.
- 함대전 시스템은 별도 실험이다.
- 없는 상위 시스템을 새로 설계하기보다 현재 연결 구조를 먼저 이해한다.
- 작은 수정이 메인 루프를 깨지 않는지 확인한다.

## 체크 포인트
- 선택 후 이동과 공격이 자연스럽게 이어지는가
- 생산된 유닛이 기존 선택/이동/전투 흐름에 자연스럽게 들어가는가
- 적 증원이 기존 전투 루프를 깨지 않는가
- 승패 판정이 HUD와 함께 직관적으로 보이는가

## AI에게 바로 보낼 수 있는 문장
```text
`Docs/Reference/SYSTEM_FLOW_DIAGRAM.md`를 기준으로 현재 시스템 연결을 먼저 이해해줘.
없는 상위 구조를 새로 만들지 말고,
카메라 -> 선택 -> 유닛 이동/전투 -> 생산/증원 -> 승패/HUD 흐름이 자연스럽게 이어지는지 기준으로 작업해줘.
중간에 분석만 하지 말고 구현과 검증까지 이어서 진행해줘.
```
