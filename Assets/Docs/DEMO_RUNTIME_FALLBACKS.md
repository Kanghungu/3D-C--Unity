# 데모·런타임 폴백 패턴 (크래시 방지)

Unity 전투 씬은 메뉴 없이 Play 하거나, 대사 테이블·미션 데이터가 비어 있어도 **한 줄 로그 + 플레이어에게 보이는 폴백 문구**로 끊기지 않게 한다.

## ActiveMission / PersistentGameCore

- **위치**: `BattleAcesSceneBootstrapper.Awake`
- **패턴**: `PersistentGameCore.Instance.ActiveMission` 이 null 이면 런타임 `MissionDefinition` 을 만들어 `AssignFallbackOneMatchDemoRuntime()` 후 `SetActiveMission`.
- **로그**: `[BattleAces] ActiveMission 없음 → Battle Aces 한 판 데모(폴백) 런타임 미션을 설정했습니다.`

## 대사 키 누락 (TryGetDialogue)

- **위치**: `PersistentGameCore.TryGetDialogue` → 테이블 없음/키 없음 시 `null`.
- **로그**: 누락된 **키 ID당 1회** `Debug.LogWarning("[PersistentGameCore] Dialogue 누락 키 — 폴백 문구 사용: …")`.
- **UI 폴백**: `BattleMissionFlow`·`DrawBriefingScreen` 등 + 패배 재도전 힌트는 `GetBuiltInDefeatRetryHint` 등 **코드 폴백 한 줄**을 둔다.

## 캠페인 진행 저장의 한계

- **저장됨**: 미션 클리어 플래그, 해금 인덱스, (선택) 보너스 목표, 마지막 시작 미션 순번(`SaveLastCampaignMissionOrderIndex`).
- **저장 안 됨**: 전투 중간 상태(유닛·자원·시간). **중간 세이브**는 후순위 — `CampaignMenuController` 캠페인 화면 하단 안내와 동일.

## 기타

- **이어하기**: `CampaignProgressStorage.GetSuggestedContinueMissionOrderIndex` — 잠금 해제 범위 안에서 **아직 클리어 안 한 가장 앞 미션**; 전부 클리어 시 인덱스 `0` 재도전.
