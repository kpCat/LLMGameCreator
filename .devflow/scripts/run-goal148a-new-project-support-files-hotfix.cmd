@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal148a-new-project-support-files-hotfix.ps1" %*
exit /b %errorlevel%
