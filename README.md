# 3D-Unity

Unity 기반 **3D SF 싱글플레이 RTS** — 지금은 **전투·캠페인 프로토타입**이 우선입니다.

## 먼저 읽을 것

1. **`AGENTS.md`** — 작업 원칙, 폴더 역할, 테스트 기준  
2. **`DEVLOG.md`** — 현재 진행 요약·비전·최근 기록  
3. **`task_brief.json`** — (선택) 목표·우선순위 스냅샷  

## 실행 흐름 (요약)

- **캠페인**: `CampaignMenu` 씬 → 미션 선택 → `NewSampleScene`(Battle Aces 전투).  
- **구형 실험**: `SampleScene` + `PrototypeBootstrapper` — 참고용, 신규 기능은 BA+Campaign 우선.  
- 빌드에 씬 등록: `File → Build Settings` (`CampaignMenu`, `NewSampleScene` 등).

## 집에서 테스트

프로젝트를 열고 `CampaignMenu` 에서 **Play** → 캠페인 미션을 돌려 본다. 콘솔에 **에러(빨강)** 가 없는지 본다.

**자세한 확인 항목**(미니맵 박스 선택·생산 줄·종료 사유·진행 ●/○·V 저장 등)은 **`DEVLOG.md` → 「집에서 확인할 때」** 를 따른다.

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

1인 개발·프로토타입 우선 기준은 **`AGENTS.md`** 를 따릅니다.
