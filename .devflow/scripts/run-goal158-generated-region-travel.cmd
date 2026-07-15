@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal158-generated-region-travel.ps1" %*
exit /b %ERRORLEVEL%
