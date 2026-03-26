# TODO.md

## 기준
- 지금 목표는 지상 전투 프로토타입 완성이다.
- 작업은 Unity에서 바로 구현 가능한 단위로 쪼갠다.
- 지상전이 먼저고, 함대전은 별도 실험 항목으로 둔다.

## 1. 프로젝트 준비
- Unity 프로젝트 생성
- 사용할 Unity 버전 확정
- 기본 씬 이름 규칙 정하기
- `Assets/Scenes`, `Assets/Scripts`, `Assets/Prefabs`, `Assets/Materials` 폴더 만들기
- 메인 전투 테스트 씬 1개 만들기
- Git에 올리지 않을 Unity 생성 파일 범위 확인

## 2. 카메라
- 카메라 이동 입력 정하기
- 카메라 전후좌우 이동 구현
- 카메라 회전 구현
- 카메라 줌 구현
- 맵 바깥으로 나가지 않도록 이동 범위 제한
- 낮은 각도의 3D 전략 시점으로 시야값 조정

## 3. 기본 맵
- 사막 성지 테마의 테스트 지형 만들기
- 이동 가능한 바닥과 이동 불가능한 장애물 구분
- 시작 위치 2개 배치
- 포위와 측면이 가능한 넓은 전장 구조 만들기
- 엄폐처럼 보이는 구조물은 두되, 실제 엄폐 시스템은 나중으로 미룸
- 성지 폐허나 제단형 구조물 배치

## 4. 유닛 데이터 구조
- 유닛 공통 스크립트 초안 만들기
- 체력, 이동 속도, 공격력, 공격 속도, 사거리 변수 정의
- 아군/적군 식별 방식 정하기
- 선택 가능 여부와 현재 상태를 저장할 구조 만들기
- 사망 시 제거 방식 정하기

## 5. 플레이어 조작
- 마우스 클릭으로 단일 유닛 선택
- 드래그 박스 선택 여부 결정
- 선택된 유닛 표시 방식 만들기
- 우클릭 이동 명령 구현
- 적 클릭 시 공격 명령으로 분기
- 아무것도 없는 바닥 클릭 시 이동만 수행

## 6. 이동
- 유닛 이동 방식 결정
- Unity NavMesh를 쓸지 간단 이동 스크립트를 쓸지 결정
- 선택된 유닛이 목표 지점까지 이동
- 여러 유닛이 겹쳐도 완전히 멈추지 않게 조정
- 목표 지점 도착 판정 만들기

## 7. 전투 기본
- 적 탐지 방식 만들기
- 사거리 안 적을 공격 대상으로 지정
- 공격 쿨다운 구현
- 피해 적용 구현
- 체력 0 이하일 때 사망 처리
- 죽은 유닛 선택 해제 처리
- 자동 공격 여부 결정

## 8. 유닛 프리팹
- 플레이어 진영 임시 유닛 1종 만들기
- 적 진영 임시 유닛 1종 만들기
- 가능하면 지원형 또는 원거리형 1종 추가
- 유닛별 색 또는 실루엣 차이 주기
- 체력바 또는 간단한 상태 표시 붙이기
- 플레이어 유닛은 중장갑 성전 제국 느낌으로 맞추기
- 적 유닛은 정면 화력형 실루엣으로 차이 주기

## 9. 생산 또는 증원
- 프로토타입 생산 방식 확정
- 자동 증원인지 거점 생산인지 먼저 하나 선택
- 생산 UI 최소 형태 만들기
- 자원 없이 쿨다운만으로 생산할지 결정
- 생성 위치와 생성 제한 규칙 만들기
- 생산된 유닛이 정상적으로 선택/이동/전투 되는지 확인

## 10. 승패 규칙
- 승리 조건 1개 정하기
- 패배 조건 1개 정하기
- 모든 적 제거 시 승리 처리
- 아군 전멸 시 패배 처리
- 결과 UI 또는 텍스트 표시
- 재시작 방식 정하기

## 11. 전투 밸런스 1차
- 유닛 체력 수치 조정
- 공격력과 공격 속도 조정
- 사거리 차이 조정
- 전투 시간이 너무 짧지 않은지 확인
- 중간 규모 전투에서 프레임과 조작감 확인

## 12. 첫 플레이 테스트
- 아군과 적군을 배치한 테스트 씬 실행
- 선택, 이동, 공격, 사망이 모두 이어지는지 확인
- 생산 또는 증원이 실제 전투 흐름에 들어가는지 확인
- 승패가 끝까지 정상 동작하는지 확인
- 10분 이상 플레이해도 흐름이 유지되는지 확인
- 문제를 `DEVLOG.md`에 기록

## 13. 함대전 실험
- 함대전을 별도 씬으로 분리할지 결정
- 지상전 완성 전에는 본 개발 라인에 넣지 않기
- 아주 작은 규모의 함선 이동/사격 실험만 허용

## 이번 주 우선순위
- Unity 프로젝트 생성
- 기본 폴더 구조 만들기
- 사막 성지 전투 테스트 씬 만들기
- RTS 카메라 구현
- 유닛 1기 선택과 이동 구현
- 적/아군 구분 방식 정하기

## 전투 프로토타입 완료 기준
- 카메라 조작 가능
- 유닛 선택 가능
- 이동 명령 가능
- 공격 가능
- 사망 처리 가능
- 간단한 생산 또는 증원 가능
- 승리/패배 가능
- 1판의 전투가 처음부터 끝까지 끊기지 않음

## 바로 하지 않을 일
- 멀티플레이
- Steam SDK 연동
- 복잡한 AI
- 화려한 연출 제작
- 혼자 오래 붙잡아야 하는 대형 리팩터링
- 행성 전체 전략 시뮬레이션 완성
- 4개 이상 문명 전부 동시 구현

## 2026-03-26 Progress Update
- Unity project bootstrapped and repository structure prepared.
- RTS camera, drag selection, movement orders, and attack orders are implemented.
- Friendly and enemy armies spawn with two roles: Vanguard and Skirmisher.
- Bases exist for both sides, enemy auto-reinforces, and the player can queue production with `1` and `2`.
- Health bars, victory/defeat flow, restart flow, and a lightweight HUD are in place.
- Simple terrain features were added while keeping the main battlefield readable.

## Next Focus
- Tune combat pacing and unit role balance.
- Improve battlefield readability and production feedback.
- Add a clearer distinction between front-line and flank play.
- Record playtest findings in `DEVLOG.md` after longer sessions.
