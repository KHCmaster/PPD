# ビルドで書き出した SARIF（Win\Directory.Build.props の AnalysisLogDir）を読み、
# 抑止されていない診断（error、warning、info）が 1 つでもあれば失敗する。
# 診断は GitHub Actions の注釈として出力する（プルリクエストの差分に表示される）
param(
    [Parameter(Mandatory)][string]$LogDir,
    [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($RepoRoot).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
$files = @(Get-ChildItem -Path $LogDir -Filter *.sarif)
if ($files.Count -eq 0) {
    Write-Output "::error::SARIF ファイルが $LogDir にありません"
    exit 1
}

$seen = @{}
foreach ($file in $files) {
    $sarif = Get-Content -Raw -Encoding utf8 $file.FullName | ConvertFrom-Json
    foreach ($run in $sarif.runs) {
        foreach ($result in $run.results) {
            if ($result.suppressions) { continue }
            $level = if ($result.level) { $result.level } else { 'warning' }
            $path = ''
            $line = 0
            $column = 0
            if ($result.locations) {
                $location = $result.locations[0].physicalLocation
                $path = ([Uri]$location.artifactLocation.uri).LocalPath
                if ($path.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
                    $path = $path.Substring($root.Length)
                }
                $path = $path.Replace('\', '/')
                $line = $location.region.startLine
                $column = $location.region.startColumn
            }
            # 複数のプロジェクトに含まれるファイル（リンクしたファイルなど）は同じ診断が重なるので 1 回だけ出す。
            # 位置のない診断（アセンブリ全体についての診断）はプロジェクトごとに出す
            $key = if ($path) { "$level|$($result.ruleId)|$path|$line|$column" } else { "$level|$($result.ruleId)|$($file.BaseName)" }
            if ($seen.ContainsKey($key)) { continue }
            $seen[$key] = $true
            $command = switch ($level) { 'error' { 'error' } 'warning' { 'warning' } default { 'notice' } }
            $message = "$($result.ruleId): $($result.message.text)"
            if ($path) {
                Write-Output "::$command file=$path,line=$line,col=$column::$message"
            }
            else {
                Write-Output "::$command::$($file.BaseName): $message"
            }
        }
    }
}

Write-Output "$($files.Count) 個の SARIF から $($seen.Count) 件の診断"
if ($seen.Count -gt 0) { exit 1 }
