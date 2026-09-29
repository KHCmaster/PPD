# マルチプレイの通信データ（PPDMultiCommon の [MessagePackObject] 型）を MessagePack でシリアライズし、バイト列を書き出す。
# MessagePack の版を上げるときに、通信の互換（同じバイト列になること、古いバイト列を読めること）を確認するためのもの。
#
# 使い方:
#   基準を作る:  powershell -NoProfile -ExecutionPolicy Bypass -File Tools\ProjectCheck\MessagePackDump.ps1 -BinDir PPD\PPD\bin\x64\Debug\skins -Out Tools\ProjectCheck\messagepack_v1.txt
#   比べる:      powershell -NoProfile -ExecutionPolicy Bypass -File Tools\ProjectCheck\MessagePackDump.ps1 -BinDir <新しいビルド> -Compare Tools\ProjectCheck\messagepack_v1.txt
#   （BinDir には PPDMultiCommon.dll と MessagePack.dll があるフォルダーを指定する）
param(
    [Parameter(Mandatory = $true)][string]$BinDir,
    [string]$Out,
    [string]$Compare
)

$ErrorActionPreference = 'Stop'
$BinDir = (Resolve-Path $BinDir).Path
$script:dirs = @($BinDir, (Split-Path $BinDir))
[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($s, $e)
    $name = (New-Object Reflection.AssemblyName($e.Name)).Name
    foreach ($d in $script:dirs) {
        $c = Join-Path $d "$name.dll"
        if (Test-Path $c) { return [Reflection.Assembly]::LoadFrom($c) }
    }
    return $null
})
$common = [Reflection.Assembly]::LoadFrom((Join-Path $BinDir 'PPDMultiCommon.dll'))
$msgpack = [Reflection.Assembly]::LoadFrom((Join-Path $BinDir 'MessagePack.dll'))
$serializerType = $msgpack.GetType('MessagePack.MessagePackSerializer')
$serialize = $serializerType.GetMethods() | Where-Object { $_.Name -eq 'Serialize' -and $_.IsGenericMethodDefinition -and $_.ReturnType -eq [byte[]] -and $_.GetParameters()[0].ParameterType.IsGenericParameter } | Sort-Object { $_.GetParameters().Count } | Select-Object -First 1
$deserialize = $serializerType.GetMethods() | Where-Object { $_.Name -eq 'Deserialize' -and $_.IsGenericMethodDefinition -and ($_.GetParameters()[0].ParameterType -eq [byte[]] -or $_.GetParameters()[0].ParameterType.Name -eq 'ReadOnlyMemory`1') } | Sort-Object { $_.GetParameters().Count } | Select-Object -First 1

function Invoke-WithDefaults($method, $type, $first) {
    $generic = $method.MakeGenericMethod($type)
    $params = $generic.GetParameters()
    $callArgs = New-Object object[] $params.Count
    if ($first -is [psobject]) { $first = $first.PSObject.BaseObject }
    $callArgs[0] = $first
    for ($i = 1; $i -lt $params.Count; $i++) { $callArgs[$i] = if ($params[$i].HasDefaultValue) { $params[$i].DefaultValue } else { $null } }
    if ($params[0].ParameterType.Name -eq 'ReadOnlyMemory`1') { $callArgs[0] = [Activator]::CreateInstance($params[0].ParameterType, @(, [byte[]]$first)) }
    return $generic.Invoke($null, $callArgs)
}

