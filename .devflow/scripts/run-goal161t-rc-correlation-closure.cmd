@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal161t-rc-correlation-closure.ps1" %*
exit /b %ERRORLEVEL%
