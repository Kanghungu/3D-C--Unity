# Windows 플레이어 빌드 (Unity 에디터가 설치된 PC에서만 동작)
# 사용법:
#   $env:UNITY_EXE = "C:\Program Files\Unity\Hub\Editor\6000.x.x\Editor\Unity.exe"
#   .\scripts\Build-Windows.ps1
#
# 또는 Unity Hub에서 설치 경로를 확인한 뒤 UNITY_EXE 를 맞춥니다.
# 에디터 메뉴 Tools/프로젝트/빌드 씬 목록 검사 로 씬 경로를 먼저 확인하는 것을 권장합니다.

param(
    [string]$UnityExe = $env:UNITY_EXE,
    [string]$OutputPath = "Build\WindowsPlayer"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $UnityExe -or -not (Test-Path $UnityExe)) {
    Write-Host "UNITY_EXE 환경 변수에 Unity.exe 전체 경로를 설정하거나 -UnityExe 로 넘겨 주세요." -ForegroundColor Yellow
    Write-Host "예: `$env:UNITY_EXE = 'C:\Program Files\Unity\Hub\Editor\...\Editor\Unity.exe'" -ForegroundColor Gray
    exit 1
}

$out = Join-Path $ProjectRoot $OutputPath
New-Item -ItemType Directory -Force -Path $out | Out-Null

# Unity 빌드는 프로젝트에 Editor 스크립트(BuildPipeline 래퍼)가 있어야 안정적입니다.
# 현재 저장소는 File > Build Settings > Build 로 수동 빌드를 권장합니다.
Write-Host "Unity 경로: $UnityExe"
Write-Host "프로젝트: $ProjectRoot"
Write-Host "출력(예정): $out"
Write-Host ""
Write-Host "자동 CLI 빌드는 환경마다 다릅니다. Unity 에디터에서 File > Build Settings > PC 빌드 를 권장합니다." -ForegroundColor Cyan
exit 0
