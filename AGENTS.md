# AGENTS.md

## 1. 프로젝트 목표
- 이 프로젝트는 Unity 기반의 3D SF 실시간 전략 게임을 만든다.
- 플랫폼은 Windows PC를 기준으로 개발한다.
- 장기 목표는 Steam 출시지만, 현재는 출시 준비보다 전투 프로토타입 완성이 우선이다.
- 게임 방향은 우주 문명 간 종교 전쟁 분위기의 싱글플레이 RTS다.
- 이 프로젝트는 1인 개발 프로젝트라는 전제를 유지한다.

## 2. 현재 단계
- 현재 단계는 완성작이 아니라 MVP/전투 프로토타입이다.
- 목표는 "짧게라도 직접 조작하고 전투가 성립하는 빌드"를 만드는 것이다.
- **진행 방향(우선): 스토리 미션** — 씬·`MissionDefinition`·대사·승패 목표로 짧은 캠페인 흐름을 쌓는다. (`CampaignMenu` → `NewSampleScene` 등)
- 구형 샘플 전장(`SampleScene` + `PrototypeBootstrapper`)은 참고·실험용으로 두되, 새 기능은 **Battle Aces + Campaign** 쪽에 붙이는 것을 기본으로 한다.
- 멀티플레이, 대규모 콘텐츠, 장기 운영 구조는 지금 범위에 포함하지 않는다.

## 3. 우선순위 기능
- 기본 카메라 이동, 회전, 줌 V
- 유닛 선택과 이동 명령 V
- 적/아군 구분 V
- 자동 공격 또는 단순 공격 명령 V
- 체력, 피해, 사망 처리 V
- 가장 작은 규모의 전투 맵 1개 V
- 최소 2종 이상의 전투 유닛 V
- 승패 판단이 가능한 간단한 게임 루프 V
- 프로토타입 확인용 UI V

## 4. 당장 하지 말아야 할 것
- 멀티플레이
- Steam 연동
- 절차가 큰 서버/백엔드 설계
- 오픈월드, 캠페인 대서사, 복잡한 외교/경제 시스템
- 과도한 최적화
- 고급 셰이더/연출 집착
- 대량의 에셋 제작
- 확장성을 이유로 한 과한 추상화

## 5. 작업 방식
- 항상 프로토타입 우선으로 판단한다.
- 새 기능은 "플레이 가능성"에 직접 기여할 때만 추가한다.
- 구현은 작은 단위로 나누고, 매 단계마다 Unity 에디터에서 바로 확인 가능한 상태를 만든다.
- 복잡한 구조보다 이해하기 쉬운 구조를 우선한다.
- 나는 Unity 초보라는 전제를 유지하고, 낯선 패턴보다 읽기 쉬운 C# 코드를 선택한다.
- Codex는 큰 리팩터링보다 현재 목표를 빠르게 검증하는 변경을 우선한다.
- 설명이 필요하면 Unity 초보도 따라갈 수 있게 짧고 직접적으로 적는다.
- 협업용 복잡한 프로세스보다 1인 개발에 맞는 단순한 작업 흐름을 유지한다.
- 저장소 루트 문서는 **`AGENTS.md` · `README.md` · `DEVLOG.md`** 를 우선한다. 세부 작업 로그는 필요 시 Git 히스토리로 추적한다.

## 6. 폴더/코드 규칙
- Unity 기본 구조를 존중하고, 주요 게임 코드는 `Assets/Scripts` 아래에 둔다.
- 폴더는 역할 기준으로 나눈다.
- 예시: `Assets/Scripts/Core`, `Assets/Scripts/Units`, `Assets/Scripts/Combat`, `Assets/Scripts/UI`
- C# 클래스는 한 파일에 하나씩 둔다.

