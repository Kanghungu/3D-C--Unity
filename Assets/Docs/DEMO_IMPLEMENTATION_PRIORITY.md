# 데모 기능 — 구현 우선순위 표

이번 작업에서 **반영된 것**과 **후순위**를 구분한다.

| 우선 | 항목 | 상태 | 비고 |
|------|------|------|------|
| P0 | 한 줄 약속 문구 통일 | **완료** | `DemoPresentationCopy` + 메뉴·HUD·토스트 연동 |
| P0 | Windows 빌드 + 스모크 순서 문서 | **완료** | `DEMO_SMOKE_AND_RELEASE.md` |
| P0 | 대사 누락 로그 + 패배 힌트 폴백 | **완료** | `PersistentGameCore` 키당 1회 경고, `GetBuiltInDefeatRetryHint` |
| P0 | 승패 스팅 재생 실패 시 무해화 | **완료** | `ProceduralAudioUtility.PlayResultSting` try/catch |
| P1 | 그래픽 프리셋 2단 | **완료** | `GameUserSettings.DemoGraphicQualityPreset` + 설정 패널 |
| P1 | 일시정지·결과 입력 안내 순서 통일 | **완료** | `BuildPauseAndResultInputBlockKo` + 결과 카드 R→Esc |
| P1 | 데모 변주(같은 맵·다른 적 패턴) | **완료** | 데모 화면 「압박 물결」+ `ApplySkirmishVariantEnemyPattern` |
| P1 | 크레딧·빌드 번호 한 줄 | **완료** | 메뉴 하단 `Application.version` |
| P1 | 키보드 전용 안내 카드 | **완료** | 데모 탭 키 안내 패널 |
| P2 | 승리 순간 카메라 살짝 정리 | **완료** | `BattleAcesVictoryPresentation` |
| — | URP/HDRP 에셋까지 바꾸는 품질 단계 | **후순위** | 지금은 `QualitySettings` 그림자·스킨 가중치 수준 |
| — | 게임패드·키 리바인드 | **후순위** | 문구로 미지원 명시만 |
| — | 승리 시 UI 페이드·레터박스 심화 | **후순위** | 카메라 당김만 적용 |
| — | 자동화 CI 스모크 | **후순위** | 수동 체크리스트 유지 |

**권장 다음 작업**: Steam 페이지용 스크린샷 3장 + `BUILD_SMOKE_CHECKLIST` 를 빌드 파이프라인 메모에 고정.
