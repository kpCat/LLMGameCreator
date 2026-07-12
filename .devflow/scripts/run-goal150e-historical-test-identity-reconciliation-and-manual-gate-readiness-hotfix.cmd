@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1" %*
exit /b %errorlevel%
