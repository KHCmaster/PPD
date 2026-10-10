@ECHO OFF
cd %~dp0

rem Usage: compile_shader.bat <ShaderName>   (e.g. compile_shader.bat AlphaBlend)
rem Expands DX11\<ShaderName>.fxt into DX11\<ShaderName>.fx and compiles it into DX11\d3d11_<ShaderName>.bin.
rem Build ShaderCompiler (Release|x64) beforehand.

"..\..\ShaderCompiler\bin\x64\Release\ShaderCompiler.exe" DX11\%1.fxt DX11\d3d11_%1.bin
if errorlevel 1 (
  echo Failure Reason Given is %errorlevel%
  pause
)
