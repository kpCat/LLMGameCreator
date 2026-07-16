@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal161-generated-save-migration.ps1" %*
exit /b %errorlevel%
