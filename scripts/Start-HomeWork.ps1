Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "== 3D-Unity project context ==" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project Root: $root"
Write-Host ""

Write-Host "[Git Status]" -ForegroundColor Yellow
git status --short
Write-Host ""

Write-Host "[Core Docs]" -ForegroundColor Yellow
$coreDocs = @(
    "AGENTS.md",
    "README.md",
    "DEVLOG.md",
    "task_brief.json"
)

foreach ($doc in $coreDocs) {
    if (Test-Path $doc) {
        Write-Host " - $doc"
    }
}

Write-Host ""
Write-Host "[Recommended]" -ForegroundColor Yellow
Write-Host "1. Run ./scripts/Show-CurrentContext.ps1"
Write-Host "2. Read AGENTS.md then work in small verifiable steps"
