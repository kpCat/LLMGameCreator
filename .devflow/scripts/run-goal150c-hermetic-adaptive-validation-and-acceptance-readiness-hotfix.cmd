@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix.ps1" %*
exit /b %errorlevel%
