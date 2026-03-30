Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "== 3D-Unity Home AI Harness ==" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project Root: $root"
Write-Host ""

Write-Host "[Git Status]" -ForegroundColor Yellow
git status --short
Write-Host ""

Write-Host "[Core Docs]" -ForegroundColor Yellow
$coreDocs = @(
    "AGENTS.md",
    "START_HERE.md",
    "SESSION_CONTEXT.md",
    "TODO.md",
    "DEVLOG.md",
    "HOME_AI_PROMPTS.md",
    "HOME_AI_HARNESS.md"
)

foreach ($doc in $coreDocs) {
    if (Test-Path $doc) {
        Write-Host " - $doc"
    }
}

Write-Host ""
Write-Host "[Recommended Next Step]" -ForegroundColor Yellow
Write-Host "1. Run ./scripts/Show-CurrentContext.ps1"
Write-Host "2. Paste a prompt from HOME_AI_PROMPTS.md"
Write-Host "3. Ask AI to keep working through implementation, verification, and docs"