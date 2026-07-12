@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-capability-runtime-equipment-slice.ps1" %*
exit /b %errorlevel%
