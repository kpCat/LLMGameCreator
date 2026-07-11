@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-featuremodule-authoring-persistence-and-certification.ps1" %*
exit /b %ERRORLEVEL%
