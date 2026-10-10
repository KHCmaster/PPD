# ソリューション内の全プロジェクトの出力アセンブリの中身を書き出す（csproj の変換前後で比較するため）。
# 書き出す内容: アセンブリ名・バージョン、参照アセンブリ、埋め込みリソース（名前とハッシュ）、型の一覧、出力フォルダーのファイル一覧。
#
# 使い方:
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools\ProjectCheck\AssemblyDump.ps1 -Out before.txt [-Configuration Release]
#   （比較は Compare-Object などで行う。ビルドは事前に済ませておく）
param(
    [Parameter(Mandatory = $true)][string]$Out,
    [string]$Configuration = 'Debug',
    [string]$Platform = 'x64'
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
$sln = Get-ChildItem $root -Filter *.sln | Select-Object -First 1
$projects = Select-String -Path $sln.FullName -Pattern '"([^"]+\.csproj)"' | ForEach-Object { Join-Path $root $_.Matches[0].Groups[1].Value }

$targets = @()
foreach ($project in $projects) {
    # プロパティを 1 つだけ指定すると、JSON ではなく値がそのまま出力される
    $targetPath = (& $msbuild $project -nologo "-p:Configuration=$Configuration" "-p:Platform=$Platform" -getProperty:TargetPath | Out-String).Trim()
    $targets += [pscustomobject]@{ Project = $project.Substring($root.Path.Length + 1); TargetPath = $targetPath }
}

# アセンブリを読み込むとファイルがロックされるので、別プロセスで調べる
$worker = {
    param($project, $path)
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("## $project")
    if (-not (Test-Path $path)) { $lines.Add("  missing output: $path"); return $lines }
    $dir = Split-Path $path
    [AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve({
        param($s, $e)
        $name = (New-Object Reflection.AssemblyName($e.Name)).Name
        foreach ($candidate in @((Join-Path $dir "$name.dll"), (Join-Path $dir "$name.exe"))) {
            if (Test-Path $candidate) { return [Reflection.Assembly]::ReflectionOnlyLoadFrom($candidate) }
        }
        try { return [Reflection.Assembly]::ReflectionOnlyLoad($e.Name) } catch { return $null }
    })
    $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($path)
    $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($path)
    $lines.Add("  output: $([IO.Path]::GetFileName($path))")
    $lines.Add("  name: $($assembly.FullName)")
    $lines.Add("  fileversion: $($version.FileVersion)")
    $lines.Add("  runtime: $($assembly.ImageRuntimeVersion)")
    $assembly.GetReferencedAssemblies() | Sort-Object FullName | ForEach-Object { $lines.Add("  ref: $($_.FullName)") }
    $sha = [Security.Cryptography.SHA256]::Create()
    foreach ($resource in ($assembly.GetManifestResourceNames() | Sort-Object)) {
        $stream = $assembly.GetManifestResourceStream($resource)
        $hash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').Substring(0, 16)
        $lines.Add("  resource: $resource $hash")
    }
    try { $types = $assembly.GetTypes() } catch [Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ } }
    $types | ForEach-Object { $_.FullName } | Sort-Object | ForEach-Object { $lines.Add("  type: $_") }
    return $lines
}

$result = New-Object System.Collections.Generic.List[string]
foreach ($target in $targets) {
    $job = Start-Job -ScriptBlock $worker -ArgumentList $target.Project, $target.TargetPath
    $lines = Receive-Job $job -Wait -AutoRemoveJob -ErrorAction Continue
    $lines | ForEach-Object { $result.Add($_) }
}

# 実行に使う出力フォルダーのファイル一覧
$outputDirs = if ($Configuration -eq 'Debug') { @('PPD\PPD\bin\x64\Debug', 'PPDeditor\PPDeditor\bin\x64\Debug', 'PPDTest\bin\x64\Debug') } else { @('release\x64\PPD') }
foreach ($outputDir in $outputDirs) {
    $full = Join-Path $root $outputDir
    $result.Add("## files $outputDir")
    if (Test-Path $full) {
        Get-ChildItem $full -Recurse -File | Where-Object { $_.Extension -in '.dll', '.exe', '.config' } |
            ForEach-Object { $_.FullName.Substring($full.Length + 1) } | Sort-Object | ForEach-Object { $result.Add("  $_") }
    }
}

[IO.File]::WriteAllLines($Out, $result, (New-Object Text.UTF8Encoding($false)))
"projects: $($targets.Count), lines: $($result.Count) -> $Out"
