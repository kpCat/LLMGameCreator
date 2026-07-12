@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal151-real-saved-project-build-recovery-and-diagnostic-truth-hotfix.ps1" %*
exit /b %ERRORLEVEL%