function New-Sample([Type]$type, [int]$depth, [int]$seed) {
    if ($type -eq [string]) { return "テスト$seed" }
    if ($type -eq [int]) { return [int](1000 + $seed) }
    if ($type -eq [long]) { return [long](10000000000 + $seed) }
    if ($type -eq [int16]) { return [int16](100 + $seed) }
    if ($type -eq [byte]) { return [byte]($seed % 250) }
    if ($type -eq [float]) { return [single](1.5 + $seed) }
    if ($type -eq [double]) { return [double](2.25 + $seed) }
    if ($type -eq [bool]) { return ($seed % 2 -eq 0) }
    if ($type -eq [DateTime]) { return (New-Object DateTime 2020, 1, 2, 3, 4, 5, ([DateTimeKind]::Utc)).AddSeconds($seed) }
    if ($type -eq [TimeSpan]) { return [TimeSpan]::FromSeconds(90 + $seed) }
    if ($type -eq [Guid]) { return [Guid]'01234567-89ab-cdef-0123-456789abcdef' }
    if ($type.IsEnum) { $values = [Enum]::GetValues($type); return $values.GetValue($seed % $values.Length) }
    if ($type.IsGenericType -and $type.GetGenericTypeDefinition() -eq [Nullable`1]) { return New-Sample $type.GetGenericArguments()[0] $depth $seed }
    if ($type.IsArray) {
        $element = $type.GetElementType()
        $array = [Array]::CreateInstance($element, 2)
        for ($i = 0; $i -lt 2; $i++) { $array.SetValue((New-Sample $element ($depth + 1) ($seed + $i + 1)), $i) }
        return , $array
    }
    if ($type.IsGenericType -and $type.GetGenericTypeDefinition() -eq [Collections.Generic.List`1]) {
        $element = $type.GetGenericArguments()[0]
        $list = [Activator]::CreateInstance($type)
        for ($i = 0; $i -lt 2; $i++) { [void]$list.Add((New-Sample $element ($depth + 1) ($seed + $i + 1))) }
        return , $list
    }
    if ($type.IsGenericType -and $type.GetGenericTypeDefinition() -eq [Collections.Generic.Dictionary`2]) {
        $k = $type.GetGenericArguments()[0]; $v = $type.GetGenericArguments()[1]
        $dict = [Activator]::CreateInstance($type)
        $dict.Add((New-Sample $k ($depth + 1) ($seed + 1)), (New-Sample $v ($depth + 1) ($seed + 2)))
        return , $dict
    }
    if ($type.IsGenericType -and $type.FullName.StartsWith('System.Tuple`')) {
        $values = @(); $j = 0
        foreach ($t in $type.GetGenericArguments()) { $j++; $values += , (New-Sample $t ($depth + 1) ($seed + $j)) }
        return $type.GetConstructor($type.GetGenericArguments()).Invoke([object[]]$values)
    }
    if ($depth -gt 3) { return $null }
    if (-not $type.GetConstructor([Type[]]@())) { throw "no default constructor: $($type.FullName)" }
    $obj = [Activator]::CreateInstance($type)
    $i = 0
    foreach ($member in $type.GetMembers([Reflection.BindingFlags]'Public,Instance')) {
        $key = $member.GetCustomAttributes($true) | Where-Object { $_.GetType().Name -eq 'KeyAttribute' }
        if (-not $key) { continue }
        $i++
        if ($member -is [Reflection.PropertyInfo] -and $member.CanWrite -and $member.GetSetMethod()) { $member.SetValue($obj, (New-Sample $member.PropertyType ($depth + 1) ($seed + $i))) }
        elseif ($member -is [Reflection.FieldInfo]) { $member.SetValue($obj, (New-Sample $member.FieldType ($depth + 1) ($seed + $i))) }
    }
    return $obj
}

$types = $common.GetTypes() | Where-Object { -not $_.IsAbstract -and ($_.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'MessagePackObjectAttribute' }) } | Sort-Object FullName
$lines = New-Object System.Collections.Generic.List[string]
foreach ($type in $types) {
    $sample = New-Sample $type 0 0
    $bytes = [byte[]](Invoke-WithDefaults $serialize $type $sample)
    $lines.Add("$($type.FullName) $([BitConverter]::ToString($bytes).Replace('-', ''))")
}
"MessagePack $($msgpack.GetName().Version), types: $($types.Count)"

if ($Out) {
    [IO.File]::WriteAllLines((Join-Path (Get-Location) $Out), $lines, (New-Object Text.UTF8Encoding($false)))
    "written: $Out"
}
if ($Compare) {
    $expected = @{}
    foreach ($line in Get-Content $Compare) { $parts = $line.Split(' '); $expected[$parts[0]] = $parts[1] }
    $failures = 0
    foreach ($line in $lines) {
        $parts = $line.Split(' ')
        $name = $parts[0]
        if (-not $expected.ContainsKey($name)) { "NEW TYPE $name"; continue }
        if ($expected[$name] -ne $parts[1]) { "DIFF serialize $name"; "  expected $($expected[$name])"; "  actual   $($parts[1])"; $failures++ }
        # 古いバイト列を読み込んで、書き直したときに同じバイト列になるか（相手が古い版でも通信できるか）
        $hex = $expected[$name]
        $old = New-Object byte[] ($hex.Length / 2)
        for ($i = 0; $i -lt $old.Length; $i++) { $old[$i] = [Convert]::ToByte($hex.Substring($i * 2, 2), 16) }
        $type = $types | Where-Object { $_.FullName -eq $name }
        $roundTrip = [byte[]](Invoke-WithDefaults $serialize $type (Invoke-WithDefaults $deserialize $type $old))
        if ([BitConverter]::ToString($roundTrip).Replace('-', '') -ne $hex) { "DIFF roundtrip $name"; $failures++ }
    }
    foreach ($name in $expected.Keys) { if (-not ($lines | Where-Object { $_.StartsWith("$name ") })) { "MISSING TYPE $name"; $failures++ } }
    "failures: $failures"
    if ($failures) { exit 1 }
}
