# Battle Aces · UGUI 이관 범위 (결정)

1인 데모에서 “대작 감”은 **한 화면씩** UGUI·폰트·레이아웃 그리드로 옮기는 편이 ROI가 큽니다.  
코드 상수: `Game.BattleAces.BattleAcesDemoQualitySprintFocus`

## 1차 이관 후보 (확정)

| 순서 | 화면 | 이유 |
|------|------|------|
| **1** | **승패 결과 카드** (`BattleAcesResultUgui` + `BattleMissionFlow.TryBuildResultCardPresentation`) | **1차 이관 완료**: 런타임 UGUI Canvas(정렬 순서 상단 바보다 높음). 브리핑은 여전히 IMGUI. |

## 2차 후보 (미확정 — 스프린트 여력 시)

- 캠페인 메인 메뉴 (`CampaignMenuController` IMGUI) — 진입 빈도 높음, 레이아웃은 패널 수가 많음.
- 브리핑 본문 — 자막 속도·스크롤과 연동되어 작업량 큼.

## 하지 않을 것 (이번 결정)

- 전투 중 HUD 전체를 한 번에 UGUI로 갈아타기 — 범위 과대. IMGUI로 가독성·팔레트만 유지.
- 인트로·시네마틱 전용 씬 신규 — 스프린트 포커스가 결과 카드일 때는 후순위.

## 구현 시 참고 파일

- 결과 UI: [`BattleAcesResultUgui.cs`](../Scripts/BattleAces/BattleAcesResultUgui.cs) · 데이터 조립 [`BattleMissionFlow.TryBuildResultCardPresentation`](../Scripts/BattleAces/BattleMissionFlow.cs) · UGUI 미부착 시 IMGUI 폴백 `DrawResultScreenImGui`
- 기존 UGUI 목표 바 레퍼: `BattleAcesObjectiveUgui` (상단 바만 UGUI인 패턴 재사용 가능)
