@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%run-canonical-runtime-unity-player-loop-playback.ps1" -ApplyCleanup %*
exit /b %ERRORLEVEL%
