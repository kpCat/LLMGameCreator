@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix.ps1" %*
exit /b %ERRORLEVEL%
