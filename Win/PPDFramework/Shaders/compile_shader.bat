@ECHO OFF
cd %~dp0

"..\..\ShaderCompiler\bin\x86\Debug\ShaderCompiler.exe" DX9\%1.fxt
if errorlevel 0 (
  "%PROGRAMFILES% (x86)\Windows Kits\10\bin\x64\fxc.exe" DX9\%1.fx /T fx_2_0 /Fo DX9\d3d9_%1.fxc
  if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   pause
  )
)

"..\..\ShaderCompiler\bin\x86\Debug\ShaderCompiler.exe" DX11\%1.fxt
if errorlevel 0 (
  "%PROGRAMFILES% (x86)\Windows Kits\10\bin\x64\fxc.exe" DX11\%1.fx /T fx_5_0 /Fo DX11\d3d11_%1.fxc
  if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   pause
  )
)

