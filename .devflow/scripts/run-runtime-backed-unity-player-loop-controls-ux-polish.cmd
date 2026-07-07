@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%run-runtime-backed-unity-player-loop-controls-ux-polish.ps1" -ApplyCleanup %*
exit /b %ERRORLEVEL%
