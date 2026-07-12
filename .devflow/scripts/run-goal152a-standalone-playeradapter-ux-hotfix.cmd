@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal152a-standalone-playeradapter-ux-hotfix.ps1" %*
exit /b %ERRORLEVEL%
