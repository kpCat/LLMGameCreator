@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0run-goal154d-all-selected-quest-hotfix.ps1" %*
exit /b %ERRORLEVEL%
