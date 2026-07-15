@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal155a-rc-record-truth.ps1"
exit /b %ERRORLEVEL%
