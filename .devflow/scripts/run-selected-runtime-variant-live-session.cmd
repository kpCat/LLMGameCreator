@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-selected-runtime-variant-live-session.ps1" %*
exit /b %ERRORLEVEL%
