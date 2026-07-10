@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-selected-runtime-variant-playeradapter-handoff.ps1" %*
exit /b %ERRORLEVEL%
