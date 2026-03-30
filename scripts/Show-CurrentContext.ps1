Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$OutputEncoding = [Console]::OutputEncoding

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Show-Section {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Head = 30
    )

    if (-not (Test-Path $Path)) {
        return
    }

    Write-Host ""
    Write-Host "== $Path ==" -ForegroundColor Cyan
    Get-Content $Path -Encoding utf8 | Select-Object -First $Head
}

Show-Section -Path "START_HERE.md" -Head 40
Show-Section -Path "SESSION_CONTEXT.md" -Head 50
Show-Section -Path "TODO.md" -Head 50

if (Test-Path "task_brief.json") {
    Write-Host ""
    Write-Host "== task_brief.json ==" -ForegroundColor Cyan
    Get-Content "task_brief.json" -Encoding utf8
}

Write-Host ""
Write-Host "Keep the main line focused on the ground combat RTS prototype." -ForegroundColor Yellow
Write-Host "Do not overwrite local uncommitted work unless the user explicitly asks." -ForegroundColor Yellow