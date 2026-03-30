# 3D-Unity

Unity 기반 3D SF 싱글플레이 RTS 전투 프로토타입 프로젝트입니다.

## 현재 목표
- 메인 목표는 `지상전 RTS 전투 프로토타입`입니다.
- 카메라, 선택, 이동, 공격, 생산, 승패가 한 흐름으로 이어져야 합니다.
- 함대전은 `별도 실험`입니다.
- 1인 개발 기준으로 작고 확인 가능한 단위로 진행합니다.

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
- `Docs/Reference/MINDMAP.md`
- `Docs/Reference/ROADMAP.md`
- `Docs/Reference/SYSTEM_FLOW_DIAGRAM.md`

## 집에서 AI에게 맡길 때 기억할 점
- 문서부터 읽고 시작하게 하기
- 미커밋 변경을 덮어쓰지 않게 하기
- 한 단계만 하고 멈추지 말고 가능한 범위를 끝까지 진행하게 하기
- 메인 목표는 계속 지상전 프로토타입이라는 점을 유지하게 하기
