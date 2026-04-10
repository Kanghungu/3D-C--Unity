# 레퍼·주간 캡처 플레이스홀더 PNG 생성 (저작권 없는 그라데이션). 실제 스틸/플레이 캡처로 교체하세요.
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Write-GradientPng {
    param(
        [string]$path,
        [int]$w,
        [int]$h,
        [int]$r0, [int]$g0, [int]$b0,
        [int]$r1, [int]$g1, [int]$b1,
        [bool]$tealRim = $false
    )
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $c0 = [System.Drawing.Color]::FromArgb(255, $r0, $g0, $b0)
    $c1 = [System.Drawing.Color]::FromArgb(255, $r1, $g1, $b1)
    $p0 = New-Object System.Drawing.Point 0, 0
    $p1 = New-Object System.Drawing.Point $w, $h
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $p0, $p1, $c0, $c1
    $gfx.FillRectangle($brush, 0, 0, $w, $h)
    if ($tealRim) {
        $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(180, 46, 180, 200), 3)
        $gfx.DrawEllipse($pen, [int]($w * 0.38), [int]($h * 0.32), [int]($w * 0.24), [int]($h * 0.2))
    }
    $gfx.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

Write-GradientPng (Join-Path $dir "reference_external_mood_01.png") 640 360 28 32 42 55 62 78 $false
Write-GradientPng (Join-Path $dir "reference_external_mood_02.png") 640 360 18 20 28 42 48 58 $true
Write-GradientPng (Join-Path $dir "reference_external_mood_03.png") 640 360 35 38 48 22 24 32 $false
Write-GradientPng (Join-Path $dir "target_sprint_reference.png") 960 540 12 14 20 24 28 38 $true
Write-GradientPng (Join-Path $dir "weekly_compare_1920x1080.png") 1920 1080 20 24 32 38 44 58 $false
Write-GradientPng (Join-Path $dir "weekly_compare_1280x720.png") 1280 720 20 24 32 38 44 58 $false
Write-Host "Wrote PNGs to $dir"
