@ECHO OFF
cd %~dp0
call compile_shader.bat Glass
call compile_shader.bat Border
call compile_shader.bat Mosaic
call compile_shader.bat GaussianFilter
call compile_shader.bat AlphaBlend
call compile_shader.bat BasicEffect