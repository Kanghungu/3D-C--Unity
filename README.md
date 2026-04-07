# 3D-Unity

Unity 기반 **3D SF 싱글플레이 RTS** — 지금은 **전투·캠페인 프로토타입**이 우선입니다.

## 먼저 읽을 것

1. **`AGENTS.md`** — 작업 원칙, 폴더 역할, 테스트 기준  
2. **`DEVLOG.md`** — 비전 → 집에서 확인 → 전체 진행 표 → 버그 메모 → 최근 요약  
3. **`task_brief.json`** — (선택) 목표·우선순위 스냅샷  

## 실행 흐름 (요약)

- **캠페인**: `CampaignMenu` 씬 → 미션 선택 → `NewSampleScene`(Battle Aces 전투).  
- **구형 실험**: `SampleScene` + `PrototypeBootstrapper` — 참고용, 신규 기능은 BA+Campaign 우선.  
- 빌드에 씬 등록: `File → Build Settings` (`CampaignMenu`, `NewSampleScene` 등).

## 집에서 테스트

**집 PC 빌드 확인(한 줄)**: 가끔은 `File → Build Settings → Build` 로 Windows 실행 파일을 한 번 뽑아, 에디터와 같이 `CampaignMenu` → 미션 1 플로우만 재현해 본다.

프로젝트를 열고 `CampaignMenu` 에서 **Play** → 캠페인 미션을 돌려 본다. 콘솔에 **에러(빨강)** 가 없는지 본다.

전투 중 **설정**(마스터·전투 믹서·화면)은 키보드 **O** — 변경 후 **설정 저장** 권장. 미니맵 크기는 **Shift+M**, 조작은 **F1**(5줄 요약·「자세히」로 전체).

**자세한 확인 항목**(미니맵 박스 선택·생산 줄·종료 사유·진행 ●/○·V 저장·우클릭 거절·FoW 등)은 **`DEVLOG.md` → §2 집에서 확인할 일** 을 따른다. 전체 어느 파트가 끝났는지는 **§3 전체 진행 요약** 표를 본다. 한 줄 요약은 **§5 최근 요약**.

- **처음이거나 빌드가 안 맞을 때만** `File → Build Settings` 에 `CampaignMenu`, `NewSampleScene` 이 들어 있는지 확인한다.  
- **실행 파일로 확인**할 때는 PC 빌드 후 같은 플로우로 재현하면 된다(선택).

문제가 나면 콘솔 메시지 전체를 복사해 두면 원인 잡기 쉽다.

## 문서·에셋

| 경로 | 설명 |
|------|------|
| `Docs/README.md` | `Docs/` 안내 |
| `Docs/VisualBoards/` | 참고용 SVG 보드(과거 `Reference/Assets` 이전) |

## 스크립트 (선택)

- `scripts/Start-HomeWork.ps1` — Git 상태 + 핵심 문서 목록  
- `scripts/Show-CurrentContext.ps1` — `README`/`DEVLOG`/`task_brief.json` 일부 출력  
- `scripts/Build-Windows.ps1` — PC 빌드 시 **Unity.exe 경로** 안내(자동 빌드는 로컬 Unity 설치에 맞게 조정)  
- Unity 에디터 **Tools/프로젝트/빌드 씬 목록 검사** — `Build Settings` 씬 파일 존재 여부 로그  

1인 개발·프로토타입 우선 기준은 **`AGENTS.md`** 를 따릅니다.
