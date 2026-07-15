@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal156-seeded-generated-project.ps1" %*
exit /b %ERRORLEVEL%