### 6.1 `Assets/Scripts` 하위 폴더 역할 (한 줄)
- **`Audio/`** — 외부 WAV 없이 쓰는 짧은 프로시저럴 효과음 등.
- **`BattleAces/`** — `NewSampleScene`용 단일 코어 RTS(경제·매치·IMGUI HUD·**UGUI 상단 목표** `BattleAcesObjectiveUgui`·미니맵·미션 런타임 목표).
- **`Camera/`** — RTS 카메라 이동·회전·줌(`RTSCameraController`).
- **`UI/`** — IMGUI 공용 팔레트·패널·버튼(`ImGuiGameUi`) — Battle Aces·캠페인 메뉴/브리핑/결과 톤 통일(UGUI 전까지).
- **`Campaign/`** — 스토리 미션 데이터(ScriptableObject)·메뉴·진행 저장·`CampaignSceneLoadUtility`(빌드 씬 검증 후 로드)·씬 마커(유물·점령 등).
- **`Editor/`** — 캠페인 자산 생성·빌드 씬 등록 등 에디터 전용 메뉴.
- **`Prototype/`** — `SampleScene` 계열 런타임 생성·구형 전장·`PrototypeGameDatabase` 등 공용 프로토타입 인프라.
- **`Selection/`** — 유닛 선택·드래그·명령 입력(`PrototypeSelectionController`).
- **`Settings/`** — 볼륨·감도·전체화면 등 플레이어 설정(PlayerPrefs).
- **`Units/`** — 유닛 이동·전투·체력·정의(`UnitDefinition` / `UnitArchetype`) — **Battle Aces·Prototype 공통**.

### 6.2 레이어 경계 (Prototype / Battle Aces / Campaign)
- **`Prototype`** — 과거 대형 전장 프로토타입. `PrototypeBootstrapper`가 씬을 통째로 깎아 쓴다. 여기서만 쓰는 연출·거점 로직이 섞일 수 있다.
- **`BattleAces`** — “짧은 전투 데모”의 기준 루프. 코어·덱·자원·승패·(옵션) 미션 목표 런타임. `Prototype`의 DB/팩토리를 **가져다 쓰지만** 부트스트랩은 `BattleAcesSceneBootstrapper`가 담당.
- **`Campaign`** — 메뉴·`MissionDefinition`·대사·해금·브리핑/결과 UI 흐름. 전투 규칙 자체는 Battle Aces에 두고, Campaign은 “어떤 미션을 싣는지”만 정한다.
- **의존 방향**: `Campaign` → `BattleAces`·`Prototype`(데이터) 는 자연스럽다. 반대로 `PrototypeBootstrapper`가 Campaign을 직접 알 필요는 없다.
- **삭제된 실험**: 과거 함대 실험(`Assets/Scripts/Fleet`, `FleetExperimentScene`)은 MVP 범위 축소로 제거되었다. 재도입은 목표 정한 뒤 별도 검토.
- 클래스명, 파일명, MonoBehaviour명은 일치시킨다.
- 네이밍은 Unity/C# 관례를 따른다.
- 불필요한 매니저 남발을 피한다.
- 지금 단계에서는 범용 시스템보다 명확한 구현을 우선한다.
- Blender 원본 파일과 최종 사용 에셋은 구분해서 보관한다.
- GitHub에는 작업 중간 결과라도 빌드보다 소스와 설정 파일 중심으로 관리한다.

## 7. 테스트/검증 방식
- 새 기능을 만들면 먼저 Unity 에디터에서 직접 플레이해 확인한다.
- 검증 기준은 코드 완성보다 실제 조작 가능 여부다.
- 최소 검증 항목:
- 유닛이 선택되는가
- 이동 명령이 동작하는가
- 전투가 발생하는가
- 피해와 사망 처리가 보이는가
- 승패가 끝까지 진행되는가
- 치명적인 오류 없이 Play 모드가 유지되는가
- 자동 테스트는 필요할 때만 추가하고, 초반에는 수동 플레이 테스트를 기본으로 한다.
- 버그 수정 시에는 재현 절차와 수정 후 확인 절차를 짧게 남긴다.

## 8. 완료 기준
- 플레이어가 카메라를 조작할 수 있다.
- 플레이어가 아군 유닛을 선택하고 이동시킬 수 있다.
- 적과 조우하면 전투가 성립한다.
- 유닛이 피해를 받고 죽는다.
- 전투 종료 조건이 있다.
- 처음 실행한 사람이 "전투 프로토타입"이라고 이해할 수 있다.
- Windows PC에서 다시 실행 가능한 상태로 정리되어 있다.
- 다음 작업자가 이어서 개발할 수 있게 폴더와 스크립트 역할이 과하게 꼬여 있지 않다.
