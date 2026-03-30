# HOME_AI_HARNESS.md

## 역할
- 집에서 AI와 작업할 때 쓰는 가벼운 하네스 안내 문서다.
- 문서만 읽히는 방식에서 한 단계 더 나아가, `문서 + 스크립트 + task_brief.json` 조합으로 작업 시작 흐름을 고정한다.
- 지금 단계에서는 무거운 API 자동화보다, 반복되는 시작 절차를 줄이고 방향 이탈을 막는 것이 목적이다.

## 구성 요소
- `HOME_AI_HARNESS.md`
  - 하네스의 목적과 사용 순서를 설명한다.
- `scripts/Start-HomeWork.ps1`
  - 집에서 작업 시작 전에 현재 Git 상태와 핵심 문서 목록을 빠르게 보여준다.
- `scripts/Show-CurrentContext.ps1`
  - 핵심 문서의 앞부분과 `task_brief.json`을 같이 보여줘서 현재 방향을 다시 잡게 한다.
- `task_brief.json`
  - 현재 목표, 금지사항, 우선순위, 세계관 방향을 구조화해서 저장한다.

## 왜 이 하네스를 쓰는가
- AI가 자꾸 중간에 멈추는 문제를 줄이기 위해
- 매번 같은 설명을 반복하지 않기 위해
- 집에서 미커밋 변경이 있어도 덮어쓰기 전에 먼저 상황을 보게 하기 위해
- 문서와 실제 작업 목표를 더 일관되게 맞추기 위해

## 권장 사용 순서
1. PowerShell에서 `./scripts/Start-HomeWork.ps1` 실행
2. 이어서 `./scripts/Show-CurrentContext.ps1` 실행
3. `HOME_AI_PROMPTS.md`에서 상황에 맞는 프롬프트 복붙
4. AI가 문서 요약 후 가장 안전한 작업 1개를 고르도록 유도
5. 시작한 작업은 가능한 범위에서 `구현 -> 간단 검증 -> 문서 반영`까지 이어서 진행

## AI에게 같이 보여주면 좋은 파일
1. `AGENTS.md`
2. `START_HERE.md`
3. `SESSION_CONTEXT.md`
4. `TODO.md`
5. `DEVLOG.md`
6. `HOME_AI_PROMPTS.md`
7. `HOME_AI_HARNESS.md`
8. 필요 시 `Docs/Reference/BATTLEFIELD_DIAGRAM.md`
9. 필요 시 `Docs/Reference/SYSTEM_FLOW_DIAGRAM.md`
10. 필요 시 `Docs/Reference/ANIMATION_STATE_DIAGRAM.md`
11. 필요 시 `Docs/Reference/ART_DIRECTION.md`

## 집에서 바로 쓰는 기본 흐름
```text
1. Start-HomeWork.ps1로 현재 상태 확인
2. Show-CurrentContext.ps1로 핵심 방향 재확인
3. HOME_AI_PROMPTS.md의 기본 시작 프롬프트 사용
4. 작업이 느리면 30~60분 작업 묶음 프롬프트를 추가
5. 작업 후 DEVLOG와 TODO가 같이 갱신됐는지 확인
```

## 이 하네스가 하지 않는 것
- Unity를 자동 실행하지 않는다.
- 코드를 자동 수정하지 않는다.
- 별도 API나 에이전트 서버를 붙이지 않는다.
- 복잡한 멀티 에이전트 오케스트레이션을 만들지 않는다.

## 다음 단계로 붙일 수 있는 확장
- 플레이 테스트 체크리스트 스크립트
- 작업 종료 보고서 스크립트
- 자주 쓰는 작업별 `task_brief` 템플릿
- 나중에 정말 필요할 때만 API 기반 자동화
