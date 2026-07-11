@echo off
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-goal148b-current-package-ui-thread-hotfix.ps1" %*
exit /b %errorlevel%
