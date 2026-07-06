@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-gamepackage-candidate-recipe-pipeline.ps1" -ApplyCleanup %*
exit /b %ERRORLEVEL%
