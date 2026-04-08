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
        [int]$Head = 40
    )

    if (-not (Test-Path $Path)) {
        return
    }

    Write-Host ""
    Write-Host "== $Path ==" -ForegroundColor Cyan
    Get-Content $Path -Encoding utf8 | Select-Object -First $Head
}

Show-Section -Path "README.md" -Head 40
Show-Section -Path "DEVLOG.md" -Head 55
Show-Section -Path "Assets/Docs/CHAPTER1_SMOKE_CHECKLIST.txt" -Head 22
Show-Section -Path "Docs/CampaignManual.md" -Head 22

if (Test-Path "task_brief.json") {
    $taskBrief = Get-Content "task_brief.json" -Encoding utf8 | ConvertFrom-Json

    if ($taskBrief.priority_work_queue) {
        Write-Host ""
        Write-Host "== Priority Snapshot ==" -ForegroundColor Cyan

        Write-Host "[Immediate]" -ForegroundColor Yellow
        foreach ($item in $taskBrief.priority_work_queue.immediate) {
            Write-Host " - $item"
        }

        Write-Host ""
        Write-Host "[Soon]" -ForegroundColor Yellow
        foreach ($item in $taskBrief.priority_work_queue.soon) {
            Write-Host " - $item"
        }

        Write-Host ""
        Write-Host "[Later]" -ForegroundColor Yellow
        foreach ($item in $taskBrief.priority_work_queue.later) {
            Write-Host " - $item"
        }
    }

    Write-Host ""
    Write-Host "== task_brief.json (full) ==" -ForegroundColor Cyan
    Get-Content "task_brief.json" -Encoding utf8
}

Write-Host ""
Write-Host "Main line: Campaign + Battle Aces. See AGENTS.md." -ForegroundColor Yellow
Write-Host "Do not overwrite local uncommitted work unless the user explicitly asks." -ForegroundColor Yellow
