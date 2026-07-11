@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal147a-authoring-and-certification-hotfix.ps1" %*
exit /b %ERRORLEVEL%
