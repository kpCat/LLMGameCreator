@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-featuremodule-composition-runtime-matrix.ps1" %*
exit /b %ERRORLEVEL%
