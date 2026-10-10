# アプリの出力フォルダーにあるアセンブリの参照を調べ、実際に置かれている版と違う版を参照しているのに
# .exe.config のバインディングリダイレクトで解決されないものを列挙する（.NET Framework 用）。
# skins から読み込まれるアセンブリ（PPDSingle、PPDMulti、PPDModSettingUI）は、PPD.exe の自動生成リダイレクトの対象にならないため確認が必要。
#
# 使い方:
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools\ProjectCheck\BindingCheck.ps1 -Exe PPD\PPD\bin\x64\Debug\PPD.exe -PluginDirs skins,dlls
param(
    [Parameter(Mandatory = $true)][string]$Exe,
    # 動的に読み込まれるプラグインのフォルダー（exe からの相対パス、カンマ区切り）。ここにある DLL も読み込まれるものとして扱う
    [string]$PluginDirs = ''
)

$ErrorActionPreference = 'Stop'
$Exe = (Resolve-Path $Exe).Path
$dir = Split-Path $Exe
$config = "$Exe.config"

# probing privatePath のフォルダーも含める
$searchDirs = @($dir)
$redirects = @{}
if (Test-Path $config) {
    [xml]$x = Get-Content $config -Raw
    $ns = New-Object Xml.XmlNamespaceManager($x.NameTable)
    $ns.AddNamespace('a', 'urn:schemas-microsoft-com:asm.v1')
    foreach ($probing in $x.SelectNodes('//a:probing', $ns)) {
        foreach ($p in $probing.privatePath.Split(';')) { $searchDirs += (Join-Path $dir $p) }
    }
    foreach ($dep in $x.SelectNodes('//a:dependentAssembly', $ns)) {
        $id = $dep.SelectSingleNode('a:assemblyIdentity', $ns)
        $br = $dep.SelectSingleNode('a:bindingRedirect', $ns)
        if ($id -and $br) {
            $range = $br.oldVersion.Split('-')
            $redirects[$id.name] = [pscustomobject]@{ Low = [version]$range[0]; High = [version]$range[-1]; New = [version]$br.newVersion }
        }
    }
}

$pluginFiles = foreach ($p in ($PluginDirs.Split(',') | Where-Object { $_ })) { $pd = Join-Path $dir $p; if (Test-Path $pd) { Get-ChildItem $pd -File -Filter *.dll | ForEach-Object { $_.FullName } } }
$job = Start-Job -ArgumentList (, $searchDirs), $Exe, (, @($pluginFiles)) -ScriptBlock {
    param($dirs, $exe, $plugins)
    $present = @{}
    $files = foreach ($d in $dirs) { if (Test-Path $d) { Get-ChildItem $d -File -ErrorAction SilentlyContinue | Where-Object { $_.Extension -in '.dll', '.exe' } } }
    foreach ($f in $files) {
        try { $name = [Reflection.AssemblyName]::GetAssemblyName($f.FullName) } catch { continue }
        if (-not $present.ContainsKey($name.Name)) { $present[$name.Name] = $name }
    }
    # exe とプラグインから、実際に読み込まれる可能性のあるアセンブリだけをたどる
    $paths = @{}
    foreach ($f in $files) { try { $n = [Reflection.AssemblyName]::GetAssemblyName($f.FullName).Name; if (-not $paths.ContainsKey($n)) { $paths[$n] = $f.FullName } } catch { } }
    $queue = New-Object System.Collections.Generic.Queue[string]
    $visited = @{}
    $queue.Enqueue($exe); foreach ($p in $plugins) { $queue.Enqueue($p) }
    while ($queue.Count) {
        $path = $queue.Dequeue()
        if ($visited.ContainsKey($path)) { continue }
        $visited[$path] = $true
        try { $asm = [Reflection.Assembly]::ReflectionOnlyLoadFrom($path) } catch { continue }
        foreach ($ref in $asm.GetReferencedAssemblies()) {
            if (-not $present.ContainsKey($ref.Name)) { continue }
            if ($present[$ref.Name].Version -ne $ref.Version) {
                [pscustomobject]@{ From = [IO.Path]::GetFileName($path); Name = $ref.Name; Wanted = $ref.Version; Present = $present[$ref.Name].Version }
            }
            $queue.Enqueue($paths[$ref.Name])
        }
    }
}
$mismatches = Receive-Job $job -Wait -AutoRemoveJob
$problems = 0
foreach ($m in $mismatches | Sort-Object Name, From -Unique) {
    $r = $redirects[$m.Name]
    $ok = $r -and $m.Wanted -ge $r.Low -and $m.Wanted -le $r.High -and $r.New -eq $m.Present
    if (-not $ok) {
        # 参照している版より新しい版が置かれていて、それが古い版と互換でなければ読み込みに失敗する
        "NG  $($m.From) -> $($m.Name) $($m.Wanted) (present $($m.Present))$(if ($r) { " redirect $($r.Low)-$($r.High) -> $($r.New)" })"
        $problems++
    }
}
"$([IO.Path]::GetFileName($Exe)): problems $problems"
if ($problems) { exit 1 }
