<#
.SYNOPSIS
単体テスト（UnitTests）と UI テスト（UITests）のカバレッジを測り、合わせた結果をプロジェクトごとに表示します。

.DESCRIPTION
dotnet-coverage を使う（dotnet tool install --global dotnet-coverage）。UI テストで起動したアプリ（PPDEditor など）も、
子のプロセスとして測る。先にソリューションを Debug|x64 でビルドしておく。
結果は CoverageResults フォルダーに、unit.coverage、ui.coverage と、合わせた merged.cobertura.xml を書く。

.PARAMETER SkipUI
UI テストを流さず、単体テストだけを測ります（UI テストは数分かかる）。
#>
param(
    [switch]$SkipUI
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$results = Join-Path $root 'CoverageResults'
$config = Join-Path $root 'coverage.config.xml'
if (Test-Path $results) {
    Remove-Item -Recurse -Force $results
}
New-Item -ItemType Directory -Path $results | Out-Null

function Invoke-Coverage([string]$project, [string]$output) {
    Push-Location (Join-Path $root $project)
    try {
        dotnet-coverage collect --settings $config -f coverage -o $output "dotnet test --no-build -c Debug -p:Platform=x64"
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "$project のテストに失敗があります（結果はそのまま合わせる）"
        }
    }
    finally {
        Pop-Location
    }
}

$files = @()
Invoke-Coverage 'UnitTests' (Join-Path $results 'unit.coverage')
$files += Join-Path $results 'unit.coverage'
if (-not $SkipUI) {
    Invoke-Coverage 'UITests' (Join-Path $results 'ui.coverage')
    $files += Join-Path $results 'ui.coverage'
}

$merged = Join-Path $results 'merged.cobertura.xml'
dotnet-coverage merge -f cobertura -o $merged @files

# プロジェクトごとに、通った行の割合を表示する
[xml]$xml = Get-Content -Raw -Encoding UTF8 $merged
$total = 0
$covered = 0
$rows = foreach ($package in $xml.coverage.packages.package) {
    $lines = @($package.SelectNodes('.//class/lines/line'))
    if ($lines.Count -eq 0) { continue }
    $hit = @($lines | Where-Object { $_.hits -ne '0' }).Count
    $total += $lines.Count
    $covered += $hit
    [pscustomobject]@{ Project = $package.name; Percent = [math]::Round(100 * $hit / $lines.Count, 1); Covered = $hit; Lines = $lines.Count }
}
$rows | Sort-Object Project | Format-Table -AutoSize
'全体: {0:N1}% ({1}/{2})' -f (100 * $covered / $total), $covered, $total
