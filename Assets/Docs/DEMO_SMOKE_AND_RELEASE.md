# 데모 완료 정의 — Windows 빌드 + 스모크 순서

**원칙**: 동일 순서(0 빌드 → 1 공통 스모크 → **2 ClassicDuel 한 판** → 3 캠페인 …)로 매 빌드 검증하면 “데모 나갈 수 있는 상태”를 재현할 수 있다.

## 0. 빌드

1. Unity **File → Build Settings → Windows** → 출력 폴더에 빌드.
2. 기본 진입 씬은 **CampaignMenu** (또는 프로젝트가 지정한 첫 씬).

## 1. 공통 짧은 스모크

`Assets/Docs/BUILD_SMOKE_CHECKLIST.txt` 를 위에서부터 순서대로.

## 2. Battle Aces · ClassicDuel 한 판 (연출·승패 — 회귀 1줄)

**NewSampleScene** 등 ClassicDuel 레이아웃으로 **한 판** 플레이하며 한 번에 훑기: **포그**(시작·거리·강도 슬라이더 반응) · **지면**(클래식 듀얼 지형·그라데이션·장애 실루엣) · **비네팅**(URP/볼륨 후처리) · **주스**(피격·생산·명령 거절 등 짧은 피드백) · **승/패**(결과 카드, R 재시작·Esc 메뉴)까지 시각·입력·오류 로그 끊김 없음.

## 3. 캠페인 흐름

`Assets/Docs/CAMPAIGN_SMOKE_CHECKLIST.txt` 전 항목(에셋이 있을 때).

## 4. 챕터별(있을 때만)

아래 **번호 순서**로 진행한다.

| 순서 | 문서 |
|------|------|
| 1 | `CHAPTER1_SMOKE_CHECKLIST.txt` |
| 2 | `CHAPTER2_SMOKE_CHECKLIST.txt` |
| 3 | `CHAPTER3_SMOKE_CHECKLIST.txt` |
| 4 | `CHAPTER4_SMOKE_CHECKLIST.txt` |
| 5 | `CHAPTER5_SMOKE_CHECKLIST.txt` |
| 6 | `CHAPTER6_SMOKE_CHECKLIST.txt` |
| 7 | `CHAPTER7_SMOKE_CHECKLIST.txt` |

## 5. 약속 문구 (한 줄)

코드 단일 출처: `DemoPresentationCopy` (`Assets/Scripts/UI/DemoPresentationCopy.cs`).  
메뉴·HUD·토스트와 달리 쓰지 말고 여기만 수정한다.

## 6. 관련 문서

- `DEMO_RUNTIME_FALLBACKS.md` — ActiveMission·대사 폴백·저장 범위.
- `DEMO_IMPLEMENTATION_PRIORITY.md` — 기능 우선순위·잔여 작업 표.
- `BATTLE_ACES_ART_DIRECTION.md` · `BATTLE_ACES_READABILITY.md` — ClassicDuel 무대·포그·UI 톤 맥락(§2 회귀와 함께 보면 됨).
