@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal153-active-abilities-mana-status-slice.ps1"
exit /b %ERRORLEVEL%
