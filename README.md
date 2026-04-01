# 3D-Unity

Unity 기반 3D SF 싱글플레이 RTS 전투 프로토타입 프로젝트입니다.

## 현재 목표
- 메인 목표는 `지상전 RTS 전투 프로토타입`입니다.
- 카메라, 선택, 이동, 공격, 생산, 승패가 한 흐름으로 이어져야 합니다.
- 함대전은 `별도 실험`입니다.
- 1인 개발 기준으로 작고 확인 가능한 단위로 진행합니다.

## 최근 방향 점검 결론
- 현재 소스는 이미 `플레이 가능한 전투 프로토타입` 기준은 충족하는 쪽으로 가고 있습니다.
- 다만 최근 작업은 전투 감각 검증보다 `전장 연출 확장`, `환경 레이어 추가`, `시스템 수 증가` 쪽으로 무게가 커졌습니다.
- 다음 작업은 새 연출을 더 늘리기보다 `전장 규모`, `거점 수`, `병종 비중`, `HUD 가독성`, `직접 플레이 검증`을 우선합니다.
- 특히 `BattlefieldSkyAtmosphere` 같은 대형 연출 코드는 당분간 더 키우기보다 멈추고, 플레이에 직접 닿는 개선을 먼저 진행합니다.

## 지금 운영 기준
- 가장 중요한 기준은 `짧게라도 직접 조작해서 끝까지 진행되는 전투`입니다.
- 메인 씬은 `SampleScene`이며, 여기서 지상전 루프를 계속 다듬습니다.
- 함대전은 `FleetExperimentScene`에만 남겨 두고 메인 개발 라인으로 확장하지 않습니다.
- 새 기능을 넣기 전에는 `지금 전투 프로토타입을 더 잘 플레이하게 만드는가`를 먼저 판단합니다.
- 작은 작업 1개를 잡더라도 가능하면 `구현 -> 간단 검증 -> 문서 반영`까지 한 흐름으로 마무리합니다.

## 먼저 읽을 문서
1. `AGENTS.md`
2. `START_HERE.md`
3. `SESSION_CONTEXT.md`
4. `TODO.md`
5. `DEVLOG.md`

## 루트 핵심 문서
- `AGENTS.md`: 작업 원칙
- `README.md`: 프로젝트 입구
- `START_HERE.md`: 집에서 AI에게 바로 맡길 때 보는 문서
- `SESSION_CONTEXT.md`: 현재 상태와 방향 요약
- `TODO.md`: 지금 할 일
- `DEVLOG.md`: 작업 기록
- `HOME_AI_PROMPTS.md`: 집에서 AI에게 바로 복붙하는 프롬프트 모음
- `HOME_AI_HARNESS.md`: 집에서 작업 시작 흐름을 고정하는 가벼운 하네스 안내
- `task_brief.json`: 현재 목표와 제약을 구조화한 요약 파일

## 하네스 파일
- `scripts/Start-HomeWork.ps1`: Git 상태와 핵심 문서를 먼저 보여주는 시작 스크립트
- `scripts/Show-CurrentContext.ps1`: 현재 방향과 `task_brief.json`을 다시 보여주는 스크립트

## 참고 문서
아래 문서는 지우지 않고 `Docs/Reference`로 정리했습니다.

- `Docs/Reference/AI_NOTES.md`
- `Docs/Reference/ANIMATION_STATE_DIAGRAM.md`
- `Docs/Reference/ART_DIRECTION.md`
- `Docs/Reference/BATTLEFIELD_DIAGRAM.md`
- `Docs/Reference/BRAINSTORMING.md`
- `Docs/Reference/COMBAT_DESIGN.md`
- `Docs/Reference/GDD.md`
- `Docs/Reference/HISTORY.md`
- `Docs/Reference/MESHY_MODELING_GUIDE.md`
- `Docs/Reference/MINDMAP.md`
- `Docs/Reference/ROADMAP.md`
- `Docs/Reference/SYSTEM_FLOW_DIAGRAM.md`

## 집에서 AI에게 맡길 때 기억할 점
- 문서부터 읽고 시작하게 하기
- 미커밋 변경을 덮어쓰지 않게 하기
- 한 단계만 하고 멈추지 말고 가능한 범위를 끝까지 진행하게 하기
- 메인 목표는 계속 지상전 프로토타입이라는 점을 유지하게 하기
- 새 하늘 연출, 새 확장 병종, 새 대형 시스템보다 먼저 `플레이 감각`을 확인하게 하기
