@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal169b-package-bound-event-proof.ps1" %*
exit /b %ERRORLEVEL%
