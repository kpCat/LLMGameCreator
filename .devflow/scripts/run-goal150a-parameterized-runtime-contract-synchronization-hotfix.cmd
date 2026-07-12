@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal150a-parameterized-runtime-contract-synchronization-hotfix.ps1" %*
exit /b %errorlevel%
