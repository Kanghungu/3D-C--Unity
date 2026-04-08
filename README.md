# 3D-Unity

Unity 기반 **3D SF 싱글플레이 RTS** — **지금 개발의 핵심은 Battle Aces 한 판짜리 데모**다. 캠페인 미션 늘리기는 옵션.

## 먼저 읽을 것

1. **`AGENTS.md`** — 작업 원칙(데모 우선), 폴더 역할, 테스트 기준  
2. **`DEVLOG.md`** — 비전 → 집에서 확인(데모 스모크) → 진행 표 → 최근 요약  
3. **`Assets/Docs/CHAPTER1_SMOKE_CHECKLIST.txt`** — 회귀 시 **고정 순서**  
4. **`task_brief.json`** — (선택) 목표·우선순위 스냅샷  

## 실행 흐름 (요약)

- **본전**: `NewSampleScene` **Battle Aces** 전투 — 메뉴 **데모**(쉬움·보통·어려움) 또는 **씬만 단독 Play**(폴백 데모) → 승패 → 재시작·메인.  
- **메뉴**: `CampaignMenu` → 메인에서 **데모 (본전)** 가 위쪽. **Esc** / **← 메인 (Esc)** 로 하위 화면 복귀. **캠페인** 은 옵션.  
- **스모크**: `Assets/Docs/CHAPTER1_SMOKE_CHECKLIST.txt` · `Assets/Docs/CAMPAIGN_SMOKE_CHECKLIST.txt`(데모 요약 + 캠페인 6미션은 선택).  
- **구형 실험**: `SampleScene` + `PrototypeBootstrapper` — 참고용, 신규 기능은 **Battle Aces** 우선.  
- 빌드: `File → Build Settings` — `CampaignMenu`, `NewSampleScene`.

## 집에서 테스트

**집 PC 빌드 확인(한 줄)**: `Build` 후 실행 파일로 **`CampaignMenu` → 메인 → 데모** 한 판(승 또는 패까지) 재현. (여유 시 캠페인 미션 1.)

에디터에서는 **`CHAPTER1_SMOKE_CHECKLIST.txt`** 순서대로: 데모·단독 씬 우선. 콘솔 **에러(빨강)** 없음.

전투 중 **설정**(마스터·전투 믹서·화면)은 키보드 **O** — 변경 후 **설정 저장** 권장. 미니맵 크기는 **Shift+M**, 조작은 **F1**(5줄 요약·「자세히」로 전체). 생산 유닛 **집결**은 **Alt+우클릭**(지면).

**메뉴·데모·(옵션) 캠페인 진행** 은 **`Docs/CampaignManual.md`**. 전투·HUD 회귀는 **`DEVLOG.md` 2절**. 표·요약은 **3·5절**.

- **처음이거나 빌드가 안 맞을 때만** `File → Build Settings` 에 `CampaignMenu`, `NewSampleScene` 이 들어 있는지 확인한다.  
- **실행 파일로 확인**할 때는 PC 빌드 후 같은 플로우로 재현하면 된다(선택).

문제가 나면 콘솔 메시지 전체를 복사해 두면 원인 잡기 쉽다.

## 문서·에셋

| 경로 | 설명 |
|------|------|
| `Assets/Docs/CHAPTER1_SMOKE_CHECKLIST.txt` | **데모 회귀 고정 순서**(본전) |
| `Assets/Docs/CAMPAIGN_SMOKE_CHECKLIST.txt` | 데모 요약 + 캠페인 6미션(선택) |
| `Docs/README.md` | `Docs/` 안내 |
| `Docs/CampaignManual.md` | 메뉴·**데모**·(옵션) 캠페인 미션 확인 |
| `Docs/VisualBoards/` | 참고용 SVG 보드 |

## 스크립트 (선택)

- `scripts/Start-HomeWork.ps1` — Git 상태 + 핵심 문서 목록  
- `scripts/Show-CurrentContext.ps1` — `README`/`DEVLOG`/`task_brief.json` 일부 출력  
- `scripts/Build-Windows.ps1` — PC 빌드 시 **Unity.exe 경로** 안내(자동 빌드는 로컬 Unity 설치에 맞게 조정)  
- Unity 에디터 **Game/Campaign/Verify Chapter 1 Build Scenes** — `CampaignMenu`·`NewSampleScene` 빌드 포함·파일 존재 로그  
- Unity 에디터 **Tools/프로젝트/빌드 씬 목록 검사** — `Build Settings` 씬 파일 존재 여부 로그  

1인 개발·프로토타입 우선 기준은 **`AGENTS.md`** 를 따릅니다.
