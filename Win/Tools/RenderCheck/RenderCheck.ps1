<#
.SYNOPSIS
描画の回帰テストと終了時の例外チェックを行います。

.DESCRIPTION
1. Tools\RenderCheck の各ツールを csc でビルドします（bin\ に出力）。
2. PPDTest の全シーンを撮影し、baseline\ の基準画像と比較します（差分は out\ に出力。-SkipScenes で省略）。
3. PPD を起動・終了し、アクセス違反が起きないことを確認します（-SkipPPD で省略）。
4. PPD の mods\ にある全 Mod（と -ScriptDirs で指定した .fsml）を読み込み、
   既定値の変換結果が基準（baseline\mods.txt）と一致することを確認します（-SkipMods で省略）。

基準画像を作り直す場合は -UpdateBaseline を付けて実行します。
PPDTest / PPD は事前に Debug|x64 でビルドしておいてください。

.EXAMPLE
powershell -ExecutionPolicy Bypass -File Tools\RenderCheck\RenderCheck.ps1
powershell -ExecutionPolicy Bypass -File Tools\RenderCheck\RenderCheck.ps1 -UpdateBaseline
#>
param(
    [switch]$UpdateBaseline,
    [switch]$SkipPPD,
    [int]$SceneCount = 11,
    # 動画など毎回内容が変わるシーン。比較結果は表示するが合否には含めない（目視で確認する）
    [int[]]$DynamicScenes = @(9, 10),
    [int]$AllowedPixels = 500,
    [switch]$SkipScenes,
    [switch]$SkipMods,
    # Mod 以外に読み込みを確認する .fsml のフォルダ（譜面フォルダなど）
    [string[]]$ScriptDirs = @()
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$toolDir = $PSScriptRoot
$binDir = Join-Path $toolDir "bin"
$outDir = Join-Path $toolDir "out"
$baselineDir = Join-Path $toolDir "baseline"
New-Item -ItemType Directory -Force $binDir, $outDir | Out-Null

# ツールのビルド
$csc = Get-ChildItem "${env:ProgramFiles}\Microsoft Visual Studio\*\*\MSBuild\Current\Bin\Roslyn\csc.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
if (-not $csc) { throw "csc.exe が見つかりません" }
foreach ($name in "SceneCapture", "ImageCompare", "ExceptionWatch") {
    & $csc /nologo /platform:x64 /r:System.Drawing.dll /out:"$binDir\$name.exe" "$toolDir\$name.cs"
    if ($LASTEXITCODE -ne 0) { throw "$name のビルドに失敗しました" }
}

$failed = $false

# シーンの撮影と比較
if ($UpdateBaseline) { New-Item -ItemType Directory -Force $baselineDir | Out-Null }
if (-not $SkipScenes) {
$ppdTest = Join-Path $root "PPDTest\bin\x64\Debug\PPDTest.exe"
$target = if ($UpdateBaseline) { $baselineDir } else { Join-Path $outDir "actual" }
& "$binDir\SceneCapture.exe" $ppdTest $target $SceneCount
if ($LASTEXITCODE -ne 0) { throw "シーンの撮影に失敗しました" }
}

if (-not $UpdateBaseline -and -not $SkipScenes) {
    for ($i = 0; $i -lt $SceneCount; $i++) {
        $expected = Join-Path $baselineDir "scene$i.png"
        $actual = Join-Path $target "scene$i.png"
        $diff = Join-Path $outDir "diff$i.png"
        $result = & "$binDir\ImageCompare.exe" $expected $actual $diff $AllowedPixels
        $code = $LASTEXITCODE
        if ($DynamicScenes -contains $i) {
            Write-Output ("scene{0}: 目視確認 ({1}) {2}" -f $i, $actual, $result)
            continue
        }
        Write-Output ("scene{0}: {1} {2}" -f $i, $(if ($code -eq 0) { "OK" } else { "NG" }), $result)
        if ($code -ne 0) { $failed = $true }
    }
}

# 終了時の例外チェック
if (-not $SkipPPD) {
    $ppd = Join-Path $root "PPD\PPD\bin\x64\Debug\PPD.exe"
    $log = Join-Path $outDir "exception.log"
    # PPD 自身が捕まえた例外は error.log に書かれ、エラーダイアログを出して終了する（アクセス違反のチェックでは分からない）
    $errorLog = Join-Path (Split-Path $ppd) "error.log"
    $errorLogSize = if (Test-Path $errorLog) { (Get-Item $errorLog).Length } else { 0 }
    # 25 秒後に閉じる。終了確認画面では Circle（既定のキー設定では K = スキャンコード 0x25）で確定する
    & "$binDir\ExceptionWatch.exe" $ppd "-disableexpansion" 25 $log 25
    $exitCode = $LASTEXITCODE
    $errorLogGrew = (Test-Path $errorLog) -and (Get-Item $errorLog).Length -gt $errorLogSize
    if ($exitCode -ne 0) {
        Write-Output "PPD: NG (アクセス違反あり。$log を確認してください)"
        $failed = $true
    }
    elseif ($errorLogGrew) {
        Write-Output "PPD: NG (エラーが発生しました。$errorLog を確認してください)"
        $failed = $true
    }
    else {
        Write-Output "PPD: OK"
    }
}

# Mod / FlowScript の互換チェック
if (-not $SkipMods) {
    $ppdBin = Join-Path $root "PPD\PPD\bin\x64\Debug"
    # PPD と同じ .NET で動かすため、プロジェクトとしてビルドする
    & dotnet build "$toolDir\ModCheck\ModCheck.csproj" -c Release -o "$binDir\ModCheck" -nologo -v:q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "ModCheck のビルドに失敗しました" }
    $modResult = if ($UpdateBaseline) { Join-Path $baselineDir "mods.txt" } else { Join-Path $outDir "mods.txt" }
    & "$binDir\ModCheck\ModCheck.exe" $ppdBin $modResult @ScriptDirs
    if ($LASTEXITCODE -ne 0) {
        Write-Output "Mods: NG (読み込めないスクリプトあり。$modResult を確認してください)"
        $failed = $true
    }
    elseif (-not $UpdateBaseline) {
        $diff = Compare-Object (Get-Content (Join-Path $baselineDir "mods.txt") -Encoding UTF8) (Get-Content $modResult -Encoding UTF8)
        if ($diff) {
            Write-Output "Mods: NG (既定値の変換結果が基準と異なります)"
            $diff | Select-Object -First 20 | ForEach-Object { Write-Output ("  {0} {1}" -f $_.SideIndicator, $_.InputObject) }
            $failed = $true
        }
        else {
            Write-Output "Mods: OK"
        }
    }
}

if ($failed) { exit 1 }
