@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal153a-parameter-domain-turn-atomicity-hotfix.ps1"
exit /b %ERRORLEVEL%
