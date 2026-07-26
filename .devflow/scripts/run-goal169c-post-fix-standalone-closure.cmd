@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal169c-post-fix-standalone-closure.ps1"
exit /b %errorlevel%
