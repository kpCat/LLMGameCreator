@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal169d-core-only-portable-closure.ps1" %*
exit /b %ERRORLEVEL%
