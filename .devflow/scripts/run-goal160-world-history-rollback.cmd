@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal160-world-history-rollback.ps1" %*
exit /b %ERRORLEVEL%
