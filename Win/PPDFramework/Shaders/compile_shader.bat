@ECHO OFF
cd %~dp0

rem Usage: compile_shader.bat <ShaderName>   (e.g. compile_shader.bat AlphaBlend)
rem Expands DX11\<ShaderName>.fxt into DX11\<ShaderName>.fx and compiles it with fx_5_0.
rem Build ShaderCompiler (Release|x64) beforehand.

set FXC="%PROGRAMFILES(X86)%\Windows Kits\10\bin\10.0.26100.0\x64\fxc.exe"
if not exist %FXC% set FXC="%PROGRAMFILES(X86)%\Windows Kits\10\bin\x64\fxc.exe"

"..\..\ShaderCompiler\bin\x64\Release\ShaderCompiler.exe" DX11\%1.fxt
%FXC% /nologo DX11\%1.fx /T fx_5_0 /Fo DX11\d3d11_%1.fxc
if errorlevel 1 (
  echo Failure Reason Given is %errorlevel%
  pause
)
