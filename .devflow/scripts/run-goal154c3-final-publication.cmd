@echo off
powershell -ExecutionPolicy Bypass -File "%~dp0run-goal154c3-final-publication.ps1" %*
exit /b %ERRORLEVEL%
