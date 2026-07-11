@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-featuremodule-composer-scalability-hotfix.ps1" %*
exit /b %ERRORLEVEL%
