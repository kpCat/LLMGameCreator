@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%run-canonical-runtime-player-command-loop.ps1" -ApplyCleanup %*
exit /b %ERRORLEVEL%
