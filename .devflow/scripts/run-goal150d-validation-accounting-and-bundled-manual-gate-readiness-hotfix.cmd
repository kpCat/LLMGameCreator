@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix.ps1" %*
exit /b %errorlevel%
