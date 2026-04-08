# 데모 완료 정의 — Windows 빌드 + 스모크 순서

**원칙**: 동일 순서로 매 빌드 검증하면 “데모 나갈 수 있는 상태”를 재현할 수 있다.

## 0. 빌드

1. Unity **File → Build Settings → Windows** → 출력 폴더에 빌드.
2. 기본 진입 씬은 **CampaignMenu** (또는 프로젝트가 지정한 첫 씬).

## 1. 공통 짧은 스모크

`Assets/Docs/BUILD_SMOKE_CHECKLIST.txt` 를 위에서부터 순서대로.

## 2. 캠페인 흐름

`Assets/Docs/CAMPAIGN_SMOKE_CHECKLIST.txt` 전 항목(에셋이 있을 때).

## 3. 챕터별(있을 때만)

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

## 4. 약속 문구 (한 줄)

코드 단일 출처: `DemoPresentationCopy` (`Assets/Scripts/UI/DemoPresentationCopy.cs`).  
메뉴·HUD·토스트와 달리 쓰지 말고 여기만 수정한다.

## 5. 관련 문서

- `DEMO_RUNTIME_FALLBACKS.md` — ActiveMission·대사 폴백·저장 범위.
- `DEMO_IMPLEMENTATION_PRIORITY.md` — 기능 우선순위·잔여 작업 표.
